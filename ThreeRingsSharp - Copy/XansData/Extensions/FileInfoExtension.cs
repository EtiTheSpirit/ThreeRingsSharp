using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;

namespace ThreeRingsSharp.XansData.Extensions {

	/// <summary>
	/// Offers extensions to <see cref="FileInfo"/>.
	/// </summary>
	public static class FileInfoExtension {

		/// <summary>
		/// Returns a string which is the path of this <see cref="FileInfo"/> relative to the rsrc folder. The returned path will never start with a slash.
		/// </summary>
		/// <param name="info">The <see cref="FileInfo"/> to extract the data from.</param>
		/// <param name="useSystemSeparators">If <see langword="true"/>, the system's filepath separator character will be used. If <see langword="false"/>, forward slashes (<c>/</c>) will be used.</param>
		/// <param name="appendRsrc">If <see langword="true"/>, the path will start with <c>/rsrc</c></param>
		public static string AsResourcePath(this FileInfo info, bool useSystemSeparators = false, bool appendRsrc = true) {
			List<string> parents = new List<string>();
			char separator = useSystemSeparators ? Path.DirectorySeparatorChar : '/';

			DirectoryInfo parentDir = info.Directory;
			while (true) {
				string dirName = parentDir.Name;
				parents.Add(dirName);
				if (dirName == "rsrc") break;
				parentDir = parentDir.Parent;
				if (parentDir == null) break;
			}

			parents.Reverse();
			if (!appendRsrc && parents.First() == "rsrc") {
				parents = parents.Skip(1).ToList();
			}
			string retn = "";
			foreach (string str in parents) {
				retn += str + separator;
			}
			string name = info.Name;
			retn += name;
			return retn;
		}

	}
}
