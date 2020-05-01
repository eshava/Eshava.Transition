using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Eshava.Core.Extensions;
using Eshava.Transition.Enums;
using Eshava.Transition.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Models;
using Eshava.Transition.Models.CSV;

namespace Eshava.Transition.Engines
{
	public class CSVConverterEngine : AbstractConverterEngine, IConverterEngine
	{
		public ContentFormat ContentFormat => ContentFormat.Csv;

		public IEnumerable<T> Convert<T>(DataProperty configuration, string data, bool removeDublicates = true) where T : class, IEmpty
		{
			if (data.IsNullOrEmpty())
			{
				return new List<T>();
			}

			data = data.Replace("\r", "");

			var dataRows = data.Split('\n');
			var dataRecords = new List<T>();
			var settings = new CSVSettings
			{
				ColumnNames = GetColumnNames(dataRows, configuration.SeparatorCSVColumn, configuration.HasColumnNamesCSV, configuration.StartRowIndexCSV),
				Properties = configuration.DataProperties,
				Separator = configuration.SeparatorCSVColumn,
				CultureInfo = configuration.CultureCode.GetCultureInfo(),
				HasSurroundingQuotationMarks = configuration.HasSurroundingQuotationMarksCSV
			};

			var startRowIndex = configuration.StartRowIndexCSV + (configuration.HasColumnNamesCSV ? 1 : 0);
			for (var rowIndex = startRowIndex; rowIndex < dataRows.Length; rowIndex++)
			{
				settings.DataRow = dataRows[rowIndex];
				var dataRecord = ProcessDataRow<T>(settings);

				if (!(dataRecord?.IsEmpty ?? false))
				{
					dataRecords.Add(dataRecord);
				}
			}

			return removeDublicates ? RemoveDoublets(configuration, dataRecords) : dataRecords;
		}

		private Dictionary<string, int> GetColumnNames(string[] columnRows, char separator, bool hasColumnRow, int startRowIndex)
		{
			if (columnRows.Length == 0 || columnRows.Length <= startRowIndex)
			{
				throw new ArgumentException($"{nameof(columnRows)} must no be empty");
			}

			string[] columns = null;
			if (hasColumnRow)
			{
				columns = columnRows[startRowIndex].Replace("\n", "").Trim().Split(separator);
			}
			else
			{
				foreach (var columnRow in columnRows.Skip(startRowIndex))
				{
					var columnsCurrent = columnRow.Replace("\n", "").Trim().Split(separator);

					if (columns == null || columns.Length < columnsCurrent.Length)
					{
						columns = columnsCurrent;
					}
				}
			}

			var columnsDictionary = new Dictionary<string, int>();

			for (var i = 0; i < columns.Length; i++)
			{
				columnsDictionary.Add(hasColumnRow ? columns[i] : i.ToString(), i);
			}

			return columnsDictionary;
		}

		private T ProcessDataRow<T>(CSVSettings settings) where T : class
		{
			string[] dataCells;
			var dataRow = settings.DataRow.Replace("\n", "").Trim();
			if (settings.HasSurroundingQuotationMarks)
			{
				dataCells = dataRow.Split('\"').Where(part => part != settings.Separator.ToString()).Skip(1).ToArray();
				if (dataCells.Last().IsNullOrEmpty() && !dataRow.EndsWith(settings.Separator.ToString()))
				{
					dataCells = dataCells.Take(dataCells.Length - 1).ToArray();
				}
			}
			else
			{
				dataCells = dataRow.Split(settings.Separator);
			}

			var dataRecordSettings = new CSVSettingsDataRecord
			{
				ColumnNames = settings.ColumnNames,
				Properties = settings.Properties,
				DataCells = dataCells,
				DataRecord = null,
				DataRecordType = typeof(T),
				CultureInfo = settings.CultureInfo
			};

			return ProcessDataRowForDataRecordType(dataRecordSettings) as T;
		}

		private object ProcessDataRowForDataRecordType(CSVSettingsDataRecord settings)
		{
			if (settings.DataRecord == null)
			{
				settings.DataRecord = Activator.CreateInstance(settings.DataRecordType);
			}

			var propertyInfos = settings.DataRecord.GetType().GetProperties();

			foreach (var propertyInfo in propertyInfos)
			{
				var propertyTargets = settings.Properties.Where(p => p.PropertyTarget.Equals(propertyInfo.Name));

				if (!propertyTargets.Any())
				{
					continue;
				}

				ProcessPropertyInfo(propertyTargets, propertyInfo, settings);
			}

			return settings.DataRecord;
		}

