using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Eshava.Core.Extensions;
using Eshava.Transition.Extensions;
using Eshava.Transition.Interfaces;
using Eshava.Transition.Models;

namespace Eshava.Transition.Engines
{
	public abstract class AbstractConverterEngine
	{
		protected IEnumerable<T> RemoveDoublets<T>(DataProperty configuration, IEnumerable<T> dataRecords) where T : class
		{
			if (!(configuration.MappingProperties?.Any() ?? false))
			{
				return dataRecords;
			}

			var type = typeof(T);
			var resultDataRecords = new List<T>();
			var propertyInfos = new List<PropertyInfo>();
			var propertyInfosIEnumerable = type.GetProperties().Where(p => CheckIfIEnumerable(p)).ToList();
			var propertyInfosIClass = type.GetProperties().Where(p => CheckIfClass(p)).ToList();

			foreach (var mappingProperty in configuration.MappingProperties)
			{
				var propertyInfo = type.GetProperty(mappingProperty);
				if (propertyInfo != null)
				{
					propertyInfos.Add(propertyInfo);
				}
			}

			foreach (var dataRecord in dataRecords)
			{
				var existingDataRecord = resultDataRecords.SingleOrDefault(d => propertyInfos.All(p => Equals(p.GetValue(d), p.GetValue(dataRecord))));

				if (existingDataRecord == null)
				{
					resultDataRecords.Add(dataRecord);
				}
				else
				{
					CompareEnumerableProperties(configuration, propertyInfosIEnumerable, existingDataRecord, dataRecord);
					CompareClassProperties(configuration, propertyInfosIClass, existingDataRecord, dataRecord);
				}
			}

			return resultDataRecords;
		}

		protected bool CheckIfClass(PropertyInfo propertyInfo)
		{
			return CheckIfClass(propertyInfo.PropertyType);
		}

		protected bool CheckIfClass(Type type)
		{
			return type.IsClass
				&& !Equals(type, typeof(string))
				&& !type.ImplementsIEnumerable()
				;
		}

		protected bool CheckIfIEnumerable(PropertyInfo propertyInfo)
		{
			if (propertyInfo == null)
			{
				return false;
			}

			return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.ImplementsIEnumerable();
		}

		protected object CreateIEnumerableInstance(Type dataType)
		{
			var type = typeof(List<>);
			var typeArgs = new Type[] { dataType.GetGenericArguments()[0] };
			var constructed = type.MakeGenericType(typeArgs);

			return Activator.CreateInstance(constructed);
		}

		protected void SetPropertyValue(PropertyInfo propertyInfo, object dataRecord, object rawValue, CultureInfo cultureInfo)
		{
			if (propertyInfo.PropertyType.IsDateTime())
			{
				if (DateTime.TryParse(rawValue?.ToString(), cultureInfo, DateTimeStyles.None, out var datetime))
				{
					propertyInfo.SetValue(dataRecord, datetime);
				}

				return;
			}

			var value = Convert.ChangeType(rawValue, propertyInfo.PropertyType, cultureInfo);
			propertyInfo.SetValue(dataRecord, value);
		}

		protected object InitDataRecordEnumerable(object dataRecord, PropertyInfo propertyInfo)
		{
			var dataRecordEnumerable = propertyInfo.GetValue(dataRecord);

			if (dataRecordEnumerable == null)
			{
				dataRecordEnumerable = CreateIEnumerableInstance(propertyInfo.PropertyType);
				propertyInfo.SetValue(dataRecord, dataRecordEnumerable);
			}

			return dataRecordEnumerable;
		}

		protected void ProcessEnumerablePropertyResult(IEnumerable<object> childs, object dataRecordEnumerable)
		{
			var methodInfoAdd = dataRecordEnumerable.GetType().GetMethod("Add");

			foreach (var child in childs)
			{
				var isIEmpty = child.GetType().ImplementsInterface(typeof(IEmpty));

				if ((isIEmpty && !(child as IEmpty).IsEmpty) || (!isIEmpty && child != null))
				{
					methodInfoAdd.Invoke(dataRecordEnumerable, new[] { child });
				}
			}
		}

		private void CompareClassProperties(DataProperty configurationParent, List<PropertyInfo> propertyInfosClass, object existingDataRecord, object newDataRecord)
		{
			foreach (var propertyInfo in propertyInfosClass)
			{
				var dataProperties = configurationParent.DataProperties.Where(p => p.PropertyTarget?.Equals(propertyInfo.Name) ?? false).ToList();
				var existingChild = propertyInfo.GetValue(existingDataRecord);
				var newChild = propertyInfo.GetValue(newDataRecord);

				var propertyInfosIEnumerableChild = propertyInfo.PropertyType.GetProperties().Where(p => CheckIfIEnumerable(p)).ToList();
				var propertyInfosClassChild = propertyInfo.PropertyType.GetProperties().Where(p => CheckIfClass(p)).ToList();

				dataProperties.ForEach(dp =>
				{
					CompareEnumerableProperties(dp, propertyInfosIEnumerableChild, existingChild, newChild);
					CompareClassProperties(dp, propertyInfosClassChild, existingChild, newChild);
				});
			}
		}

