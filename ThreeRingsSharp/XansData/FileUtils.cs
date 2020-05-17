using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Offers better file control methods.
	/// </summary>
	public static class FileUtils {

		/// <summary>
		/// Identical to <see cref="FileInfo.OpenWrite"/>, but this will completely erase the file and rewrite it from scratch.
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		public static FileStream OpenWriteNew(this FileInfo file) {
			file.Delete();
			return file.OpenWrite();
		}

	}
}