		private void ProcessPropertyInfo(IEnumerable<DataProperty> propertyTargets, PropertyInfo propertyInfo, CSVSettingsDataRecord settings)
		{
			foreach (var propertyTarget in propertyTargets)
			{
				if (propertyTarget.HasMapping)
				{
					SetPropertyValue(propertyInfo, settings.DataRecord, propertyTarget.MappedValue, settings.CultureInfo);
				}
				else
				{
					var columnIndex = settings.ColumnNames.ContainsKey(propertyTarget.PropertySource ?? "") ? settings.ColumnNames[propertyTarget.PropertySource] : -1;

					if (columnIndex >= settings.DataCells.Length)
					{
						continue;
					}

					if (columnIndex == -1)
					{
						CheckAndProcessDefaultColumnIndex(settings, propertyTarget, propertyInfo);

						continue;
					}

					var rawValue = settings.DataCells[columnIndex];
					if (propertyTarget.ValueMappings != null)
					{
						var mapping = propertyTarget.ValueMappings.FirstOrDefault(m => m.Source.Equals(rawValue, StringComparison.OrdinalIgnoreCase));

						if (!mapping.Source.IsNullOrEmpty())
						{
							rawValue = mapping.Target;
						}
					}

					SetPropertyValue(propertyInfo, settings.DataRecord, rawValue, settings.CultureInfo);
				}
			}
		}

		private bool CheckAndProcessDefaultColumnIndex(CSVSettingsDataRecord settings, DataProperty propertyTarget, PropertyInfo propertyInfo)
		{
			if ((!(propertyTarget.DataProperties?.Any() ?? false) || !CheckIfClass(propertyInfo)) && !CheckIfIEnumerable(propertyInfo))
			{
				return false;
			}

			var dataRecordSettings = new CSVSettingsDataRecord
			{
				ColumnNames = settings.ColumnNames,
				Properties = propertyTarget.DataProperties,
				DataCells = settings.DataCells,
				CultureInfo = settings.CultureInfo
			};

			return CheckAndProcessEnumerables(dataRecordSettings, propertyInfo, settings.DataRecord) || CheckAndProcessDataClasses(dataRecordSettings, propertyInfo, settings.DataRecord);
		}

		private bool CheckAndProcessEnumerables(CSVSettingsDataRecord settings, PropertyInfo propertyInfo, object dataRecord)
		{
			if (CheckIfIEnumerable(propertyInfo))
			{
				var dataRecordEnumerable = propertyInfo.GetValue(dataRecord);

				if (dataRecordEnumerable == null)
				{
					dataRecordEnumerable = CreateIEnumerableInstance(propertyInfo.PropertyType);
					propertyInfo.SetValue(dataRecord, dataRecordEnumerable);
				}

				settings.DataRecord = null;
				settings.DataRecordType = propertyInfo.PropertyType.GetGenericArguments()[0];

				var childDataRecord = ProcessDataRowForDataRecordType(settings) as IEmpty;

				if (!(childDataRecord?.IsEmpty ?? false))
				{
					dataRecordEnumerable.GetType().GetMethod("Add").Invoke(dataRecordEnumerable, new[] { childDataRecord });
				}

				return true;
			}

			return false;
		}

		private bool CheckAndProcessDataClasses(CSVSettingsDataRecord settings, PropertyInfo propertyInfo, object dataRecord)
		{
			if (CheckIfClass(propertyInfo))
			{
				settings.DataRecord = propertyInfo.GetValue(dataRecord);
				settings.DataRecordType = propertyInfo.PropertyType;

				var childDataRecord = ProcessDataRowForDataRecordType(settings);

				if (childDataRecord != null)
				{
					propertyInfo.SetValue(dataRecord, childDataRecord);
				}

				return true;
			}

			return false;
		}


		public IEnumerable<string> Convert<T>(DataProperty configuration, IEnumerable<T> data) where T : class
		{
			if (data == null || !data.Any())
			{
				return Array.Empty<string>();
			}

			var columnHeaderRow = GetColumnHeaderRow(configuration);
			var dataRows = new List<string[]>();
			var cultureInfo = configuration.CultureCode.GetCultureInfo();

			if (configuration.HasColumnNamesCSV)
			{
				dataRows.Add(columnHeaderRow);
			}

			foreach (var dataItem in data)
			{
				dataRows.AddRange(ProcessDataRecord(dataItem, configuration, columnHeaderRow.Length, cultureInfo, configuration.HasSurroundingQuotationMarksCSV));
			}

			return new[] { TransformDataRowArraysToDataRowString(dataRows, configuration.SeparatorCSVColumn) };
		}

		private IEnumerable<string[]> ProcessDataRecord<T>(T dataRecord, DataProperty dataProperty, int columnCount, CultureInfo cultureInfo, bool hasSurroundingQuotationMarks) where T : class
		{
			return ProcessDataRecord(dataRecord, dataProperty, new string[columnCount], cultureInfo, hasSurroundingQuotationMarks);
		}

