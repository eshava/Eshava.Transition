using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Eshava.Core.Extensions;
using Eshava.Transition.Enums;
using Eshava.Transition.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Models;
using Eshava.Transition.Models.XML;

namespace Eshava.Transition.Engines
{
	public class XMLConverterEngine : AbstractRawDataConverterEngine<XMLSettings, XmlNode>, IConverterEngine
	{
		public ContentFormat ContentFormat => ContentFormat.Xml;

		public IEnumerable<T> Convert<T>(DataProperty configuration, string data, bool removeDublicates = true) where T : class, IEmpty
		{
			if (data.IsNullOrEmpty())
			{
				return new List<T>();
			}

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(data);
			var xmlRoots = xmlDocument.SelectNodes(configuration.PropertySource);

			if (xmlRoots.Count != 1)
			{
				return new List<T>();
			}

			var dataRecords = ProcessRow<T>(configuration.DataProperties.First(), xmlRoots[0], configuration.CultureCode.GetCultureInfo());

			return removeDublicates ? RemoveDoublets(configuration.DataProperties.First(), dataRecords) : dataRecords;
		}

		private List<T> ProcessRow<T>(DataProperty configuration, XmlNode rootNode, CultureInfo cultureInfo)
		{
			var dataRecords = new List<T>();
			var settings = new XMLSettings
			{
				DataProperty = configuration,
				RawDataNode = rootNode,
				DataType = typeof(T),
				CultureInfo = cultureInfo
			};
			var items = ProcessDataProperty(settings);

			foreach (var item in items)
			{
				dataRecords.Add((T)item);
			}

			return dataRecords;
		}

		protected override IEnumerable<object> ProcessDataProperty(XMLSettings settings)
		{
			var dataRecords = new List<object>();

			if (settings.DataProperty.PropertySource.IsNullOrEmpty())
			{
				dataRecords.Add(ProcessDataProperty(settings, settings.RawDataNode));
			}
			else
			{
				var rawData = settings.RawDataNode.SelectNodes(settings.DataProperty.PropertySource);
				foreach (XmlNode rawDataNode in rawData)
				{
					dataRecords.Add(ProcessDataProperty(settings, rawDataNode));
				}
			}

			return dataRecords;
		}

		private object ProcessDataProperty(XMLSettings settings, XmlNode rawDataNode)
		{
			var isClass = CheckIfClass(settings.DataType);
			var nodeSettings = new XMLSettings
			{
				DataType = settings.DataType,
				DataRecord = isClass ? Activator.CreateInstance(settings.DataType) : null,
				CultureInfo = settings.CultureInfo
			};

			if (isClass)
			{
				ProcessXmlNode(settings.DataProperty.DataProperties, rawDataNode, nodeSettings);

				return nodeSettings.DataRecord;
			}

			object value = null;
			nodeSettings.RawDataNode = rawDataNode;
			nodeSettings.DataProperty = settings.DataProperty;
			ProcessPrimitiveDataTypeProperty(nodeSettings, (s, rawValue) => { value = System.Convert.ChangeType(rawValue, s.DataType, s.CultureInfo); });

			return value;
		}

