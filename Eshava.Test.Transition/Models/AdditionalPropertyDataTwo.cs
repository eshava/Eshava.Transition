using Eshava.Core.Extensions;
using Eshava.Transition.Interfaces;

namespace Eshava.Test.Transition.Models
{
	public class AdditionalPropertyDataTwo : IEmpty
	{
		public string Eta { get; set; }
		public string Theta { get; set; }
		public string AttributeEta { get; set; }
		public string AttributeTheta { get; set; }

		public bool IsEmpty =>
			   Eta.IsNullOrEmpty()
			&& AttributeEta.IsNullOrEmpty()
			&& Theta.IsNullOrEmpty()
			&& AttributeTheta.IsNullOrEmpty();
	}
}