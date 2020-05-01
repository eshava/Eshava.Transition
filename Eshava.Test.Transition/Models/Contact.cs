using System.Collections.Generic;
using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class Contact : IEmpty
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Title { get; set; }
		public string Position { get; set; }
		public IEnumerable<Communication> Communications { get; set; }
		public bool IsEmpty => LastName.IsNullOrEmpty();
	}
}