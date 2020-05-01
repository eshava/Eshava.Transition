using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Transition.Enums;
using Eshava.Transition.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Models;
using Eshava.Transition.Models.JSON;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eshava.Transition.Engines
{
	public class JSONConverterEngine : AbstractRawDataConverterEngine<JSONSettings, JToken>, IConverterEngine
	{
		private static readonly Type _typeString = typeof(string);

		public ContentFormat ContentFormat => ContentFormat.Json;

		public IEnumerable<T> Convert<T>(DataProperty configuration, string data, bool removeDublicates = true) where T : class, IEmpty
		{
			if (data.IsNullOrEmpty())
			{
				return new List<T>();
			}

			var jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

			List<T> dataRecords;
			if (jsonData is JObject)
			{
				var jsonObject = jsonData as JObject;
				if (jsonObject.First is JProperty && jsonObject.First.Children().FirstOrDefault() is JArray)
				{
					dataRecords = ProcessRow<T>(configuration, jsonObject.First);
				}
				else
				{
					// process single data object
					dataRecords = ProcessRow<T>(configuration, jsonObject);
				}
			}
			else if (jsonData is JArray)
			{
				var jsonObject = jsonData as JArray;
				dataRecords = ProcessRow<T>(configuration, jsonObject);
			}
			else
			{
				dataRecords = new List<T>();
			}

			return removeDublicates ? RemoveDoublets(configuration, dataRecords) : dataRecords;
		}

		private List<T> ProcessRow<T>(DataProperty configuration, JToken dataObject)
		{
			JArray jToken;
			if (dataObject is JProperty && !dataObject.Path.IsNullOrEmpty() && dataObject.Path.Equals(configuration.PropertySource))
			{
				jToken = dataObject.Children().First() as JArray;
			}
			else if (dataObject is JArray && configuration.PropertySource.IsNullOrEmpty())
			{
				jToken = dataObject as JArray;
			}
			else if (dataObject is JObject)
			{
				jToken = new JArray(dataObject);
			}
			else
			{
				return new List<T>();
			}

			var dataRecords = new List<T>();
			var settings = new JSONSettings
			{
				DataProperty = configuration,
				RawDataNode = jToken,
				DataType = typeof(T),
				CultureInfo = configuration.CultureCode.GetCultureInfo()
			};
			var items = ProcessDataProperty(settings);

			foreach (var item in items)
			{
				dataRecords.Add((T)item);
			}

			return dataRecords;
		}

		protected override IEnumerable<object> ProcessDataProperty(JSONSettings settings)
		{
			var dataRecords = new List<object>();

			if (settings.RawDataNode is JArray)
			{
				var dataArray = settings.RawDataNode as JArray;

				foreach (var dataObject in dataArray)
				{
					dataRecords.Add(ProcessDataProperty(settings, dataObject));
				}
			}
			else
			{
				dataRecords.Add(ProcessDataProperty(settings, settings.RawDataNode));
			}

			return dataRecords;
		}

		protected override JToken GetRawDataForClassProperty(JToken rawData)
		{
			if (rawData is JProperty)
			{
				return rawData.First;
			}

			return rawData;
		}

		private object ProcessDataProperty(JSONSettings settings, JToken dataObject)
		{
			var nodeSettings = new JSONSettings
			{
				DataType = settings.DataType,
				DataRecord = Activator.CreateInstance(settings.DataType),
				CultureInfo = settings.CultureInfo
			};

			ProcessJToken(settings.DataProperty.DataProperties, dataObject, nodeSettings);

			return nodeSettings.DataRecord;
		}

		private void ProcessJToken(IEnumerable<DataProperty> dataProperties, JToken rawDataNode, JSONSettings nodeSettings)
		{
			foreach (var dataProperty in dataProperties)
			{
				nodeSettings.DataProperty = dataProperty;

				if (dataProperty.PropertySource.IsNullOrEmpty())
				{
					nodeSettings.RawDataNode = rawDataNode;
				}
				else
				{
					nodeSettings.RawDataNode = rawDataNode.Children().FirstOrDefault(t => ((JProperty)t).Name.Equals(dataProperty.PropertySource));
				}

				if (dataProperty.PropertyTarget.IsNullOrEmpty())
				{
					if (!dataProperty.DataProperties.Any())
					{
						continue;
					}

					foreach (var dataPropertyChild in dataProperty.DataProperties)
					{
						var childSettings = new JSONSettings
						{
							DataProperty = dataPropertyChild,
							RawDataNode = nodeSettings.RawDataNode.First().Children().FirstOrDefault(t => ((JProperty)t).Name.Equals(dataPropertyChild.PropertySource)),
							DataType = nodeSettings.DataType,
							DataRecord = nodeSettings.DataRecord,
							CultureInfo = nodeSettings.CultureInfo
						};

						ProcessPropertyInfo(childSettings);
					}
				}
				else
				{
					ProcessPropertyInfo(nodeSettings);
				}
			}
		}

		protected override void ProcessEnumerableProperty(JSONSettings settings)
		{
			//The enumerable object had to be always initialized 
			var dataRecordEnumerable = InitDataRecordEnumerable(settings);

			if (settings.RawDataNode == null)
			{
				return;
			}

			var childSettings = new JSONSettings
			{
				DataProperty = settings.DataProperty,
				RawDataNode = settings.RawDataNode.First,
				DataType = settings.PropertyInfo.PropertyType.GetGenericArguments()[0],
				CultureInfo = settings.CultureInfo
			};

			ProcessEnumerableProperty(childSettings, dataRecordEnumerable);
		}

		protected override string GetValue(JToken rawDataNode)
		{
			return ((JValue)((JProperty)rawDataNode).Value).Value.ToString();
		}


		public IEnumerable<string> Convert<T>(DataProperty configuration, IEnumerable<T> data) where T : class
		{
			if (data == null || !data.Any())
			{
				return Array.Empty<string>();
			}

			var jArray = new JArray();

			foreach (var dataRecord in data)
			{
				var jObject = ProcessDataRecord(dataRecord, configuration);
				if (jObject != null)
				{
					jArray.Add(jObject);
				}
			}

			var cultureInfo = configuration.CultureCode.GetCultureInfo();

			if (configuration.PropertySource.IsNullOrEmpty())
			{
				if (configuration.SplitExportResult)
				{
					return jArray.Select(item => SerializeObject(item, cultureInfo)).ToList();
				}

				return new List<string> { SerializeObject(jArray, cultureInfo) };
			}

			var wrapperObject = new JObject
			{
				new JProperty(configuration.PropertySource, jArray)
			};

			return new List<string> { SerializeObject(wrapperObject, cultureInfo) };
		}

		private string SerializeObject(JToken jToken, CultureInfo cultureInfo)
		{
			return JsonConvert.SerializeObject(jToken, Formatting.Indented, new JsonSerializerSettings
			{
				Culture = cultureInfo
			});
		}

		private JObject ProcessDataRecord<T>(T dataRecord, DataProperty configuration)
		{
			return ProcessDataRecord(new JSONSettings { DataRecord = dataRecord, DataProperty = configuration, CultureInfo = configuration.CultureCode.GetCultureInfo() });
		}

		private JObject ProcessDataRecord(JSONSettings parentSettings)
		{
			var propertyInfos = parentSettings.DataRecord.GetType().GetProperties();
			var resultObject = new JObject();

			foreach (var childDataProperty in parentSettings.DataProperty.DataProperties)
			{
				var propertyInfo = propertyInfos.SingleOrDefault(pi => pi.Name.Equals(childDataProperty.PropertyTarget));

				var settings = new JSONSettings
				{
					DataProperty = childDataProperty,
					PropertyInfo = propertyInfo,
					PropertyInfos = propertyInfos,
					DataRecord = parentSettings.DataRecord,
					RawDataNode = resultObject,
					CultureInfo = parentSettings.CultureInfo
				};

				if (propertyInfo != null)
				{
					if (CheckIfIEnumerable(propertyInfo))
					{
						BuildAndAddJProperty(settings, resultObject, ProcessEnumerableDataProperty);
					}
					else if (CheckIfClass(propertyInfo))
					{
						BuildAndAddJProperty(settings, resultObject, ProcessClassDataProperty);
					}
					else if (!childDataProperty.HasMapping)
					{
						ProcessPrimitiveDataProperty(settings);
					}
				}
				else if (!childDataProperty.PropertySource.IsNullOrEmpty())
				{
					BuildAndAddJProperty(settings, resultObject, ProcessDataRecord);
				}
			}

			return resultObject;
		}

		private void BuildAndAddJProperty(JSONSettings settings, JObject targetObject, Func<JSONSettings, JToken> buildPropertyContent)
		{
			var propertyContent = buildPropertyContent(settings);
			if (propertyContent == null)
			{
				return;
			}

			if (settings.DataProperty.PropertySource.IsNullOrEmpty())
			{
				foreach (var jProperty in (propertyContent as JObject).Children().Where(p => p is JProperty))
				{
					targetObject.Add(jProperty);
				}
			}
			else
			{
				targetObject.Add(new JProperty(settings.DataProperty.PropertySource, propertyContent));
			}
		}

		private void ProcessPrimitiveDataProperty(JSONSettings settings)
		{
			if (!CheckConditionalDataProperty(settings.DataRecord, settings.DataProperty, settings.PropertyInfos))
			{
				return;
			}

			var rawValueBoxed = settings.PropertyInfo.GetValue(settings.DataRecord);
			if (rawValueBoxed == null)
			{
				return;
			}

			if (rawValueBoxed.GetType() == _typeString)
			{
				rawValueBoxed = CheckAndApplyMapping(rawValueBoxed.ToString(), settings.DataProperty);
			}
			else if (settings.DataProperty.ExportAsString)
			{
				rawValueBoxed = System.Convert.ToString(rawValueBoxed, settings.CultureInfo);
			}

			var resultObject = settings.RawDataNode as JObject;
			var property = resultObject.Children().SingleOrDefault(c => c.Type == JTokenType.Property && ((JProperty)c).Name == settings.DataProperty.PropertySource) as JProperty;
			if (property != null)
			{
				resultObject.Remove(settings.DataProperty.PropertySource);
			}

			resultObject.Add(new JProperty(settings.DataProperty.PropertySource, rawValueBoxed));
		}

		private JObject ProcessClassDataProperty(JSONSettings settings)
		{
			var dataRecordClass = settings.PropertyInfo.GetValue(settings.DataRecord);

			return ProcessDataRecord(new JSONSettings { DataRecord = dataRecordClass, DataProperty = settings.DataProperty, CultureInfo = settings.CultureInfo });
		}

		private JArray ProcessEnumerableDataProperty(JSONSettings settings)
		{
			var result = new JArray();
			var dataRecordEnumerable = settings.PropertyInfo.GetValue(settings.DataRecord) as System.Collections.IEnumerable;

			if (dataRecordEnumerable != null)
			{
				foreach (var subItem in dataRecordEnumerable)
				{
					var resultItem = ProcessDataRecord(new JSONSettings { DataRecord = subItem, DataProperty = settings.DataProperty, CultureInfo = settings.CultureInfo });
					if (resultItem != null)
					{
						result.Add(resultItem);
					}
				}
			}

			return result.Count > 0 ? result : null;
		}
	}
}