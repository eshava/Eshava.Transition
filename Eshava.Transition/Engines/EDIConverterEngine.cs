using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Eshava.Core.Extensions;
using Eshava.Transition.Enums;
using Eshava.Transition.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Interfaces.EDI;
using Eshava.Transition.Models;
using Eshava.Transition.Models.EDI;

namespace Eshava.Transition.Engines
{
	public class EDIConverterEngine : AbstractConverterEngine, IConverterEngine
	{
		public ContentFormat ContentFormat => ContentFormat.Edi;

		public IEnumerable<T> Convert<T>(DataProperty configuration, string data, bool removeDublicates = true) where T : class, IEmpty
		{
			if (data.IsNullOrEmpty())
			{
				return new List<T>();
			}

			data = data.Replace("\r", "");

			var dataRows = data.Split('\n');
			var dataRecords = ProcessRow<T>(configuration, dataRows);

			return removeDublicates ? RemoveDoublets(configuration, dataRecords) : dataRecords;
		}

		private List<T> ProcessRow<T>(DataProperty configuration, string[] dataRows)
		{
			var dataRecords = new List<T>();

			var settings = new EdiDataPropertySettings
			{
				DataRows = dataRows,
				DataProperty = configuration,
				DataRecordType = typeof(T),
				StartRowIndex = 0
			};

			var items = ProcessDataProperty(settings, configuration.CultureCode.GetCultureInfo());

			foreach (var item in items)
			{
				dataRecords.Add((T)item);
			}

			return dataRecords;
		}

		private IEnumerable<object> ProcessDataProperty(EdiDataPropertySettings dataPropertySettings, CultureInfo cultureInfo)
		{
			var offsetRowIndex = 0;
			var loopCount = 0;
			var endOfFile = false;
			var dataRecords = new List<object>();

			if (!(dataPropertySettings.DataProperty.DataProperties?.Any() ?? false))
			{
				return dataRecords;
			}

			do
			{
				var currentStartRowIndex = dataPropertySettings.StartRowIndex + loopCount * offsetRowIndex;

				if (dataPropertySettings.DataRows.Length > currentStartRowIndex)
				{
					var dataRecord = Activator.CreateInstance(dataPropertySettings.DataRecordType);
					offsetRowIndex = CalculateRowIndexOffset(dataPropertySettings.DataProperty, offsetRowIndex);

					var settings = new EdiSettings
					{
						DataRows = dataPropertySettings.DataRows,
						DataRecord = dataRecord,
						DataType = dataPropertySettings.DataRecordType,
						CurrentStartRowIndex = currentStartRowIndex,
						CultureInfo = cultureInfo
					};

					foreach (var dataProperty in dataPropertySettings.DataProperty.DataProperties)
					{
						settings.DataProperty = dataProperty;
						endOfFile = ProcessPropertyInfo(settings);
					}

					dataRecords.Add(dataRecord);
				}
				else
				{
					endOfFile = true;
				}

				loopCount++;
			}
			while (dataPropertySettings.DataProperty.CanRepeatEDI && !endOfFile);

			return dataRecords;
		}

		private bool ProcessPropertyInfo(EdiSettings settings)
		{
			settings.PropertyInfo = settings.DataType.GetProperty(settings.DataProperty.PropertyTarget);
			var endOfFile = false;

			if (settings.DataProperty.HasMapping)
			{
				SetPropertyValue(settings.PropertyInfo, settings.DataRecord, settings.DataProperty.MappedValue, settings.CultureInfo);
			}
			else if (CheckIfIEnumerable(settings.PropertyInfo))
			{
				ProcessEnumerableProperty(settings);
			}
			else if (CheckIfClass(settings.PropertyInfo))
			{
				ProcessClassProperty(settings);
			}
			else
			{
				endOfFile = ProcessPrimitiveDataTypeProperty(settings);
			}

			return endOfFile;
		}

		private int CalculateRowIndexOffset(DataProperty dataProperty, int offsetRowIndex)
		{
			if (dataProperty.CanRepeatEDI && offsetRowIndex <= 0)
			{
				var rowIndexRange = GetRowIndexRange(dataProperty, -1, -1);
				offsetRowIndex = 1 + rowIndexRange.MaxIndex - rowIndexRange.MinIndex;
			}

			return offsetRowIndex;
		}

		private void ProcessClassProperty(EdiSettings settings)
		{
			var dataPropertySettings = new EdiDataPropertySettings
			{
				DataRows = settings.DataRows,
				DataProperty = settings.DataProperty,
				DataRecordType = settings.PropertyInfo.PropertyType,
				StartRowIndex = settings.CurrentStartRowIndex
			};

			var child = ProcessDataProperty(dataPropertySettings, settings.CultureInfo).FirstOrDefault();
			if (child != null && !(child as IEmpty).IsEmpty)
			{
				SetPropertyValue(settings.PropertyInfo, settings.DataRecord, child, settings.CultureInfo);
			}
		}

