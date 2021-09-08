using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SKAnimatorTools.Configuration.Attributes;

namespace SKAnimatorTools.Configuration {

	/// <summary>
	/// Represents a user's configuration.<para/>
	/// Properties should be acquired and set through this class.
	/// </summary>
	public static class UserConfiguration {

		public const string DEFAULT_DIRECTORY = @"C:\Program Files (x86)\Steam\steamapps\common\Spiral Knights\rsrc";

		[
			ConfigEntry, DefaultValue(true)
		]
		public static bool IsFirstTimeOpening { get; set; } = true;

		/// <summary>
		/// The directory the program opens up by default.
		/// </summary>
		[
			ConfigEntry, DefaultValue(DEFAULT_DIRECTORY)
		]
		public static string DefaultLoadDirectory { get; set; } = DEFAULT_DIRECTORY;

		/// <summary>
		/// The directory the program saves to by default.
		/// </summary>
		[
			ConfigEntry, DefaultValue("")
		]
		public static string LastSaveDirectory { get; set; } = "";

		/// <summary>
		/// The resource directory.
		/// </summary>
		[
			ConfigEntry, DefaultValue(DEFAULT_DIRECTORY)
		]
		public static string RsrcDirectory { get; set; } = DEFAULT_DIRECTORY;

		/// <summary>
		/// Whether or not to make the directory of the file viewer persistent.
		/// </summary>
		[
			ConfigEntry, DefaultValue(false)
		]
		public static bool RememberDirectoryAfterOpen { get; set; } = false;

		/// <summary>
		/// Scale models by 100x
		/// </summary>
		[
			ConfigEntry, DefaultValue(false)
		]
		public static bool ScaleBy100 { get; set; } = false;

		/// <summary>
		/// Protect against models that have no scale.
		/// </summary>
		[
			ConfigEntry, DefaultValue(true)
		]
		public static bool ProtectAgainstZeroScale { get; set; } = true;

		/// <summary>
		/// The mode to use when dealing with <see cref="ConditionalConfig"/> instances in the export.<para/>
		/// <c>0 = Prompt Me</c><para/>
		/// <c>1 = All models</c><para/>
		/// <c>2 = Enabled models</c><para/>
		/// <c>3 = Disabled models</c><para/>
		/// <c>4 = Default model</c><para/>
		/// </summary>
		[
			ConfigEntry, DefaultValue(0)
		]
		public static int ConditionalConfigExportMode { get; set; } = 0;

		/// <summary>
		/// The mode to use when dealing with <see cref="StaticSetConfig"/> instances in the export.<para/>
		/// <c>0 = Prompt Me</c><para/>
		/// <c>1 = All</c><para/>
		/// <c>2 = One</c><para/>
		/// </summary>
		[
			ConfigEntry, DefaultValue(0)
		]
		public static int StaticSetExportMode { get; set; } = 0;

		/// <summary>
		/// Whether or not to embed textures directly in the glTF.
		/// </summary>
		[
			ConfigEntry, DefaultValue(false)
		]
		public static bool EmbedTextures { get; set; } = false;

		/// <summary>
		/// The level to use when logging in the main program window.
		/// </summary>
		[
			ConfigEntry, DefaultValue(0)
		]
		public static int LoggingLevel { get; set; } = 0;

		/// <summary>
		/// Disable GUI elements in favor of processing speed.
		/// </summary>
		[
			ConfigEntry, DefaultValue(false)
		]
		public static bool PreferSpeed { get; set; } = false;

		/// <summary>
		/// Saves this configuration to json.
		/// </summary>
		public static void SaveConfiguration() {
			foreach (PropertyInfo property in typeof(UserConfiguration).GetProperties()) {
				if (property.GetCustomAttribute<ConfigEntryAttribute>() != null) {
					object? value = property.GetValue(null);
					ConfigurationInterface.SetConfigurationValue(property.Name, value);
				}
			}
		}

		/// <summary>
		/// Loads this configuration from json.
		/// </summary>
		static UserConfiguration() {
			foreach (PropertyInfo property in typeof(UserConfiguration).GetProperties()) {
				if (property.GetCustomAttribute<ConfigEntryAttribute>() != null) {
					DefaultValueAttribute? defaultValueAttr = property.GetCustomAttribute<DefaultValueAttribute>();
					if (defaultValueAttr == null) continue;

					object? defaultValue = defaultValueAttr.DefaultValue;
					if (defaultValue?.GetType() != property.PropertyType) throw new InvalidCastException("Property [" + property.Name + "]'s DefaultValue attribute has a different type than the type of the property! (Attempt to cast " + defaultValue.GetType().Name + " into " + property.PropertyType.Name + ")");
					object? storedValue = ConfigurationInterface.GetConfigurationValue(property.Name, defaultValue, true);

					// Special handling for numeric values.
					if (storedValue?.GetType() == typeof(long))
						storedValue = Convert.ToInt32(storedValue);
					if (storedValue?.GetType() == property.PropertyType) {
						property.SetValue(null, storedValue);
					}
				}
			}

			ConfigurationInterface.OnConfigurationChanged += (string key, dynamic oldValue, dynamic newValue) => {
				PropertyInfo? prop = typeof(UserConfiguration).GetProperty(key);
				if (prop != null) {
					prop.SetValue(null, newValue);
				}
			};
		}

	}
}
