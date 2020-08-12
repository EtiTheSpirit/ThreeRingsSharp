using System.IO;

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
