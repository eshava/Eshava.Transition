using System.Collections.Generic;
using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class Company : IEmpty
	{
		public string AddressNumber { get; set; }
		public string CompanyName { get; set; }
		public CompanyAddress Address { get; set; }
		public IEnumerable<Contact> Contacts { get; set; }
		public IEnumerable<Communication> Communications { get; set; }

		public bool IsEmpty => CompanyName.IsNullOrEmpty();
	}
}