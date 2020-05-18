using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace ThreeRingsSharp.XansData.XML {
	public static class XMLExtension {

		/// <summary>
		/// Iterates through all of the XML elements with the given <paramref name="elementName"/> at the given <paramref name="depths"/>.
		/// </summary>
		/// <param name="reader">The <see cref="XmlReader"/> to grab data from.</param>
		/// <param name="elementName">The name of the elements that should be returned.</param>
		/// <param name="depths">The depth to locate the element at.</param>
		/// <returns></returns>
		public static IEnumerable<XElement> ElementsNamed(this XmlReader reader, string elementName, params int[] depths) {
			reader.MoveToContent(); // will not advance reader if already on a content node; if successful, ReadState is Interactive
			reader.Read();          // this is needed, even with MoveToContent and ReadState.Interactive
			while (!reader.EOF && reader.ReadState == ReadState.Interactive) {
				// corrected for bug noted by Wes below...
				if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals(elementName) && depths.Contains(reader.Depth)) {
					// this advances the reader...so it's either XNode.ReadFrom() or reader.Read(), but not both
					if (XNode.ReadFrom(reader) is XElement matchedElement)
						yield return matchedElement;
				} else
					reader.Read();
			}
		}

	}
}
