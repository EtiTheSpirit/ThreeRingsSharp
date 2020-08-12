using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using ThreeRingsSharp.DataHandlers.Scene.Data;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.XML.ConfigReferences {

	/// <summary>
	/// Handles iterating through tile elements.
	/// </summary>
	[Obsolete]
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
					string[] clsName = JavaClassNameStripper.GetSplitClassName(impl);
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
						int width = 1;
						int height = 1;
						if (widthElement != null && widthElement.Value != null) {
							if (!int.TryParse(widthElement.Value, out width)) width = 1;
						}
						if (heightElement != null && heightElement.Value != null) {
							if (!int.TryParse(heightElement.Value, out height)) height = 1;
						}
						ShallowTileConfig.FromData(name, refOrMdl, setModelTarget, isDerived, width, height);
					}
				}
			}
			return ShallowTileConfig.TileLookup;
		}

	}
}
