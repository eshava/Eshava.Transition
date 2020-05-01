using System.Collections.Generic;

namespace Eshava.Transition.Models.CSV
{
	public class CSVSettingsBase
	{
		public Dictionary<string, int> ColumnNames { get; set; }
		public IEnumerable<DataProperty> Properties { get; set; }
	}
}