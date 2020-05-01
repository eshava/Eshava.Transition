using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Eshava.Transition.Enums
{
	[JsonConverter(typeof(StringEnumConverter))]
	public enum ContentFormat
	{
		[EnumMember(Value = nameof(None))]
		None = 0,
		[EnumMember(Value = nameof(Json))]
		Json = 1,
		[EnumMember(Value = nameof(Xml))]
		Xml = 2,
		[EnumMember(Value = nameof(Csv))]
		Csv = 3,
		[EnumMember(Value = nameof(Edi))]
		Edi = 4
	}
}