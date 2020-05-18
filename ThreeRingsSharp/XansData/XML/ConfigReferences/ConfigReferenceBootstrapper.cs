using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using System.Xml.Linq;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.XansData.XML.ConfigReferences {

	/// <summary>
	/// Loads all of the XML config reference files and stores their data.
	/// </summary>
	public class ConfigReferenceBootstrapper {

		/// <summary>
		/// A read-only map from a given type (e.g. "tile", "placeable") to a <see cref="IReadOnlyDictionary{TKey, TValue}"/> of its own lookup (e.g. a lookup of tile IDs to actual tile references)
		/// </summary>
		public static IReadOnlyDictionary<string, dynamic> References => _References;
		private static readonly Dictionary<string, dynamic> _References = new Dictionary<string, dynamic>();

		/// <summary>
		/// Initializes all config references.
		/// </summary>
		static ConfigReferenceBootstrapper() {
			// Get a reference to the current EXE directory.
			string currentExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("\\", "/");
			if (!currentExeDir.EndsWith("/")) currentExeDir += '/';

			DirectoryInfo configRefsDir = new DirectoryInfo(currentExeDir + "ConfigRefs/");
			if (!configRefsDir.Exists) {
				throw new DirectoryNotFoundException("The ConfigRefs folder is missing!");
			}

			foreach (FileInfo configRef in configRefsDir.EnumerateFiles()) {
				// Go through every config reference file, then create an XmlReader to go through the data
				using (XmlReader reader = XmlReader.Create(configRef.FullName)) {
					// First things first: Locate the "class" attribute.
					while (reader.Read()) {
						if (reader.Name == "object" && reader.Depth == 1) {
							string classAttr = reader.GetAttribute("class");
							if (classAttr != null) {
								// Is it an array class?
								if (classAttr.StartsWith("[")) {
									// Java appends [ to the start of lists when using their raw class representation. If this bracket is found
									// then there is a list of entries within this node. Process it and break out of the loop.

									string classRaw = classAttr.Substring(2).Replace(";", "");
									string className = ClassNameStripper.GetBaseClassName(classRaw);
									if (className == "TileConfig") {
										_References["tile"] = Tile.IterateTiles(reader);
									} else if (className == "PlaceableConfig") {
										
									}
								}
							}
							break;
						}
					}
				}
			}
		}
	}
}
