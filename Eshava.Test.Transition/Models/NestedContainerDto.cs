using System.Collections.Generic;

namespace Eshava.Test.Transition.Models
{
	public class NestedContainerDto
	{
		public NestedDataDto Data { get; set; }
	}

	public class NestedDataDto
	{
		public IList<NestedDataGroupDto> NestedGroups { get; set; }
	}

	public class NestedDataGroupDto
	{
		public NestedDataGroupDto()
		{
			Fields = new List<NestedDataFieldDto>();
		}

		public int SequenceNumber { get; set; }
		
		public string Label { get; set; }
		public IList<NestedDataFieldDto> Fields { get; set; }
	}

	public class NestedDataFieldDto
	{
		public int SequenceNumber { get; set; }

		public string Label { get; set; }
		public string Value { get; set; }
		public string Type { get; set; }

		public int Index { get; set; }
		public int PosY => 10 + (Index * 15);
	}
}