		private IEnumerable<string[]> ProcessDataRecord(object dataRecord, DataProperty dataProperty, string[] dataRow, CultureInfo cultureInfo, bool hasSurroundingQuotationMarks)
		{
			var originDataRow = dataRow;
			var dataRows = new List<string[]>();
			var subDataRows = new List<string[]>();

			if (dataProperty.DataProperties?.Any() ?? false)
			{
				var propertyInfos = dataRecord.GetType().GetProperties();

				foreach (var childDataProperty in dataProperty.DataProperties)
				{
					var propertyInfo = propertyInfos.SingleOrDefault(p => p.Name.Equals(childDataProperty.PropertyTarget));

					if (propertyInfo != null)
					{
						var settings = new CSVExportSettings
						{
							DataRecord = dataRecord,
							DataRow = dataRow,
							DataProperty = childDataProperty,
							PropertyInfo = propertyInfo,
							CultureInfo = cultureInfo,
							HasSurroundingQuotationMarks = hasSurroundingQuotationMarks
						};

						if (CheckIfIEnumerable(propertyInfo))
						{
							dataRow = ProcessEnumerableDataProperty(settings, originDataRow, subDataRows);
						}
						else if (CheckIfClass(propertyInfo))
						{
							dataRow = ProcessClassDataProperty(settings);
						}
						else if (!childDataProperty.HasMapping)
						{
							settings.PropertyInfos = propertyInfos;
							ProcessPrimitiveDataProperty(settings);
						}
					}
				}

				dataRows.AddRange(CombineDataRows(dataRow, subDataRows));
			}

			return dataRows;
		}

		private IEnumerable<string[]> CombineDataRows(string[] dataRow, IEnumerable<string[]> subDataRows)
		{
			var dataRows = new List<string[]>();

			if (subDataRows.Any())
			{
				foreach (var subDataRow in subDataRows)
				{
					var dataRowClone = dataRow.ToArray();
					for (var i = 0; i < dataRowClone.Length; i++)
					{
						if (!subDataRow[i].IsNullOrEmpty())
						{
							dataRowClone[i] = subDataRow[i];
						}
					}
					dataRows.Add(dataRowClone);
				}
			}
			else
			{
				dataRows.Add(dataRow);
			}

			return dataRows;
		}

		private void ProcessPrimitiveDataProperty(CSVExportSettings settings)
		{
			if (!CheckConditionalDataProperty(settings.DataRecord, settings.DataProperty, settings.PropertyInfos))
			{
				return;
			}

			var rawValue = GetRawValue(settings.DataRecord, settings.PropertyInfo, settings.CultureInfo) ?? "";
			if (!rawValue.IsNullOrEmpty())
			{
				rawValue = CheckAndApplyMapping(rawValue, settings.DataProperty);
			}

			if (settings.HasSurroundingQuotationMarks)
			{
				rawValue = $"\"{rawValue}\"";
			}

			settings.DataRow[settings.DataProperty.PropertySourceIndexCSV] = rawValue;
		}

		private string[] ProcessClassDataProperty(CSVExportSettings settings)
		{
			var dataRecordClass = settings.PropertyInfo.GetValue(settings.DataRecord);
			if (dataRecordClass != null)
			{
				settings.DataRow = ProcessDataRecord(dataRecordClass, settings.DataProperty, settings.DataRow, settings.CultureInfo, settings.HasSurroundingQuotationMarks).First();
			}

			return settings.DataRow;
		}

		private string[] ProcessEnumerableDataProperty(CSVExportSettings settings, string[] originDataRow, List<string[]> subDataRows)
		{
			var dataRecordEnumerable = settings.PropertyInfo.GetValue(settings.DataRecord) as System.Collections.IEnumerable;

			if (dataRecordEnumerable != null)
			{
				foreach (var subItem in dataRecordEnumerable)
				{
					var currentDataRow = settings.DataProperty.IsSameDataRecord ? settings.DataRow : originDataRow.ToArray();
					var enumerableResult = ProcessDataRecord(subItem, settings.DataProperty, currentDataRow, settings.CultureInfo, settings.HasSurroundingQuotationMarks);

					if (settings.DataProperty.IsSameDataRecord)
					{
						settings.DataRow = enumerableResult.First();
					}
					else
					{
						subDataRows.AddRange(enumerableResult);
					}
				}
			}

			return settings.DataRow;
		}

		private string[] GetColumnHeaderRow(DataProperty configuration)
		{
			var columnIndices = new Dictionary<int, string>();
			GetColumnIndices(configuration, columnIndices);

			var columns = new string[columnIndices.Keys.Max() + 1];

			for (var index = 0; index < columns.Length; index++)
			{
				if (columnIndices.ContainsKey(index))
				{
					columns[index] = columnIndices[index];
				}
			}

			return columns;
		}

		private void GetColumnIndices(DataProperty dataProperty, Dictionary<int, string> columnIndices)
		{
			if (dataProperty.PropertySourceIndexCSV >= 0 && !columnIndices.ContainsKey(dataProperty.PropertySourceIndexCSV))
			{
				columnIndices.Add(dataProperty.PropertySourceIndexCSV, dataProperty.PropertySource);
			}

			if (dataProperty.DataProperties?.Any() ?? false)
			{
				foreach (var childDataProperty in dataProperty.DataProperties)
				{
					GetColumnIndices(childDataProperty, columnIndices);
				}
			}
		}

		private string TransformDataRowArraysToDataRowString(List<string[]> dataRows, char separator)
		{
			var stringBuilder = new StringBuilder();
			dataRows.ForEach(row => stringBuilder.AppendLine(String.Join(separator.ToString(), row)));

			return stringBuilder.ToString().TrimEnd(Environment.NewLine.ToArray());
		}
	}
}