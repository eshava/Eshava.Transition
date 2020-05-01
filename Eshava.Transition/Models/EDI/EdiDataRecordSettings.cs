namespace Eshava.Transition.Models.EDI
{
	public class EdiDataRecordSettings
	{
		public object DataRecord { get; set; }
		public DataProperty DataProperty { get; set; }
		public int MaxLineLength { get; set; }
		public int MaxLineCount { get; set; }
	}
}