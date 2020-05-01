using System.Collections.Generic;

namespace Eshava.Transition.Interfaces.EDI
{
	public interface IEdiSubDataRecordEnumerableSettings : IEdiSubDataRecordSettings
	{
		List<IEnumerable<char[]>> SubDataRows { get; }
	}
}