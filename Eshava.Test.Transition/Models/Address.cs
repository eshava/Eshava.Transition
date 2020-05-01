using System.Collections.Generic;
using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class Address : IEmpty
	{
		public string AddressNumber { get; set; }
		public string CompanyName { get; set; }
		public string Street { get; set; }
		public int ZIPCode { get; set; }
		public string Place { get; set; }
		public string Country { get; set; }
		public IEnumerable<Contact> Contacts { get; set; }
		public IEnumerable<Communication> Communications { get; set; }

		public bool IsEmpty => CompanyName.IsNullOrEmpty();
	}
}