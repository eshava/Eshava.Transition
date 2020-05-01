using System.Reflection;

namespace Eshava.Transition.Models.CSV
{
	public class CSVExportSettings
	{
		public object DataRecord { get; set; }
		public string[] DataRow { get; set; }

		public DataProperty DataProperty { get; set; }
		public PropertyInfo PropertyInfo { get; set; }

		public PropertyInfo[] PropertyInfos { get; set; }
	}
}