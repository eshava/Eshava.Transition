using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class AdditionalPropertyDataOne : IEmpty
	{
		public string Delta { get; set; }
		public string AttributeDelta { get; set; }

		public bool IsEmpty => Delta.IsNullOrEmpty() && AttributeDelta.IsNullOrEmpty();
	}
}