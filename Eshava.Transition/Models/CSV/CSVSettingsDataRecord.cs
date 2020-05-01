using System;

namespace Eshava.Transition.Models.CSV
{
	public class CSVSettingsDataRecord : CSVSettingsBase
	{
		public object DataRecord { get; set; }
		public Type DataRecordType { get; set; }
		public string[] DataCells { get; set; }
	}
}