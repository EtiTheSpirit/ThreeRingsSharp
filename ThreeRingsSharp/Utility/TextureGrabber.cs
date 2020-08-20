using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// <see cref="ModelPropertyUtility"/>
	/// </summary>
	[Obsolete("This method of getting textures is incorrect. Use ThreeRingsSharp.DataHandlers.Properties.ModelPropertyUtility.FindTexturesFromDirects instead.", true)]
	public class TextureGrabber {
		

		/// <summary>
		/// A utility method that, when given the directory of a model file and a texture name, will attempt to locate a given texture.
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="textureName"></param>
		/// <returns></returns>
		public static string GetFullTexturePath(FileInfo sourceFile, string textureName) {
			bool texFolderExists = sourceFile.Directory.EnumerateDirectories().Where(directory => directory.Name == "textures").Count() > 0;
			string dir = sourceFile.Directory.FullName.Replace("\\", "/") + "/";
			if (textureName.EndsWith(".psd")) {
				Debug.WriteLine("WARNING: Photoshop file referenced! Is this the right texture? Changing extension to png...");
				textureName = textureName.Substring(0, textureName.Length - 3) + "png";
			}

			// Try to resolve here first.
			FileInfo tex;
			if (texFolderExists) {
				tex = new FileInfo(dir + "textures/" + textureName);
				if (tex.Exists) {
					// Good!
					return tex.FullName;
				}
			}

			// Now if we made it here, there was either no textures directory, or the image wasn't found in the texture directory.
			tex = new FileInfo(dir + textureName);
			if (tex.Exists) {
				return tex.FullName;
			}
			Debug.WriteLine("WARNING: Unable to locate texture! Searched: " + tex.FullName);
			return null;
		}

	}
}
