using System.Globalization;
using System.Linq;
using Eshava.Core.Extensions;

namespace Eshava.Transition.Extensions
{
	public static class StringExtensions
	{
		public static  CultureInfo GetCultureInfo(this string cultureCode)
		{
			if (cultureCode.IsNullOrEmpty())
			{
				return CultureInfo.InvariantCulture;
			}

			var culture = cultureCode.ToLower();

			return CultureInfo.GetCultures(CultureTypes.AllCultures).FirstOrDefault(c => c.Name.ToLower() == culture) ?? CultureInfo.InvariantCulture;
		}
	}
}