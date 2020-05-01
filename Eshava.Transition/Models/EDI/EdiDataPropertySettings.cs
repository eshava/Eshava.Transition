using System;

namespace Eshava.Transition.Models.EDI
{
	public class EdiDataPropertySettings
	{
		public string[] DataRows { get; set; }
		public Type DataRecordType { get; set; }
		public DataProperty DataProperty { get; set; }
		public int StartRowIndex { get; set; }
	}
}