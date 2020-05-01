using System.Reflection;

namespace Eshava.Transition.Interfaces.EDI
{
	public interface IEdiPrimitiveDataSettings : IEdiSubDataRecordSettings
	{
		PropertyInfo[] PropertyInfos { get; }
	}
}