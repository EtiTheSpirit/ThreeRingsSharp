using com.threerings.export.tools;
using java.io;
using java.util.zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData;

namespace ClydeDecompressor {
	class Program {
		static void Main(string[] args) {
			if (args.Length == 0) {
				System.Console.ForegroundColor = ConsoleColor.Red;
				System.Console.WriteLine("Please drag a .dat file onto this app.");
			}
			FileInfo datFile = new FileInfo(args[0]);
			if (datFile.Exists && datFile.Extension.ToLower() == ".dat") {
				// BinaryToBinaryConverter.convert(datFile.FullName, datFile.FullName + "-decomp.dat", false);
				// ^ NO GOOD! Can't convert stuff like Knights because if it can't find the data, it errors out.

				// I have to manually decompress.
				(bool isCompressed, string _, string _) = VersionInfoScraper.GetDatInfo(datFile);

				FileInputStream readFile = new FileInputStream(datFile.FullName);
				DataInputStream rawBinaryStream = new DataInputStream(readFile);

				// Skip the first 8 bytes (the header of Clyde files)
				rawBinaryStream.readInt();
				rawBinaryStream.readInt();

				if (isCompressed) {
					System.Console.WriteLine("Decompressing...");
					InflaterInputStream decompStream = new InflaterInputStream(rawBinaryStream);
					List<byte> bytes = new List<byte>();
					while (true) {
						int v = decompStream.read();
						if (v == -1) break;
						bytes.Add((byte)v);
					}
					decompStream.close();

					FileInfo newFile = new FileInfo(datFile.FullName + "-decompressed.dat");
					using (FileStream str = newFile.OpenWriteNew()) {
						str.Write(bytes.ToArray(), 0, bytes.Count);
					}
					System.Console.WriteLine("Done! Wrote to " + datFile.FullName);
				}
			} else {
				System.Console.WriteLine("Failed to load. One of these two pieces of information is faulty.");
				System.Console.WriteLine("Exists? " + (datFile.Exists ? "YES" : "NO") + " (expecting YES)");
				System.Console.WriteLine("Extension: " + datFile.Extension + " (expecting .dat)");
			}
			System.Console.ReadKey();
		}
	}
}
