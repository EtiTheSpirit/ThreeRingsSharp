using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utilities.Parameters.Implementation;

namespace ThreeRingsSharp.Utilities.Parameters {
	public class TextureHelper {

		/// <summary>
		/// Assuming this is a ModelConfig storing textures, this will try to find the textures from its parameters. Additionally, it will find the default texture of the model, and return the <see cref="Choice"/> that uses this default if applicable.
		/// </summary>
		/// <param name="modelConfig"></param>
		/// <param name="defaultTextureFromVisibleMesh"></param>
		/// <returns></returns>
		public static (List<string>, string, Choice?) FindTexturesAndActiveFromDirects(ShadowClass modelConfig, string defaultTextureFromVisibleMesh) {
			List<string> retn = new List<string>();
			if (modelConfig["implementation"]!.IsA("com.threerings.opengl.model.config.ModelConfig$Imported")) {
				(string[], string) info = GetDefaultTextures(modelConfig["implementation"], defaultTextureFromVisibleMesh);
				retn.AddRange(info.Item1);
				defaultTextureFromVisibleMesh = info.Item2;
			}

			Choice? defaultChoice = null;

			foreach (ShadowClass param in modelConfig["parameters"]!) {
				if (param.IsA("com.threerings.config.Parameter$Choice")) {
					Choice choice = new Choice(modelConfig, param);
					foreach (Direct direct in choice.Directs) {
						Direct.DirectPointer ptr = direct.ValuePointer!;
						if (ptr.Path.Contains("material_mappings") && ptr.Path.EndsWith("file")) {
							foreach (Choice.Option option in choice.Options) {
								object? value = option.Arguments[direct.Name];
								if (value is string str) {
									retn.Add(str);
									if (defaultChoice == null) {
										defaultChoice = choice;
									}
								}
							}
						}
					}
				}
			}

			return (retn, defaultTextureFromVisibleMesh, defaultChoice);
		}

		/// <summary>
		/// Returns the currently active textures for each of the MaterialMappings within this ModelConfig.<para/>
		/// This will only pull the textures actively in use by the model. Any other variants will not be acquired.
		/// </summary>
		/// <param name="imported"></param>
		/// <param name="defFromVisibleMesh"></param>
		/// <returns></returns>
		public static (string[], string) GetDefaultTextures(ShadowClass imported, string defFromVisibleMesh) {
			ShadowClass[] materialMappings = imported["materialMappings"]!;
			string[] textures = new string[materialMappings.Length];

			for (int index = 0; index < textures.Length; index++) {
				ShadowClass mapping = materialMappings[index];
				object argsObj = mapping["material"]!["_arguments"];
				Dictionary<object, object?>? args = null;
				if (argsObj is Dictionary<object, object?>) {
					args = (Dictionary<object, object?>)argsObj;
				} else if (argsObj is ShadowClass argMapShadow) {
					if (argMapShadow.IsA("com.threerings.config.ArgumentMap") && !argMapShadow.IsTemplate) {
						if (argMapShadow.TryGetField("_entries", out object? entries)) {
							if (entries is Dictionary<object, object?> dict) {
								args = dict;
							}
						}
					}
				}
				if (args != null) {
					if (args.GetValueOrDefault("Texture") is ShadowClass textureCfg) {
						args = textureCfg["_arguments"]!;
						if (args.GetValueOrDefault("File") is string file) {
							textures[index] = file;
							if (mapping["texture"] == defFromVisibleMesh) {
								defFromVisibleMesh = new FileInfo(file).Name;
							}
						}
					}
				}
			}
			return (textures, defFromVisibleMesh);
		}
	}
}
