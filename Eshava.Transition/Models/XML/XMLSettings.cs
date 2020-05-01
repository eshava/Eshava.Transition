using System.Reflection;
using System.Xml;

namespace Eshava.Transition.Models.XML
{
	public class XMLSettings : AbstractSettings<XmlNode>
	{
		public XmlDocument Document { get; set; }
		public PropertyInfo[] PropertyInfos { get; set; }
	}
}