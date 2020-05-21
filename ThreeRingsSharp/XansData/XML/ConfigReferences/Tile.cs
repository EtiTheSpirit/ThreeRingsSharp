using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using ThreeRingsSharp.DataHandlers.Scene.Data;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.XansData.XML.ConfigReferences {

	/// <summary>
	/// Handles iterating through tile elements.
	/// </summary>
	public class Tile {

		/// <summary>
		/// Go through all of the tile data and get its entries.
		/// </summary>
		/// <param name="reader"></param>
		public static IReadOnlyDictionary<string, ShallowTileConfig> IterateTiles(XmlReader reader) {
			foreach (XElement element in reader.ElementsNamed("entry", 2)) {
				XElement nameNode = element.Element("name");
				XElement implNode = element.Element("implementation");

				if (implNode != null) {
					string name = nameNode.Value;
					string impl = implNode.Attribute("class").Value;
					string refOrMdl = null;
					string[] clsName = ClassNameStripper.GetSplitClassName(impl);
					bool isDerived = clsName.Length == 2 && clsName[1] == "Derived";
					string setModelTarget = null;

					XElement widthElement = implNode.Element("width");
					XElement heightElement = implNode.Element("height");

					if (isDerived) {
						if (implNode.Element("tile") != null && implNode.Element("tile").Element("name") != null) {
							refOrMdl = implNode.Element("tile").Element("name").Value;
						}
					} else {
						if (implNode.Element("model") != null) {
							XElement modelNode = implNode.Element("model");
							if (modelNode.Element("name") != null) {
								refOrMdl = implNode.Element("model").Element("name").Value;
							}
							if (modelNode.Element("arguments") != null) {
								Dictionary<string, string> argContainer = new Dictionary<string, string>();
								string lastKey = null;
								foreach (XElement modelElement in modelNode.Element("arguments").Elements()) {
									if (modelElement.Name == "key") {
										lastKey = modelElement.Value;
									} else if (modelElement.Name == "value") {
										if (lastKey != null) argContainer[lastKey] = modelElement.Value;
										lastKey = null;
									}
								}

								setModelTarget = argContainer.GetOrDefault("Model");
							}
						}
					}

					// This will be stored in ShallowTileConfig.TileLookup, so no worries about GC.
					if (refOrMdl != null) {
						ShallowTileConfig.FromData(name, refOrMdl, setModelTarget, isDerived);
					}
				}
			}
			return ShallowTileConfig.TileLookup;
		}

	}
}
