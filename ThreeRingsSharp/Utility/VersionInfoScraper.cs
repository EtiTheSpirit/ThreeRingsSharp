using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using java.lang;
using java.io;
using com.threerings.export;
using com.threerings.opengl.model.config;
using java.awt;
using java.util.zip;
using System.IO.Compression;
using sun.misc;
using com.google.common.io;

namespace ThreeRingsSharp.Utility {
	public class VersionInfoScraper {

		private const string IMPLEMENTATION_TAG = "com.threerings.opengl.model.config.ModelConfig$Implementation";


		/// <summary>
		/// A very hacky method of returning the implementation of this model in its string form so that if Clyde can't read it, we can still see its name.
		/// </summary>
		/// <param name="datFile"></param>
		/// <param name="isCompressed"></param>
		/// <returns></returns>
		private static string HackyGetImplementation(FileInfo datFile, bool isCompressed) {
			FileInputStream fileIn = new FileInputStream(datFile.FullName);
			fileIn.skip(8);
			byte[] buffer;
			if (isCompressed) {
				DataInputStream dat = new DataInputStream(new InflaterInputStream(fileIn));
				buffer = ByteStreams.toByteArray(dat);
			} else {
				buffer = ByteStreams.toByteArray(fileIn);
			}
			string modelAsString = Encoding.ASCII.GetString(buffer);
			int index = modelAsString.IndexOf(IMPLEMENTATION_TAG) + IMPLEMENTATION_TAG.Length;
			index += 4; // Accomodate for int
			byte typeLength = buffer[index];
			string clip = modelAsString.Substring(index + 1, typeLength);
			int dollar = clip.IndexOf("$");
			if (dollar != -1) {
				clip = clip.Substring(0, dollar);
			}
			if (clip.Contains('.')) {
				clip = clip.Substring(clip.LastIndexOf('.') + 1);
			}
			return clip;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the input <see cref="FileInfo"/> represents a file exported by Clyde.
		/// </summary>
		/// <param name="datFile"></param>
		/// <returns></returns>
		public static bool IsValidClydeFile(FileInfo datFile) {
			using (FileStream inp = datFile.OpenRead()) {
				byte[] header = new byte[4];
				inp.Read(header, 0, 4);
				uint headerInt = BitConverter.ToUInt32(header.Reverse().ToArray(), 0);
				return headerInt == 0xFACEAF0E;
			}
		}

		/// <summary>
		/// Returns three strings, in order, the compression status (Yes/No), the version name (user friendly), and the implementation.
		/// </summary>
		/// <param name="datFile"></param>
		/// <returns></returns>
		public static (string, string, string) GetCosmeticInformation(FileInfo datFile) {
			string isCompressed;
			string version;
			FileInputStream fileIn = new FileInputStream(datFile.FullName);
			DataInputStream dataInput = new DataInputStream(fileIn);
			dataInput.readInt();
			int v = dataInput.readUnsignedShort();
			if (v == 0x1000) {
				version = "Classic";
			} else if (v == 0x1001) {
				version = "Intermediate";
			} else if (v == 0x1002) {
				version = "VarInt (Latest)";
			} else {
				version = "Unknown Format ID!";
			}
			bool compressedFormatFlag = dataInput.readUnsignedShort() == 0x1000;
			isCompressed = compressedFormatFlag ? "Yes" : "No";
			return (isCompressed, version, HackyGetImplementation(datFile, compressedFormatFlag));
		}
	}
}
