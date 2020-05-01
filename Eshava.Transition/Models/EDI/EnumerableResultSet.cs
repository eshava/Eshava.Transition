using System.Collections.Generic;

namespace Eshava.Transition.Models.EDI
{
	public class EnumerableResultSet
	{
		public List<char[]> TargetDataRows { get; set; }
		public IEnumerable<char[]> ResultdataRows { get; set; }
		public int MinLineIndex { get; set; }
		public int MaxLineIndex { get; set; }
		public int MaxLineLength { get; set; }
	}
}