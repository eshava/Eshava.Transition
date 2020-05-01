using System.Globalization;

namespace Eshava.Transition.Models.CSV
{
	public class CSVSettings : CSVSettingsBase
	{
		public string DataRow { get; set; }
		public char Separator { get; set; }
		public CultureInfo CultureInfo { get; set; }
		public bool HasSurroundingQuotationMarks { get; set; }
	}
}