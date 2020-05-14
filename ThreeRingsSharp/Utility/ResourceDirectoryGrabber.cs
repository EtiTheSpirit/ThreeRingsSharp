﻿using com.threerings.opengl.model.config;
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
		/// A reference to the rsrc directory.
		/// </summary>
		public static DirectoryInfo ResourceDirectory { get; set; } = null;

		/// <summary>
		/// Returns <see cref="ResourceDirectory"/> as its string path. It will always end in a slash. This enforces the use of forward slashes (/) so that it works with <see cref="CompoundConfig"/> references.<para/>
		/// Returns <see langword="null"/> if <see cref="ResourceDirectory"/> is <see langword="null"/>.
		/// </summary>
		public static string ResourceDirectoryPath {
			get {
				if (ResourceDirectory == null) return null;
				string fullName = ResourceDirectory.FullName.Replace('\\', '/');
				if (!fullName.EndsWith("/")) fullName += "/";
				return fullName;
			}
		}

		/// <summary>
		/// Takes <paramref name="fileIn"/> and climbs up its directory tree until it locates a folder named <c>rsrc</c>. It will then return the path starting from rsrc and ending at this file.<para/>
		/// The returned path will always use forward slashes.
		/// </summary>
		/// <param name="fileIn">The file that is presumably a descendant of the rsrc directory.</param>
		/// <param name="includeRsrc">If false, the directory path will NOT start with <c>rsrc/</c>.</param>
		/// <returns></returns>
		public static string GetFormattedPathFromRsrc(FileInfo fileIn, bool includeRsrc = false) {
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
			if (!includeRsrc && parents.First() == "rsrc") {
				parents = parents.Skip(1).ToList();
			}
			string retn = "";
			foreach (string str in parents) {
				retn += str + ".";
			}
			retn += fileIn.Name.Replace(fileIn.Extension, "");
			return retn;
		}

		/// <summary>
		/// Returns a path that uses forward slashes, going <paramref name="depth"/> folders up. A depth of 1 will return the parent directory, a depth of 2 will return the parent of the parent directory, and so on.
		/// </summary>
		/// <param name="fileIn"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if depth is less than 1.</exception>
		public static string GetDirectoryDepth(FileInfo fileIn, int depth = 1) {
			if (depth < 1) throw new ArgumentOutOfRangeException("depth");

			List<string> parents = new List<string>();

			DirectoryInfo parentDir = fileIn.Directory;
			for (int i = 0; i < depth; i++) {
				string dirName = parentDir.Name;
				parents.Add(dirName);
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