		private bool ProcessPrimitiveDataTypeProperty(EdiSettings settings)
		{
			var currentRowIndex = settings.CurrentStartRowIndex + settings.DataProperty.LineIndexEDI;
			var endOfFile = false;

			if (settings.DataRows.Length > currentRowIndex)
			{
				var rawValue = GetValue(settings.DataRows[currentRowIndex], settings.DataProperty.PositionEDI, settings.DataProperty.LengthEDI).Trim();

				if (rawValue.IsNullOrEmpty())
				{
					return endOfFile;
				}

				if (settings.DataProperty.ValueMappings != null)
				{
					var mapping = settings.DataProperty.ValueMappings.FirstOrDefault(m =>
						(m.Source.IsNullOrEmpty() && rawValue.IsNullOrEmpty())
						|| (!m.Source.IsNullOrEmpty() && m.Source.Equals(rawValue, StringComparison.InvariantCultureIgnoreCase))
					);

					if (!mapping.Source.IsNullOrEmpty())
					{
						rawValue = mapping.Target;
					}
				}

				SetPropertyValue(settings.PropertyInfo, settings.DataRecord, rawValue, settings.CultureInfo);
			}
			else
			{
				endOfFile = true;
			}

			return endOfFile;
		}

		private void ProcessEnumerableProperty(EdiSettings settings)
		{
			var dataRecordEnumerable = InitDataRecordEnumerable(settings.DataRecord, settings.PropertyInfo);

			var dataPropertySettings = new EdiDataPropertySettings
			{
				DataRows = settings.DataRows,
				DataProperty = settings.DataProperty,
				DataRecordType = settings.PropertyInfo.PropertyType.GetGenericArguments()[0],
				StartRowIndex = settings.CurrentStartRowIndex
			};

			var childs = ProcessDataProperty(dataPropertySettings, settings.CultureInfo);
			ProcessEnumerablePropertyResult(childs, dataRecordEnumerable);
		}

		private (int MinIndex, int MaxIndex) GetRowIndexRange(DataProperty configuration, int minRowIndex, int maxRowIndex)
		{
			if (configuration.LengthEDI > 0)
			{
				minRowIndex = minRowIndex > -1 ? Math.Min(minRowIndex, configuration.LineIndexEDI) : configuration.LineIndexEDI;
				maxRowIndex = Math.Max(maxRowIndex, configuration.LineIndexEDI);
			}

			if (configuration.DataProperties?.Any() ?? false)
			{
				foreach (var property in configuration.DataProperties)
				{
					(minRowIndex, maxRowIndex) = GetRowIndexRange(property, minRowIndex, maxRowIndex);
				}
			}

			return (minRowIndex, maxRowIndex);
		}

		private string GetValue(string line, int position, int lenght)
		{
			if (line.Length <= position)
			{
				return "";
			}

			var result = line.Substring(position);

			if (result.Length > lenght)
			{
				return result.Substring(0, lenght);
			}

			return result;
		}

		public IEnumerable<string> Convert<T>(DataProperty configuration, IEnumerable<T> data) where T : class
		{
			if (data == null || !data.Any())
			{
				return Array.Empty<string>();
			}

			var dataRows = new List<string>();
			var cultureInfo = configuration.CultureCode.GetCultureInfo();

			if (configuration.CanRepeatEDI)
			{
				dataRows.Add(ProcessDataRecords<T>(data, configuration, cultureInfo));
			}
			else
			{
				foreach (var dataItem in data)
				{
					dataRows.Add(ProcessDataRecord<T>(dataItem, configuration, cultureInfo));
				}
			}

			return dataRows;
		}

		private string ProcessDataRecord<T>(T dataRecord, DataProperty dataProperty, CultureInfo cultureInfo) where T : class
		{
			var maxLineLength = GetMaximumLineLength(dataProperty);
			var maxLineCount = GetMaxLineCount(dataProperty);
			var dataRecordSettings = new EdiDataRecordSettings
			{
				DataRecord = dataRecord,
				DataProperty = dataProperty,
				MaxLineLength = maxLineLength,
				MaxLineCount = maxLineCount,
				CultureInfo = cultureInfo
			};

			var dataRows = ProcessDataRecord(dataRecordSettings).ToList();

			return TransformDataRowArraysToDataRowString(dataRows);
		}

