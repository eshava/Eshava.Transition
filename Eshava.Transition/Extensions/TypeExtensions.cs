using System;
using Eshava.Core.Extensions;

namespace Eshava.Transition.Extensions
{
	public static class TypeExtensions
	{
		private static readonly Type _typeInteger = typeof(int);
		private static readonly Type _typeLong = typeof(long);
		private static readonly Type _typeDecimal = typeof(decimal);

		public static bool IsInteger(this Type type)
		{
			return type.GetDataType() == _typeInteger;
		}

		public static bool IsLong(this Type type)
		{
			return type.GetDataType() == _typeLong;
		}

		public static bool IsDecimal(this Type type)
		{
			return type.GetDataType() == _typeDecimal;
		}
	}
}