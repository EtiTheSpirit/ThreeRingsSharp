using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// Represents a <see cref="Parameter.Direct"/> in a different manner that allows directly
	/// accessing the object or data it points to.
	/// </summary>
	public class WrappedDirect {

		/// <summary>
		/// The reference to the Clyde object associated with this direct.
		/// </summary>
		public object Config { get; set; }

		/// <summary>
		/// The raw text-based paths for this direct.
		/// </summary>
		public string[] Paths { get; set; }

		/// <summary>
		/// If this direct references a ConfigReference, and if that ConfigReference points to one of the packed configs, this is the name of the packed config (e.g. <c>"material"</c>)
		/// </summary>
		public string ConfigReferenceContainer { get; }

		private object EndObject { get; }

		public WrappedDirect(object cfg, Parameter.Direct direct) {

		}

		/// <summary>
		/// Given a full path from a <see cref="Parameter.Direct"/>, this will traverse it and acquire the data at the end.
		/// </summary>
		/// <param name="path"></param>
		private void TraverseDirectPath(string path) {
			// implementation.material_mappings[0].material["Texture"]["File"]
			// StringExtensions.SnakeToCamel();

			// A bit of a hack to make splitting this path easier:
			path = path.Replace("[", ".[");

			// Split it by the segments of this path, and get rid of the implementation word at the start.
			string[] pathSegments = path.Split('.').Skip(1).ToArray();

			// The latest object stored when traversing this direct's path.
			object latestObject = null;

			for (int idx = 0; idx < pathSegments.Length; idx++) {
				string currentIndex = pathSegments[idx].SnakeToCamel();
				string betweenBrackets = currentIndex.BetweenBrackets();
				if (betweenBrackets != null) {
					// This is either an array index, or a reference to a config.
					// The simple way to test this is that if it's a numeric index, it's an array index.
					if (int.TryParse(currentIndex, out int arrayIndex)) {
						// Access this array index.
						latestObject = ReflectionHelper.Index(latestObject, arrayIndex);
					} else {
						// Access the config reference.
						latestObject = TraverseConfigReference((ConfigReference)latestObject, currentIndex.Substring(1, currentIndex.Length - 2));
					}
				} else {
					// This is referencing a property.

				}
			}

		}

		private object TraverseConfigReference(ConfigReference latestObject, string configReferenceName) {
			string refName = latestObject.getName();
			
		}

	}
}
