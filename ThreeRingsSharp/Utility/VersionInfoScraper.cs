using com.google.common.io;
using com.threerings.export;
using com.threerings.opengl.model.config;
using java.io;
using java.lang;
using java.util.zip;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ThreeRingsSharp.Utility {

	/// <summary>
	/// A hacky class that can read the raw binary of a Clyde file and find its implementation.<para/>
	/// When files are read by <see cref="BinaryImporter"/>, their implementation is cast into a <see cref="Class"/>, or is made null if that implementation doesn't exist. This is particularly problematic for Spiral Knights, where player knight models use the unique <c>ProjectXModelConfig</c> that does not exist in this library since Spiral Knights defines it instead. This class allows us to see that it's using this class.
	/// </summary>
	public class VersionInfoScraper {

		// TODO: Not use this.
		private const string IMPLEMENTATION_TAG = "com.threerings.opengl.model.config.ModelConfig$Implementation";

		/// <summary>
		/// A very hacky method of returning the implementation of this model in its string form so that if Clyde can't read it, we can still see its name. This returns the full class name.<para/>
		/// This is only functional for extensions of <see cref="ModelConfig"/>.
		/// </summary>
		/// <param name="datFile"></param>
		/// <param name="isCompressed"></param>
		/// <returns></returns>
		private static string HackyGetImplementation(FileInfo datFile, bool isCompressed) {
			// This is mainly intended for Spiral Knights's ProjectXModelConfig.
			// SK Animator Tools relied on being in the game directory (like Spiral Spy did) to detect ^
			// This allows reading arbitrary class names, even for hypothetical cases where it's a completely different game that uses Clyde.

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
			index += 4; // Accomodate for int of space taken after that string. I don't know what purpose it serves (maybe class size?)
			byte typeLength = buffer[index]; // Length of the string storing the name of the type.
			string clip = modelAsString.Substring(index + 1, typeLength);
			return clip;
		}

		/// <summary>
		/// Returns <see langword="true"/> if the input <see cref="FileInfo"/> represents a file exported by Clyde. This tests the header of the file and searches accordingly:
		/// <list type="table">
		/// <item>
		/// <term>Binary</term>
		/// <description>Attempts to find <c>0xFACEAF0E</c> as the first four bytes, which is Clyde's header.</description>
		/// </item>
		/// <item>
		/// <term>XML</term>
		/// <description>Attempts to find the Java Class Attribute (<c>&lt;java class="com.threerings.export.XMLImporter" [...]&gt;</c>) at the start of the file.</description>
		/// </item>
		/// </list>
		/// </summary>
		/// <param name="clydeFile">The file to verify as a Clyde file.</param>
		/// <returns></returns>
		public static (bool, ClydeFormat) IsValidClydeFile(FileInfo clydeFile) {
			using (FileStream inp = clydeFile.OpenRead()) {
				byte[] header = new byte[4];
				inp.Read(header, 0, 4);
				uint headerInt = BitConverter.ToUInt32(header.Reverse().ToArray(), 0);
				if (headerInt == 0xFACEAF0E) {
					return (true, ClydeFormat.Binary);
				}
			}
			using (FileStream inp = clydeFile.OpenRead()) {
				try {
					using (XmlReader reader = XmlReader.Create(inp)) {
						reader.Read();

						// ...Read again to skip the newline. wat.
						reader.Read();

						reader.Read(); // Read again to skip the first node.
						if (reader.Name == "java" && reader.GetAttribute("class") == "com.threerings.export.XMLImporter") {
							return (true, ClydeFormat.XML);
						}
					}
				} catch (XmlException) { }
			}
			return (false, ClydeFormat.None);
		}

		/// <summary>
		/// Returns three <see langword="string"/>, in order, the compression status (as a <see langword="string"/>, "Yes" or "No"), the version name (user friendly), and the implementation.
		/// </summary>
		/// <param name="clydeFile"></param>
		/// <param name="format"></param>
		/// <returns></returns>
		public static (string, string, string) GetCosmeticInformation(FileInfo clydeFile, ClydeFormat format) {
			if (format == ClydeFormat.Binary) {
				(bool, string, string) info = GetDatInfo(clydeFile);
				return (info.Item1 ? "Yes" : "No", info.Item2, info.Item3);
			} else if (format == ClydeFormat.XML) {
				return GetXMLInfo(clydeFile);
			}
			throw new ArgumentException("Invalid Clyde Format.");
		}

		/// <summary>
		/// Handles the input <see cref="FileInfo"/> as if it's a binary .DAT file. This reads whether or not it's compressed, its version, and its implementation.
		/// </summary>
		/// <param name="clydeFile"></param>
		/// <returns></returns>
		public static (bool, string, string) GetDatInfo(FileInfo clydeFile) {
			// string isCompressed;
			string version;
			FileInputStream fileIn = new FileInputStream(clydeFile.FullName);
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
			// isCompressed = compressedFormatFlag ? "Yes" : "No";
			return (compressedFormatFlag, version, HackyGetImplementation(clydeFile, compressedFormatFlag));
		}

		/// <summary>
		/// Handles the input <see cref="FileInfo"/> as if it's an XML file. This reads its version and implementation.
		/// </summary>
		/// <param name="clydeFile"></param>
		/// <returns></returns>
		private static (string, string, string) GetXMLInfo(FileInfo clydeFile) {
			string version = null;
			string impl = null;
			using (XmlReader reader = XmlReader.Create(clydeFile.FullName)) {
				while (reader.Read()) {
					if (reader.Name == "java") {
						version = reader.GetAttribute("version") ?? "Unknown";
					}
					if (reader.Name == "implementation") {
						impl = reader.GetAttribute("class") ?? "ERR_NO_IMPL";
					}

					if (version != null && impl != null) {
						break;
					}
				}
			}
			return ("N/A", version, impl);
		}
	}

	/// <summary>
	/// Describes the storage medium of a Clyde file (Binary vs XML)
	/// </summary>
	public enum ClydeFormat {

		/// <summary>
		/// This is not a Clyde file.
		/// </summary>
		None,

		/// <summary>
		/// Represents a Clyde file in its binary format.
		/// </summary>
		Binary,

		/// <summary>
		/// Represents a Clyde file in its XML format.
		/// </summary>
		XML

	}
}
