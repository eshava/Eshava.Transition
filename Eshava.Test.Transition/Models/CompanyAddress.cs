using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class CompanyAddress : IEmpty
	{
		public string Street { get; set; }
		public int ZIPCode { get; set; }
		public string Place { get; set; }
		public string Country { get; set; }
		public bool IsEmpty => false;
	}
}