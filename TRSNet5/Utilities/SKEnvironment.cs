using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// A utility that describes the environment, such as where the rsrc directory is.
	/// </summary>
	public static class SKEnvironment {

		/// <summary>
		/// The Spiral Knights game folder.
		/// </summary>
		public static DirectoryInfo RSRC_DIR { get; set; } = new DirectoryInfo(@"E:\Steam Games\steamapps\common\Spiral Knights\");

		/// <summary>
		/// Using <see cref="RSRC_DIR"/> this will return a <see cref="FileInfo"/> described by <paramref name="dir"/>. For example, if <paramref name="dir"/> is <c>character/npc/monster/gremlin/null/model.dat</c>, then this will return the absolute path to that file.
		/// </summary>
		/// <param name="dir">The path of the file relative to rsrc.</param>
		/// <param name="upOneFromRSRC">If true, then the path will be resolved from the Spiral Knights directory rather than the rsrc directory.</param>
		/// <returns></returns>
		public static FileInfo ResolveSKFile(string dir, bool upOneFromRSRC = false) {
			return new FileInfo(Path.Combine(RSRC_DIR.FullName, upOneFromRSRC ? @"\..\" : string.Empty, dir));
		}

		/// <summary>
		/// Given a <see cref="FileInfo"/> presumably existing within <see cref="RSRC_DIR"/>, this will trim away everything up to rsrc and return the path
		/// as a string. If this is not applicable, the <see cref="FileSystemInfo.FullName"/> is returned.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static string GetRSRCRelativePath(FileInfo file) {
			string fnForward = file.FullName.Replace(@"\", "/");
			string rsrcForward = RSRC_DIR.Parent!.FullName.Replace(@"\", "/");
			if (rsrcForward.EndsWith('/')) {
				rsrcForward = rsrcForward[..^1];
			}
			if (fnForward.ToLower().StartsWith(rsrcForward.ToLower())) {
				return fnForward[(rsrcForward.Length + 1)..];
			}
			return fnForward;
		}

	}
}
