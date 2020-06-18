using System.Runtime.CompilerServices;
using Castle.Core.Internal;
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