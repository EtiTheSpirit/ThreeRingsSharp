using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// A utility class that looks at directory information to try to find the rsrc folder.
	/// </summary>
	public class ResourceDirectoryGrabber {

		/// <summary>
		/// Takes <paramref name="fileIn"/> and climbs up its directory tree until it locates a folder named <code>rsrc</code>. It will then return the path starting from rsrc and ending at this file.
		/// </summary>
		/// <param name="fileIn"></param>
		/// <returns></returns>
		public static string GetFormattedPathFromRsrc(FileInfo fileIn) {
			List<string> parents = new List<string>();

			DirectoryInfo parentDir = fileIn.Directory;
			while (true) {
				string dirName = parentDir.Name;
				parents.Add(dirName);
				if (dirName == "rsrc") break;
				parentDir = parentDir.Parent;
				if (parentDir == null) break;
			}

			parents.Reverse();
			string retn = "";
			foreach (string str in parents) {
				retn += str + ".";
			}
			retn += fileIn.Name.Replace(fileIn.Extension, "");
			return retn;
		}

	}
}
