using System;
using Eshava.Core.Extensions;

namespace Eshava.Transition.Extensions
{
	public static class TypeExtensions
	{
		private static readonly Type _typeInteger = typeof(int);
		private static readonly Type _typeLong = typeof(long);
		private static readonly Type _typeShort = typeof(short);
		private static readonly Type _typeByte = typeof(byte);
		private static readonly Type _typeDecimal = typeof(decimal);
		private static readonly Type _typeDouble = typeof(double);
		private static readonly Type _typeFloat = typeof(float);
		private static readonly Type _typeDateTime = typeof(DateTime);

		public static bool IsInteger(this Type type)
		{
			return type.GetDataType() == _typeInteger;
		}

		public static bool IsLong(this Type type)
		{
			return type.GetDataType() == _typeLong;
		}

		public static bool IsShort(this Type type)
		{
			return type.GetDataType() == _typeShort;
		}

		public static bool IsByte(this Type type)
		{
			return type.GetDataType() == _typeByte;
		}

		public static bool IsDecimal(this Type type)
		{
			return type.GetDataType() == _typeDecimal;
		}

		public static bool IsDouble(this Type type)
		{
			return type.GetDataType() == _typeDouble;
		}

		public static bool IsFloat(this Type type)
		{
			return type.GetDataType() == _typeFloat;
		}

		public static bool IsDateTime(this Type type)
		{
			return type.GetDataType() == _typeDateTime;
		}
	}
}