		private string ProcessDataRecords<T>(IEnumerable<T> dataRecords, DataProperty dataProperty, CultureInfo cultureInfo) where T : class
		{
			var dataRows = new List<char[]>();
			var maxLineLength = GetMaximumLineLength(dataProperty);
			var maxLineCount = GetMaxLineCount(dataProperty);

			var dataRecordSettings = new EdiDataRecordSettings
			{
				DataProperty = dataProperty,
				MaxLineLength = maxLineLength,
				MaxLineCount = maxLineCount,
				CultureInfo = cultureInfo
			};

			foreach (var dataRecord in dataRecords)
			{
				dataRecordSettings.DataRecord = dataRecord;
				dataRows.AddRange(ProcessDataRecord(dataRecordSettings));
			}

			return TransformDataRowArraysToDataRowString(dataRows);
		}

		private IEnumerable<char[]> ProcessDataRecord(EdiDataRecordSettings dataRecordSettings)
		{
			var dataRows = new List<char[]>();
			var subDataRows = new List<IEnumerable<char[]>>();

			var propertyInfos = dataRecordSettings.DataRecord.GetType().GetProperties();

			foreach (var childDataProperty in dataRecordSettings.DataProperty.DataProperties)
			{
				var propertyInfo = propertyInfos.SingleOrDefault(pi => pi.Name.Equals(childDataProperty.PropertyTarget));

				if (propertyInfo != null)
				{
					var subDataRecordSettings = new EdiSubDataRecordSettings
					{
						DataRecord = dataRecordSettings.DataRecord,
						DataProperty = childDataProperty,
						PropertyInfo = propertyInfo,
						PropertyInfos = propertyInfos,
						TargetDataRows = dataRows,
						SubDataRows = subDataRows,
						MaxLineLength = dataRecordSettings.MaxLineLength,
						MaxLineCount = dataRecordSettings.MaxLineCount
					};

					if (CheckIfIEnumerable(propertyInfo))
					{
						dataRows = ProcessEnumerableDataProperty(subDataRecordSettings, dataRecordSettings.CultureInfo);
					}
					else if (CheckIfClass(propertyInfo))
					{
						dataRows = ProcessClassDataProperty(subDataRecordSettings, dataRecordSettings.CultureInfo);
					}
					else if (!childDataProperty.HasMapping)
					{
						ProcessPrimitiveDataProperty(subDataRecordSettings, dataRecordSettings.CultureInfo);
					}
				}

				dataRows = CombineDataRows(dataRows, subDataRows).ToList();
			}

			return dataRows;
		}

		private IEnumerable<char[]> CombineDataRows(List<char[]> dataRows, List<IEnumerable<char[]>> subDataRows)
		{
			var combinedDataRows = new List<char[]>();

			if (subDataRows.Any())
			{
				foreach (var subDataRow in subDataRows)
				{
					var subDataRowArray = subDataRow.ToArray();
					var dataRowsClone = dataRows.Select(r => r.ToArray()).ToArray();

					combinedDataRows.AddRange(CombineDataRows(dataRowsClone, subDataRowArray));
				}
			}
			else
			{
				combinedDataRows.AddRange(dataRows);
			}

			return combinedDataRows;
		}

		private IEnumerable<char[]> CombineDataRows(IList<char[]> dataRows, IEnumerable<char[]> subDataRow)
		{
			if (!subDataRow.Any())
			{
				return dataRows;
			}

			var subDataRowArray = subDataRow.ToArray();
			var dataRowClones = dataRows.Select(r => r.ToArray()).ToList();

			for (var rowIndex = 0; rowIndex < dataRows.Count; rowIndex++)
			{
				var dataRowClone = dataRowClones[rowIndex];
				for (var cellIndex = 0; cellIndex < dataRowClone.Count(); cellIndex++)
				{
					var subValue = subDataRowArray[rowIndex][cellIndex];
					if (subValue != ' ')
					{
						dataRowClone[cellIndex] = subValue;
					}
				}
			}

			for (var i = dataRows.Count; i < subDataRowArray.Length; i++)
			{
				dataRowClones.Add(subDataRowArray[i]);
			}

			return dataRowClones;
		}

		private void ProcessPrimitiveDataProperty(IEdiPrimitiveDataSettings settings, CultureInfo cultureInfo)
		{
			if (!CheckConditionalDataProperty(settings.DataRecord, settings.DataProperty, settings.PropertyInfos))
			{
				return;
			}

			var rawValue = GetRawValue(settings.DataRecord, settings.PropertyInfo, cultureInfo);
			if (!rawValue.IsNullOrEmpty())
			{
				rawValue = CheckAndApplyMapping(rawValue, settings.DataProperty);

				while (settings.TargetDataRows.Count < (settings.DataProperty.LineIndexEDI + 1))
				{
					settings.TargetDataRows.Add(InitialiseDataRow(settings.MaxLineLength));
				}

				var dataRow = settings.TargetDataRows[settings.DataProperty.LineIndexEDI];
				var fieldLength = settings.DataProperty.LengthEDI > rawValue.Length ? rawValue.Length : settings.DataProperty.LengthEDI;

				for (var i = 0; i < fieldLength; i++)
				{
					dataRow[i + settings.DataProperty.PositionEDI] = rawValue[i];
				}
			}
		}

