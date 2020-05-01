using System.Reflection;
using Newtonsoft.Json.Linq;

namespace Eshava.Transition.Models.JSON
{
	public class JSONSettings : AbstractSettings<JToken>
	{
		public PropertyInfo[] PropertyInfos { get; set; }
	}
}