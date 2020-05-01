using System;
using System.Globalization;
using System.Reflection;

namespace Eshava.Transition.Models
{
	public abstract class AbstractSettings
	{
		public DataProperty DataProperty { get; set; }
		public PropertyInfo PropertyInfo { get; set; }
		public Type DataType { get; set; }
		public object DataRecord { get; set; }
		public CultureInfo CultureInfo { get; set; }
	}

	public abstract class AbstractSettings<T> : AbstractSettings
	{
		public T RawDataNode { get; set; }
	}
}