		private List<char[]> ProcessClassDataProperty(IEdiSubDataRecordSettings settings, CultureInfo cultureInfo)
		{
			var dataRecordClass = settings.PropertyInfo.GetValue(settings.DataRecord);
			if (dataRecordClass != null)
			{
				var dataRecordSettings = new EdiDataRecordSettings
				{
					DataRecord = dataRecordClass,
					DataProperty = settings.DataProperty,
					MaxLineLength = settings.MaxLineLength,
					MaxLineCount = settings.MaxLineCount,
					CultureInfo = cultureInfo
				};

				var classResult = ProcessDataRecord(dataRecordSettings).ToList();
				settings.TargetDataRows = CombineDataRows(settings.TargetDataRows, classResult).ToList();
			}

			return settings.TargetDataRows;
		}

		private List<char[]> ProcessEnumerableDataProperty(IEdiSubDataRecordEnumerableSettings settings, CultureInfo cultureInfo)
		{
			var dataRecordEnumerable = settings.PropertyInfo.GetValue(settings.DataRecord) as System.Collections.IEnumerable;

			if (dataRecordEnumerable != null)
			{
				(var minIndex, var maxIndex) = GetRowIndexRange(settings.DataProperty, -1, -1);

				var dataRecordSettings = new EdiDataRecordSettings
				{
					DataProperty = settings.DataProperty,
					MaxLineLength = settings.MaxLineLength,
					MaxLineCount = settings.MaxLineCount,
					CultureInfo = cultureInfo
				};

				foreach (var subItem in dataRecordEnumerable)
				{
					dataRecordSettings.DataRecord = subItem;
					var enumerableResult = ProcessDataRecord(dataRecordSettings);

					if (settings.DataProperty.IsSameDataRecord)
					{
						settings.TargetDataRows = CombineDataRows(settings.TargetDataRows, enumerableResult).ToList();
					}
					else if (settings.DataProperty.CanRepeatEDI)
					{
						var resultSet = new EnumerableResultSet
						{
							TargetDataRows = settings.TargetDataRows,
							ResultdataRows = enumerableResult,
							MinLineIndex = minIndex,
							MaxLineIndex = maxIndex,
							MaxLineLength = settings.MaxLineLength
						};
						AddEnumerableResultToDataRows(resultSet);
					}
					else
					{
						settings.SubDataRows.Add(enumerableResult);
					}
				}
			}

			return settings.TargetDataRows;
		}

		private List<char[]> AddEnumerableResultToDataRows(EnumerableResultSet enumerableResultSet)
		{
			var arrayResult = enumerableResultSet.ResultdataRows.ToArray();
			while (enumerableResultSet.TargetDataRows.Count < enumerableResultSet.MinLineIndex)
			{
				enumerableResultSet.TargetDataRows.Add(InitialiseDataRow(enumerableResultSet.MaxLineLength));
			}

			for (var index = enumerableResultSet.MinLineIndex; index <= enumerableResultSet.MaxLineIndex; index++)
			{
				if (arrayResult.Length > index)
				{
					enumerableResultSet.TargetDataRows.Add(arrayResult[index]);
				}
				else
				{
					enumerableResultSet.TargetDataRows.Add(InitialiseDataRow(enumerableResultSet.MaxLineLength));
				}
			}

			return enumerableResultSet.TargetDataRows;
		}

		private char[] InitialiseDataRow(int length)
		{
			var dataRow = new char[length];
			for (var i = 0; i < length; i++)
			{
				dataRow[i] = ' ';
			}

			return dataRow;
		}

		private int GetMaximumLineLength(DataProperty dataProperty)
		{
			var max = dataProperty.LengthEDI + dataProperty.PositionEDI;

			if (dataProperty.DataProperties != null)
			{
				max = Math.Max(max, dataProperty.DataProperties.Max(GetMaximumLineLength));
			}

			return max;
		}

		private int GetMaxLineCount(DataProperty dataProperty)
		{
			var max = dataProperty.LineIndexEDI;

			if (dataProperty.DataProperties != null)
			{
				max = Math.Max(max, dataProperty.DataProperties.Max(GetMaxLineCount));
			}

			return max;
		}

		private string TransformDataRowArraysToDataRowString(List<char[]> dataRows)
		{
			var dataRow = new StringBuilder();

			dataRows.ForEach(row => dataRow.AppendLine(new string(row)));

			return dataRow.ToString().TrimEnd(Environment.NewLine.ToArray());
		}
	}
}