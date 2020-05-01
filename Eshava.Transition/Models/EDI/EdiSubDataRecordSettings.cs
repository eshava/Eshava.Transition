using System.Collections.Generic;
using System.Reflection;
using Eshava.Transition.Interfaces.EDI;

namespace Eshava.Transition.Models.EDI
{
	public class EdiSubDataRecordSettings : EdiDataRecordSettings, IEdiSubDataRecordSettings, IEdiSubDataRecordEnumerableSettings, IEdiPrimitiveDataSettings
	{
		public PropertyInfo PropertyInfo { get; set; }
		public PropertyInfo[] PropertyInfos { get; set; }
		public List<char[]> TargetDataRows { get; set; }
		public List<IEnumerable<char[]>> SubDataRows { get; set; }
	}
}