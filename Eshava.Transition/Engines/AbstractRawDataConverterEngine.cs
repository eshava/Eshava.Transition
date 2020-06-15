using System;
using System.Collections.Generic;
using System.Linq;
using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Models;

namespace Eshava.Transition.Engines
{
	public abstract class AbstractRawDataConverterEngine<SettingType, RawDataType> : AbstractConverterEngine where SettingType : AbstractSettings<RawDataType>, new()
	{
		protected void ProcessPropertyInfo(SettingType settings)
		{
			settings.PropertyInfo = settings.DataType.GetProperty(settings.DataProperty.PropertyTarget);

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
			else if (settings.RawDataNode != null)
			{
				ProcessPrimitiveDataTypeProperty(settings);
			}
		}

		protected object InitDataRecordEnumerable(SettingType settings)
		{
			return InitDataRecordEnumerable(settings.DataRecord, settings.PropertyInfo);
		}

		protected void ProcessEnumerableProperty(SettingType settings, object dataRecordEnumerable)
		{
			var childs = ProcessDataProperty(settings);
			ProcessEnumerablePropertyResult(childs, dataRecordEnumerable);
		}

		protected abstract IEnumerable<object> ProcessDataProperty(SettingType settings);
		protected abstract void ProcessEnumerableProperty(SettingType settings);
		protected abstract string GetValue(RawDataType rawDataNode);

		protected virtual RawDataType GetRawDataForClassProperty(RawDataType rawData)
		{
			return rawData;
		}

		private void ProcessClassProperty(SettingType settings)
		{
			var classSettings = new SettingType
			{
				DataType = settings.PropertyInfo.PropertyType,
				RawDataNode = GetRawDataForClassProperty(settings.RawDataNode),
				DataProperty = settings.DataProperty
			};

			var child = ProcessDataProperty(classSettings).FirstOrDefault();
			if (child != null && !(child as IEmpty).IsEmpty)
			{
				SetPropertyValue(settings.PropertyInfo, settings.DataRecord, child, settings.CultureInfo);
			}
		}

		private void ProcessPrimitiveDataTypeProperty(SettingType settings)
		{
			ProcessPrimitiveDataTypeProperty(settings, (s, rawValue) => SetPropertyValue(s.PropertyInfo, s.DataRecord, rawValue, s.CultureInfo));
		}

		protected void ProcessPrimitiveDataTypeProperty(SettingType settings, Action<SettingType, string> setPropertyValue)
		{
			var rawValue = GetValue(settings.RawDataNode).Trim();

			if (rawValue.IsNullOrEmpty())
			{
				return;
			}

			if (settings.DataProperty.ValueMappings != null)
			{
				var mapping = settings.DataProperty.ValueMappings.FirstOrDefault(m => m.Source.Equals(rawValue, StringComparison.OrdinalIgnoreCase));

				if (mapping != null && !mapping.Source.IsNullOrEmpty())
				{
					rawValue = mapping.Target;
				}
			}

			setPropertyValue(settings, rawValue);
		}
	}
}