		private void ProcessXmlNode(IEnumerable<DataProperty> dataProperties, XmlNode rawDataNode, XMLSettings nodeSettings)
		{
			foreach (var dataProperty in dataProperties)
			{
				nodeSettings.DataProperty = dataProperty;

				if (dataProperty.PropertySource.IsNullOrEmpty())
				{
					nodeSettings.RawDataNode = rawDataNode;
				}
				else if (dataProperty.PropertySource.ToLower() == rawDataNode.Name.ToLower())
				{
					nodeSettings.RawDataNode = rawDataNode;
				}
				else
				{
					nodeSettings.RawDataNode = rawDataNode.SelectSingleNode(dataProperty.PropertySource);
				}

				if (dataProperty.PropertyTarget.IsNullOrEmpty())
				{
					if (!dataProperty.DataProperties.Any())
					{
						continue;
					}

					foreach (var dataPropertyChild in dataProperty.DataProperties)
					{
						var childSettings = new XMLSettings
						{
							DataProperty = dataPropertyChild,
							RawDataNode = nodeSettings.RawDataNode.SelectSingleNode(dataPropertyChild.PropertySource),
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

		protected override void ProcessEnumerableProperty(XMLSettings settings)
		{
			//The enumerable object had to be always initialized 
			var dataRecordEnumerable = InitDataRecordEnumerable(settings);

			if (settings.RawDataNode == null)
			{
				return;
			}

			foreach (var dataPropertyChild in settings.DataProperty.DataProperties)
			{
				var childSettings = new XMLSettings
				{
					DataProperty = dataPropertyChild,
					RawDataNode = settings.RawDataNode,
					DataType = settings.PropertyInfo.PropertyType.GetGenericArguments()[0],
					CultureInfo = settings.CultureInfo
				};

				ProcessEnumerableProperty(childSettings, dataRecordEnumerable);
			}
		}

		protected override string GetValue(XmlNode rawDataNode)
		{
			return rawDataNode.InnerText;
		}


		public IEnumerable<string> Convert<T>(DataProperty configuration, IEnumerable<T> data) where T : class
		{
			if (data == null || !data.Any())
			{
				return Array.Empty<string>();
			}

			var document = new XmlDocument();
			document.LoadXml($"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><{configuration.PropertySource} />");
			var xmlNRoot = document.LastChild;
			var result = new List<string>();
			var replaceRootNode = configuration.DataProperties.First().PropertySource.IsNullOrEmpty();

			foreach (var dataRecord in data)
			{
				var xmlNode = ProcessDataRecord(dataRecord, configuration.DataProperties.First(), document, configuration.CultureCode.GetCultureInfo());
				if (xmlNode != null)
				{
					if (replaceRootNode)
					{
						document.ReplaceChild(xmlNode, document.LastChild);
						xmlNRoot = document.LastChild;
					}
					else
					{
						xmlNRoot.AppendChild(xmlNode);
					}

					if (configuration.SplitExportResult)
					{
						result.Add(document.OuterXml);
						xmlNRoot.RemoveAll();
					}
				}
			}

			if (!configuration.SplitExportResult)
			{
				result.Add(document.OuterXml);
			}

			return result;
		}

		private XmlNode ProcessDataRecord<T>(T dataRecord, DataProperty dataProperty, XmlDocument document, CultureInfo cultureInfo)
		{
			return ProcessDataRecord(new XMLSettings { DataRecord = dataRecord, DataProperty = dataProperty, Document = document, CultureInfo = cultureInfo });
		}

		private XmlNode ProcessDataRecord(XMLSettings parentSettings)
		{
			var propertyInfos = parentSettings.DataRecord.GetType().GetProperties();
			var extractFirstChild = false;
			XmlNode xmlNode;
			if (parentSettings.DataProperty.PropertySource.IsNullOrEmpty() && parentSettings.RawDataNode == null)
			{
				extractFirstChild = true;
				xmlNode = parentSettings.Document.CreateNode(XmlNodeType.Element, "Dummy", null);
			}
			else if (parentSettings.DataProperty.PropertySource.IsNullOrEmpty())
			{
				xmlNode = parentSettings.RawDataNode;
			}
			else
			{
				xmlNode = parentSettings.Document.CreateNode(XmlNodeType.Element, parentSettings.DataProperty.PropertySource, null);
			}

			var additionAttributes = GetAdditionalPropertyData(AdditionalPropertyDataType.Attribute, parentSettings.DataProperty);
			AddAttributesToNode(parentSettings.Document, xmlNode, additionAttributes);

			foreach (var childDataProperty in parentSettings.DataProperty.DataProperties)
			{
				var propertyInfo = propertyInfos.SingleOrDefault(pi => pi.Name.Equals(childDataProperty.PropertyTarget));

				var settings = new XMLSettings
				{
					Document = parentSettings.Document,
					DataProperty = childDataProperty,
					PropertyInfo = propertyInfo,
					PropertyInfos = propertyInfos,
					DataRecord = parentSettings.DataRecord,
					RawDataNode = xmlNode,
					CultureInfo = parentSettings.CultureInfo
				};

				if (propertyInfo != null)
				{
					if (CheckIfIEnumerable(propertyInfo))
					{
						BuildAndAddPropertyNodes(settings, xmlNode, ProcessEnumerableDataProperty);
					}
					else if (CheckIfClass(propertyInfo))
					{
						BuildAndAddPropertyNodes(settings, xmlNode, ProcessClassDataProperty);
					}
					else if (!childDataProperty.HasMapping)
					{
						ProcessPrimitiveDataProperty(settings);
					}
				}
				else if (!childDataProperty.PropertySource.IsNullOrEmpty())
				{
					BuildAndAddPropertyNodes(settings, xmlNode, ProcessDataRecord);
				}
			}

			return extractFirstChild ? xmlNode.FirstChild : xmlNode;
		}

		private void BuildAndAddPropertyNodes(XMLSettings settings, XmlNode parentNode, Func<XMLSettings, XmlNode> buildPropertyNode)
		{
			var propertyNode = buildPropertyNode(settings);
			if (propertyNode == null)
			{
				return;
			}

			var additionAttributes = GetAdditionalPropertyData(AdditionalPropertyDataType.Attribute, settings.DataProperty);
			AddAttributesToNode(settings.Document, propertyNode, additionAttributes);

			if (settings.DataProperty.PropertySource.IsNullOrEmpty())
			{
				foreach (XmlNode childPropertyNode in propertyNode.ChildNodes)
				{
					parentNode.AppendChild(childPropertyNode);
				}
			}
			else
			{
				parentNode.AppendChild(propertyNode);
			}
		}

		private void ProcessPrimitiveDataProperty(XMLSettings settings)
		{
			if (!CheckConditionalDataProperty(settings.DataRecord, settings.DataProperty, settings.PropertyInfos))
			{
				return;
			}

			var rawValue = GetRawValue(settings.DataRecord, settings.PropertyInfo, settings.CultureInfo);
			ProcessPrimitiveDataProperty(settings, rawValue);
		}

		private void ProcessPrimitiveDataProperty(XMLSettings settings, string rawValue)
		{
			var additionAttributes = GetAdditionalPropertyData(AdditionalPropertyDataType.Attribute, settings.DataProperty);

			if (rawValue.IsNullOrEmpty() && !additionAttributes.Any())
			{
				return;
			}

			var propertyNode = settings.Document.CreateNode(XmlNodeType.Element, settings.DataProperty.PropertySource, null);
			if (!rawValue.IsNullOrEmpty())
			{
				rawValue = CheckAndApplyMapping(rawValue, settings.DataProperty);
				propertyNode.InnerText = rawValue;
			}

			AddAttributesToNode(settings.Document, propertyNode, additionAttributes);

			settings.RawDataNode.AppendChild(propertyNode);
		}

		private XmlNode ProcessClassDataProperty(XMLSettings settings)
		{
			var dataRecordClass = settings.PropertyInfo.GetValue(settings.DataRecord);

			return ProcessDataRecord(new XMLSettings { DataRecord = dataRecordClass, DataProperty = settings.DataProperty, Document = settings.Document, RawDataNode = settings.RawDataNode, CultureInfo = settings.CultureInfo });
		}

		private XmlNode ProcessEnumerableDataProperty(XMLSettings settings)
		{
			var result = settings.Document.CreateNode(XmlNodeType.Element, settings.DataProperty.PropertySource, null);
			var dataRecordEnumerableRaw = settings.PropertyInfo.GetValue(settings.DataRecord);
			var dataRecordEnumerable = dataRecordEnumerableRaw as System.Collections.IEnumerable;

			if (dataRecordEnumerable != null)
			{
				var subItemType = settings.PropertyInfo.PropertyType.GetDataTypeFromIEnumerable();

				foreach (var subItem in dataRecordEnumerable)
				{
					if (CheckIfClass(subItemType))
					{
						var resultItem = ProcessDataRecord(new XMLSettings { DataRecord = subItem, DataProperty = settings.DataProperty.DataProperties.First(), Document = settings.Document, CultureInfo = settings.CultureInfo });
						if (resultItem != null)
						{
							result.AppendChild(resultItem);
						}
					}
					else
					{
						var subSettings = new XMLSettings
						{
							Document = settings.Document,
							DataProperty = settings.DataProperty.DataProperties.First(),
							PropertyInfo = settings.PropertyInfo,
							PropertyInfos = null,
							DataRecord = subItem,
							RawDataNode = result,
							CultureInfo = settings.CultureInfo
						};

						var rawValue = GetRawValue(subItemType, subItem, settings.CultureInfo);
						ProcessPrimitiveDataProperty(subSettings, rawValue);
					}
				}
			}

			return result.ChildNodes.Count > 0 ? result : null;
		}

		private IEnumerable<AdditionalPropertyData> GetAdditionalPropertyData(AdditionalPropertyDataType type, DataProperty dataProperty)
		{
			return dataProperty.AdditionalPropertyData
				?.Where(data => data.Type == type)
				?? Array.Empty<AdditionalPropertyData>();
		}

		private void AddAttributesToNode(XmlDocument document, XmlNode node, IEnumerable<AdditionalPropertyData> additionAttributes)
		{
			if (additionAttributes.Any())
			{
				foreach (var additionAttribute in additionAttributes)
				{
					var attribute = document.CreateAttribute(additionAttribute.Name);
					attribute.Value = additionAttribute.Value;
					node.Attributes.Append(attribute);
				}
			}
		}
	}
}