using System.Collections.Generic;
using System.Reflection;
using Eshava.Transition.Models;

namespace Eshava.Transition.Interfaces.EDI
{
	public interface IEdiSubDataRecordSettings
	{
		object DataRecord { get; }
		DataProperty DataProperty { get; }
		PropertyInfo PropertyInfo { get; }
		List<char[]> TargetDataRows { get; set; }
		int MaxLineLength { get; }
		int MaxLineCount { get; }
	}
}