		private void CompareEnumerableProperties(DataProperty configurationParent, List<PropertyInfo> propertyInfosIEnumerable, object existingDataRecord, object newDataRecord)
		{
			foreach (var propertyInfo in propertyInfosIEnumerable)
			{
				var existingEnumerableObject = propertyInfo.GetValue(existingDataRecord);
				var newEnumerableObject = propertyInfo.GetValue(newDataRecord);
				var dataProperties = configurationParent.DataProperties.Where(p => Equals(p.PropertyTarget, propertyInfo.Name)).ToList();
				var mappings = dataProperties.SelectMany(p => p.MappingProperties).Distinct().ToList();

				var existingEnumerable = (IEnumerable)existingEnumerableObject;
				var newEnumerable = (IEnumerable)newEnumerableObject;
				var childsToAdd = new List<object>();

				if (mappings.Any())
				{
					childsToAdd.AddRange(CompareListEntries(dataProperties, mappings, existingEnumerable, newEnumerable));
				}
				else
				{
					foreach (var newItem in newEnumerable)
					{
						childsToAdd.Add(newItem);
					}
				}

				if (childsToAdd.Any())
				{
					childsToAdd.ForEach(item => existingEnumerableObject.GetType().GetMethod("Add").Invoke(existingEnumerableObject, new[] { item }));
				}
			}
		}

		private List<object> CompareListEntries(List<DataProperty> dataProperties, List<string> mappings, IEnumerable existingEnumerable, IEnumerable newEnumerable)
		{
			var itemsToAdd = new List<object>();
			List<PropertyInfo> propertyInfosMapping = null;
			List<PropertyInfo> propertyInfosIEnumerableItem = null;
			List<PropertyInfo> propertyInfosIClassItem = null;

			foreach (var newItem in newEnumerable)
			{
				if (propertyInfosMapping == null)
				{
					propertyInfosMapping = newItem.GetType().GetProperties().Where(p => mappings.Contains(p.Name)).ToList();
					propertyInfosIEnumerableItem = newItem.GetType().GetProperties().Where(p => CheckIfIEnumerable(p)).ToList();
					propertyInfosIClassItem = newItem.GetType().GetProperties().Where(p => CheckIfClass(p)).ToList();
				}

				var newItemFoundInExistingItems = false;
				foreach (var existingItem in existingEnumerable)
				{
					if (propertyInfosMapping.All(p => Equals(p.GetValue(existingItem), p.GetValue(newItem))))
					{
						dataProperties.ForEach(dp =>
						{
							CompareEnumerableProperties(dp, propertyInfosIEnumerableItem, existingItem, newItem);
							CompareClassProperties(dp, propertyInfosIClassItem, existingItem, newItem);
						});

						newItemFoundInExistingItems = true;

						break;
					}
				}

				if (!newItemFoundInExistingItems)
				{
					itemsToAdd.Add(newItem);
				}
			}

			return itemsToAdd;
		}

		protected bool CheckConditionalDataProperty(object dataRecord, DataProperty childDataPropery, PropertyInfo[] propertyInfos)
		{
			if (!childDataPropery.ConditionalPropertyName.IsNullOrEmpty() && !childDataPropery.ConditionalPropertyValue.IsNullOrEmpty())
			{
				var conditionalValue = propertyInfos.SingleOrDefault(p => p.Name.Equals(childDataPropery.ConditionalPropertyName))?.GetValue(dataRecord)?.ToString();

				if (!childDataPropery.ConditionalPropertyValue.Equals(conditionalValue))
				{
					return false;
				}
			}

			return true;
		}

		protected string GetRawValue(object dataRecord, PropertyInfo propertyInfo, CultureInfo cultureInfo)
		{
			var rawValueBoxed = propertyInfo.GetValue(dataRecord);

			return GetRawValue(propertyInfo.PropertyType, rawValueBoxed, cultureInfo);
		}

		protected string GetRawValue(Type type, object rawValueBoxed, CultureInfo cultureInfo)
		{
			string rawValue;
			if (rawValueBoxed != null && type.IsInteger() || type.IsInteger())
			{
				rawValue = Convert.ToInt64(rawValueBoxed).ToString(cultureInfo);
			}
			else if (rawValueBoxed != null && type.IsDecimal())
			{
				rawValue = Convert.ToDecimal(rawValueBoxed).ToString(cultureInfo);
			}
			else if (rawValueBoxed != null && type.IsDouble())
			{
				rawValue = Convert.ToDouble(rawValueBoxed).ToString(cultureInfo);
			}
			else if (rawValueBoxed != null && type.IsFloat())
			{
				rawValue = Convert.ToSingle(rawValueBoxed).ToString(cultureInfo);
			}
			else if (rawValueBoxed != null && type.IsDateTime())
			{
				if (cultureInfo == CultureInfo.InvariantCulture)
				{
					rawValue = Convert.ToDateTime(rawValueBoxed).ToString("yyyy-MM-ddTHH:mm:ssZ");
				}
				else
				{
					rawValue = Convert.ToDateTime(rawValueBoxed).ToString(cultureInfo);
				}
			}
			else
			{
				rawValue = rawValueBoxed?.ToString();
			}

			return rawValue;
		}

		protected string CheckAndApplyMapping(string rawValue, DataProperty dataProperty)
		{
			if (dataProperty.ValueMappings?.Any() ?? false)
			{
				var mapping = dataProperty.ValueMappings.FirstOrDefault(m => m.Target.Equals(rawValue, StringComparison.InvariantCultureIgnoreCase));
				if (mapping != default && !mapping.Source.IsNullOrEmpty())
				{
					rawValue = mapping.Source;
				}
			}

			return rawValue;
		}
	}
}