namespace Eshava.Transition.Models.EDI
{
	public class EdiSettings : AbstractSettings
	{
		public string[] DataRows { get; set; }
		public int CurrentStartRowIndex { get; set; }
	}
}