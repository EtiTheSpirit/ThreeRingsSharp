using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Reflection;
using System.Xml.Linq;
using ThreeRingsSharp.Utility;
using java.io;
using com.threerings.export;
using java.lang;
using com.threerings.export.tools;
using com.google.common.io;
using com.threerings.opengl.material.config;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.XML.ConfigReferences {

	/// <summary>
	/// Loads all of the XML config reference files and stores their data.
	/// </summary>
	public class ConfigReferenceBootstrapper {

		/// <summary>
		/// The current version of the MergedConfigReferences file.
		/// </summary>
		public const int MERGED_FILE_VERSION = 2;

		/// <summary>
		/// A read-only map from a given type name (e.g. "tile", "placeable", or "material") to an object representing its implementation in the Clyde format itself.<para/>
		/// Configs should be referenced like an array: <c>ConfigReferences["tile"]</c><para/>
		/// It can be indexed via a string name, which is the name of the file without .dat or .xml<para/>
		/// It can also be indexed via type, such as via <c>ConfigReferences[<see langword="typeof"/>(<see cref="MaterialConfig"/>)]</c>
		/// </summary>
		public static ConfigReferenceContainer ConfigReferences {
			get {
				FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
				if (!mergedBinFile.Exists) PopulateConfigRefs();
				return _ConfigReferences;
			}
		}
		private static readonly ConfigReferenceContainer _ConfigReferences = new ConfigReferenceContainer();


		private static bool HasPopulatedConfigs { get; set; } = false;
		private static readonly string CurrentExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).Replace("\\", "/");

		static ConfigReferenceBootstrapper() {
			if (!CurrentExeDir.EndsWith("/")) CurrentExeDir += '/';
		}

		/// <summary>
		/// Initializes all config references together.
		/// </summary>
		private static void PopulateConfigRefs() {
			if (HasPopulatedConfigs) return;
			XanLogger.WriteLine("Hold up! This model wants to reference some configs. I'm populating that data now... This might take just a moment!");
			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");

			if (!mergedBinFile.Exists) {
				XanLogger.WriteLine("Special merged binary file doesn't exist! Manually iterating through ConfigRefs...");
				XanLogger.UpdateLog();
				ReadFromRawConfigRefs();
				return;
			}

			// I need to split these merged binary files into their individual dat components.
			// Thankfully, OOO uses Guava which comes boxed with a way of doing this, AND it's already in the Java stream format. Woohoo!
			using (BinaryReader reader = new BinaryReader(mergedBinFile.OpenRead())) {
				if (reader.ReadInt32() == MERGED_FILE_VERSION) {
					int numFiles = reader.ReadInt32();
					for (int index = 0; index < numFiles; index++) {
						int size = reader.ReadInt32();
						int nameLength = reader.ReadByte();
						string name = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
						int typeLength = reader.ReadByte();
						reader.ReadBytes(typeLength); // typeName
						byte[] dat = reader.ReadBytes(size);

						BinaryImporter importer = new BinaryImporter(ByteSource.wrap(dat).openStream());
						object obj = importer.readObject();
						if (obj == null) {
							XanLogger.WriteLine("WARNING: Reference for [" + name + "] returned null!");
							XanLogger.UpdateLog();
						}
						Type type = obj.GetType();
						if (type.IsArray) type = type.GetElementType();

						_ConfigReferences[name] = (obj, type);

						importer.close();
						XanLogger.WriteLine("Populated [" + name + "].", true);
						XanLogger.UpdateLog();
					}
				} else {
					XanLogger.WriteLine("Special merged binary file is out of date! Manually iterating through ConfigRefs...");
					XanLogger.UpdateLog();
					ReadFromRawConfigRefs();
				}
			}

			XanLogger.WriteLine("Done! Config data is populated.");
			XanLogger.UpdateLog();
			HasPopulatedConfigs = true;
		}

		/// <summary>
		/// Accesses <c>MergedConfigReferences.bin</c> and pulls the data from the specific named entry, or null if the merged binary file doesn't exist, is out of date, or the object couldn't be found.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		internal static object GetConfigReferenceFromName(string name) {
			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
			if (!mergedBinFile.Exists) return null;
			// I need to split these merged binary files into their individual dat components.
			// Thankfully, OOO uses Guava which comes boxed with a way of doing this, AND it's already in the Java stream format. Woohoo!
			using (BinaryReader reader = new BinaryReader(mergedBinFile.OpenRead())) {
				if (reader.ReadInt32() != MERGED_FILE_VERSION) return null;

				int numFiles = reader.ReadInt32();
				for (int index = 0; index < numFiles; index++) {
					int size = reader.ReadInt32();
					int nameLength = reader.ReadByte();
					string entryName = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
					int typeLength = reader.ReadByte();
					reader.ReadBytes(typeLength); // typeName.
					byte[] dat = reader.ReadBytes(size);

					if (entryName == name) {
						BinaryImporter importer = new BinaryImporter(ByteSource.wrap(dat).openStream());
						object obj = importer.readObject();
						Type type = obj.GetType();
						if (type.IsArray) type = type.GetElementType();

						_ConfigReferences[entryName] = (obj, type);
						importer.close();
						return obj;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Accesses <c>MergedConfigReferences.bin</c> and pulls the data from the specific typed entry, or null if the merged binary file doesn't exist, is out of date, or the object couldn't be found.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static object GetConfigReferenceFromType(Type type) {
			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
			if (!mergedBinFile.Exists) return null;

			if (type.IsArray) type = type.GetElementType();
			string name = type.Name;
			using (BinaryReader reader = new BinaryReader(mergedBinFile.OpenRead())) {
				if (reader.ReadInt32() != MERGED_FILE_VERSION) return null;

				int numFiles = reader.ReadInt32();
				for (int index = 0; index < numFiles; index++) {
					int size = reader.ReadInt32();
					int nameLength = reader.ReadByte();
					string entryName = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
					int typeLength = reader.ReadByte();
					string typeName = Encoding.ASCII.GetString(reader.ReadBytes(typeLength));
					byte[] dat = reader.ReadBytes(size);

					if (typeName == name) {
						BinaryImporter importer = new BinaryImporter(ByteSource.wrap(dat).openStream());
						object obj = importer.readObject();
						_ConfigReferences[entryName] = (obj, type);

						importer.close();
						return obj;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Returns all of the legal names for config references, or null if the merged binary file doesn't exist or is out of date.
		/// </summary>
		/// <returns></returns>
		internal static string[] GetConfigReferenceNames() {
			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
			if (!mergedBinFile.Exists) return null;
			// I need to split these merged binary files into their individual dat components.
			// Thankfully, OOO uses Guava which comes boxed with a way of doing this, AND it's already in the Java stream format. Woohoo!
			string[] names;
			using (BinaryReader reader = new BinaryReader(mergedBinFile.OpenRead())) {
				if (reader.ReadInt32() != MERGED_FILE_VERSION) return null;
				int numFiles = reader.ReadInt32();
				names = new string[numFiles];
				for (int index = 0; index < numFiles; index++) {
					int size = reader.ReadInt32();
					int nameLength = reader.ReadByte();
					string entryName = Encoding.ASCII.GetString(reader.ReadBytes(nameLength));
					int typeLength = reader.ReadByte();
					string typeName = Encoding.ASCII.GetString(reader.ReadBytes(typeLength));
					reader.ReadBytes(size); // Since we don't need to store anything, just read and call it a day.
					names[index] = entryName;
				}
			}
			return names;
		}

		/// <summary>
		/// If the merged binary stuff doesn't exist, then this will try to find a ConfigRefs folder.
		/// </summary>
		private static void ReadFromRawConfigRefs() {
			// Get a reference to the current EXE directory.
			DirectoryInfo configRefsDir = new DirectoryInfo(CurrentExeDir + "ConfigRefs/");
			if (!configRefsDir.Exists) {
				XanLogger.UpdateAutomatically = false; // Since this throw will prevent it from finishing.
				throw new DirectoryNotFoundException("The ConfigRefs folder is missing! This is a relatively complex issue. You need to install DatDec (go to https://github.com/lucas-allegri/datdec/releases), decompile ALL of the files in the Spiral Knights configs directory, and then copy only the XML files from that directory (DatDec will create the XML files) into a new folder here with the EXE. Create a new folder in the same one as this EXE file named \"ConfigRefs\", and put all of the XML files inside. After you do that, relaunch the program and try again.");
			}
			DirectoryInfo prunedRefsDir = new DirectoryInfo(configRefsDir.FullName + "/BinaryConfigs");

			if (PruneXMLAndMakeBinaries()) return;

			foreach (FileInfo configRef in prunedRefsDir.EnumerateFiles("*.dat")) {
				string fName = configRef.Name.Replace(configRef.Extension, "");

				FileInputStream cfgStream = new FileInputStream(configRef.FullName);
				//XMLImporter importer = new XMLImporter(cfgStream);
				BinaryImporter importer = new BinaryImporter(cfgStream);
				object obj = importer.readObject();
				if (obj == null) {
					XanLogger.WriteLine("WARNING: Reference for [" + fName + "] returned null!");
					XanLogger.UpdateLog();
				}
				Type type = obj.GetType();
				if (type.IsArray) type = type.GetElementType();
				_ConfigReferences[fName] = (obj, type);
				importer.close();
				XanLogger.WriteLine("Populated [" + fName + "].", true);
				XanLogger.UpdateLog();
			}
		}



		/// <summary>
		/// Populates a list of XML Configs from the existing default config refs that don't include entries referencing classes that don't exist (e.g. SK-specific stuff).<para/>
		/// This then translates them to binary files so that loading is considerably faster.<para/>
		/// Returns whether or not to skip populating the config refs dictionary (<see langword="true"/> if it should be skipped).
		/// </summary>
		private static bool PruneXMLAndMakeBinaries() {
			// Get a reference to the current EXE directory.
			DirectoryInfo configRefsDir = new DirectoryInfo(CurrentExeDir + "ConfigRefs/");
			DirectoryInfo prunedRefsDir = new DirectoryInfo(configRefsDir.FullName + "/BinaryConfigs");
			if (!prunedRefsDir.Exists) {
				prunedRefsDir.Create();
			} else {

				JustMakeBinaries(prunedRefsDir);
				return true;
			}

			XanLogger.WriteLine("Pruning configurations to remove entries with classes that don't exist... This might take a bit!");
			XanLogger.WriteLine("While we're here, I'll create a file of every config reference merged together so you can get rid of that ConfigRefs folder.");
			XanLogger.UpdateLog();

			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
			FileStream str = mergedBinFile.OpenWriteNew();
			BinaryWriter writer = new BinaryWriter(str);

			List<FileInfo> allXMLFiles = configRefsDir.EnumerateFiles("*.xml").ToList();

			// Write a placeholder int, which will be replaced with the version and number of entries respectively.
			writer.Write(0);
			writer.Write(0);
			int numFiles = 0;

			foreach (FileInfo configRef in allXMLFiles) {
				string fName = configRef.Name.Replace(configRef.Extension, "");

				// So what I have to do here is read the XML for the config files.
				// I then have to read it entry by entry, and prune out entries that contain illegal classes
				// (primarly, these "illegal classes" will be references to things in SK itself)
				// So let's do that.

				var newXML = new System.Text.StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<java class=\"com.threerings.export.XMLImporter\" version=\"1.0\">\n");

				using (XmlReader reader = XmlReader.Create(configRef.FullName)) {
					XElement element = reader.ElementsNamed("object").First();
					string clsName = element.Attribute("class").Value;
					string originalName = clsName;
					string typeName = null;
					if (clsName != null) {
						clsName = JavaClassNameStripper.RemoveSignature(clsName);
						typeName = clsName.AfterLastIndexOf(".");
						try {
							Class.forName(clsName);
						} catch (ClassNotFoundException) {
							XanLogger.WriteLine("Can't use [" + fName + "] (it exists solely for SK).");
							XanLogger.UpdateLog();
							continue;
						}
						// So here, there's the object element.
						List<XElement> newElements = element.Elements().ToList();
						foreach (XElement entryElement in newElements) {
							if (!HasLegalClasses(entryElement)) entryElement.Remove(); //element.Add(entryElement);
						}

						newXML.AppendLine(element.ToString());
						newXML.Append("</java>");
					}
					FileInfo newXMLFile = new FileInfo(prunedRefsDir.FullName + "/" + configRef.Name);
					System.IO.File.WriteAllText(newXMLFile.FullName, newXML.ToString());

					string datFile = newXMLFile.FullName.Substring(0, newXMLFile.FullName.Length - newXMLFile.Extension.Length) + ".dat";
					XMLToBinaryConverter.convert(newXMLFile.FullName, datFile, true);
					newXMLFile.Delete();

					FileInfo datFileInfo = new FileInfo(datFile);
					int size = (int)datFileInfo.Length;
					using (BinaryReader binReader = new BinaryReader(datFileInfo.OpenRead())) {
						writer.Write(size);
						writer.Write((byte)fName.Length);
						writer.Write(Encoding.ASCII.GetBytes(fName));
						writer.Write((byte)typeName.Length);
						writer.Write(Encoding.ASCII.GetBytes(typeName));
						writer.Write(binReader.ReadBytes(size));
					}
					numFiles++;
				}
				XanLogger.WriteLine("Pruned [" + fName + "] and converted it to binary.");
				XanLogger.UpdateLog();
			}
			str.Flush();
			str.Close();

			using (BinaryWriter wr = new BinaryWriter(mergedBinFile.OpenWrite())) {
				// This abuses the fact that OpenWrite won't erase existing data and instead overwrites it.
				wr.Write(MERGED_FILE_VERSION);
				wr.Write(numFiles);
			}
			XanLogger.WriteLine("Pruning complete! The merged binary file is done too.");
			XanLogger.UpdateLog();
			return false;
		}

		private static void JustMakeBinaries(DirectoryInfo prunedRefsDir) {
			XanLogger.WriteLine("Existing .DAT files found! Only creating merged binaries...");
			XanLogger.UpdateLog();

			FileInfo mergedBinFile = new FileInfo(CurrentExeDir + "MergedConfigReferences.bin");
			FileStream str = mergedBinFile.OpenWriteNew();
			BinaryWriter writer = new BinaryWriter(str);

			// Write a placeholder int.
			writer.Write(0);
			writer.Write(0);
			int numFiles = 0;

			foreach (FileInfo configRef in prunedRefsDir.EnumerateFiles("*.dat")) {
				string fName = configRef.Name.Replace(configRef.Extension, "");

				FileInputStream cfgStream = new FileInputStream(configRef.FullName);
				//XMLImporter importer = new XMLImporter(cfgStream);
				BinaryImporter importer = new BinaryImporter(cfgStream);
				object obj = importer.readObject();
				if (obj == null) {
					XanLogger.WriteLine("WARNING: Reference for [" + fName + "] returned null!");
					XanLogger.UpdateLog();
				}
				Type type = obj.GetType();
				if (type.IsArray) type = type.GetElementType();
				_ConfigReferences[fName] = (obj, type);

				importer.close();
				XanLogger.WriteLine("Populated [" + fName + "].", true);
				XanLogger.UpdateLog();

				int size = (int)configRef.Length;
				using (BinaryReader binReader = new BinaryReader(configRef.OpenRead())) {
					writer.Write(size);
					writer.Write((byte)fName.Length);
					writer.Write(Encoding.ASCII.GetBytes(fName));
					writer.Write((byte)type.Name.Length);
					writer.Write(Encoding.ASCII.GetBytes(type.Name));
					writer.Write(binReader.ReadBytes(size));
				}

				numFiles++;
			}
			str.Flush();
			str.Close();

			using (BinaryWriter wr = new BinaryWriter(mergedBinFile.OpenWrite())) {
				// This abuses the fact that OpenWrite won't erase existing data.
				wr.Write(MERGED_FILE_VERSION);
				wr.Write(numFiles);
			}
			XanLogger.WriteLine("The merged binary file has been created.");
			XanLogger.UpdateLog();
		}

		/// <summary>
		/// Returns <see langword="true"/> if this <see cref="XElement"/> only has class references that can be resolved.<para/>
		/// Returns <see langword="false"/> if this <see cref="XElement"/> contains class references that cannot be resolved.
		/// </summary>
		/// <param name="rootEntry"></param>
		/// <returns></returns>
		private static bool HasLegalClasses(XElement rootEntry) {
			foreach (XElement element in rootEntry.Descendants()) {
				XAttribute classAttr = element.Attribute("class");
				if (classAttr != null) {
					//try {
					//Class.forName(classAttr.Value); //.Replace('$', '.')

					string csClass = classAttr.Value.Replace('$', '+');

					// Shortcut. I want to skip out on searching if at all possible.
					if (csClass.StartsWith("com.threerings.projectx")) {
						DoesClassExistCache[csClass] = false;
						return false;
					}

					// Another shortcut. If it's a stock java class then just don't check.
					if (!csClass.StartsWith("java")) {
						if (!DoesClassExistCache.ContainsKey(csClass)) {
							// Y U K K I
							bool exists = OOOLib.GetTypes().Where(type => type.FullName == csClass).Count() > 0;
							DoesClassExistCache[csClass] = exists;
							if (!exists) {
								return false;
							}
						} else {
							if (DoesClassExistCache[csClass] == false) return false;
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// A reference to the OOOLibAndDeps.dll assembly.
		/// </summary>
		private static readonly Assembly OOOLib = AppDomain.CurrentDomain.GetAssemblies().Where(assembly => assembly.FullName.Contains("OOOLibAndDeps")).First();

		/// <summary>
		/// A cache representing whether or not certain classes exist.
		/// </summary>
		private static readonly Dictionary<string, bool> DoesClassExistCache = new Dictionary<string, bool>();
	}
}
