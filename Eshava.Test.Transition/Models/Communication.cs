using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class Communication : IEmpty
	{
		public string Type { get; set; }
		public string Value { get; set; }
		public bool IsEmpty => Value.IsNullOrEmpty();
	}
}