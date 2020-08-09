namespace Eshava.Transition.Models.CSV
{
	public class CSVColumnSettings
	{
		public string[] DataRows { get; set; }
		public char Separator { get; set; }
		public bool HasColumnRow { get; set; }
		public int StartRowIndex { get; set; }
		public bool HasSurroundingQuotationMarks { get; set; }
	}
}