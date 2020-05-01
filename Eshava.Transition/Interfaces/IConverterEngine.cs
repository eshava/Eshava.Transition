using System.Collections.Generic;
using Eshava.Transition.Enums;
using Eshava.Transition.Models;

namespace Eshava.Transition.Interfaces
{
	public interface IConverterEngine
	{
		ContentFormat ContentFormat { get; }

		IEnumerable<T> Convert<T>(DataProperty configuration, string data, bool removeDublicates = true) where T : class, IEmpty;
		IEnumerable<string> Convert<T>(DataProperty configuration, IEnumerable<T> data) where T : class;
	}
}