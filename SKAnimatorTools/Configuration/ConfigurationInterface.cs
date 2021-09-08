using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.XansData.Extensions;

namespace SKAnimatorTools.Configuration {

	/// <summary>
	/// Provides a means of getting and setting config values.
	/// </summary>
	public class ConfigurationInterface {

		/// <summary>
		/// An event that is fired whenever a configuration value changes.
		/// </summary>
		public static event ConfigChangedEvent? OnConfigurationChanged;
		public delegate void ConfigChangedEvent(string configKey, dynamic oldValue, dynamic newValue);

		/// <summary>
		/// The configuration data.
		/// </summary>
		private static Dictionary<string, dynamic?>? Configuration = null;

		/// <summary>
		/// A reference to the config file.
		/// </summary>
		private static readonly FileInfo ConfigFile = new FileInfo(@".\threeringssharp.cfg");

		/// <summary>
		/// Loads configuration data from the local config file.
		/// </summary>
		private static void LoadConfigs() {
			if (!ConfigFile.Exists) {
				using FileStream str = ConfigFile.Create(); 
				str.WriteByte((byte)'{');
				str.WriteByte((byte)'}');
			}
			Configuration = JsonConvert.DeserializeObject<Dictionary<string, dynamic?>>(File.ReadAllText(ConfigFile.FullName));
		}

		/// <summary>
		/// Saves configuration data to the local config file.
		/// </summary>
		private static void SaveConfigs() {
			File.WriteAllText(ConfigFile.FullName, JsonConvert.SerializeObject(Configuration));
		}

		/// <summary>
		/// Attempts to get the given configuration value. If <paramref name="writeIfDoesntExist"/> is <see langword="true"/> and <paramref name="defaultValue"/> is not <see langword="null"/>, then <paramref name="defaultValue"/> will be written to the specified key.
		/// </summary>
		/// <param name="key">The key for the desired configuration value.</param>
		/// <param name="defaultValue">The default value to return if the key has no associated value.</param>
		/// <param name="writeIfDoesntExist">Whether or not to write the default value (if it's not null) to the configuration.</param>
		/// <exception cref="ArgumentNullException">Thrown if the key is null or empty.</exception>
		/// <exception cref="ArgumentException">Thrown if the key contains illegal characters or is longer than 32 characters.</exception>
		public static dynamic? GetConfigurationValue(string key, dynamic? defaultValue = null, bool writeIfDoesntExist = false) {
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
			if (key.Length > 32) throw new ArgumentException("The key is too long! Please make sure the key is <= 32 characters long.", "key");
			if (!key.IsAlphanumeric()) throw new ArgumentException("The key contains illegal characters! Please only use alphanumeric characters.", "key");
			if (Configuration == null) LoadConfigs();

			if (Configuration!.ContainsKey(key)) {
				return Configuration[key];
			}
			if (defaultValue != null && writeIfDoesntExist) {
				Configuration[key] = defaultValue;
				SaveConfigs();
			}
			return defaultValue;
		}

		/// <summary>
		/// Sets the given configuration key to the given configuration value. If <paramref name="value"/> is <see langword="null"/>, the key will be erased.
		/// </summary>
		/// <param name="key">The key for the desired configuration value.</param>
		/// <param name="value">The value to assign.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if the key is null or empty.</exception>
		/// <exception cref="ArgumentException">Thrown if the key contains illegal characters or is longer than 32 characters.</exception>
		public static void SetConfigurationValue(string key, dynamic? value) {
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
			if (key.Length > 32) throw new ArgumentException("The key is too long! Please make sure the key is <= 32 characters long.", nameof(key));
			if (!key.IsAlphanumeric()) throw new ArgumentException("The key contains illegal characters! Please only use alphanumeric characters.", nameof(key));
			if (Configuration == null) LoadConfigs();
			dynamic? oldValue = null;
			if (Configuration!.ContainsKey(key)) {
				oldValue = Configuration[key];
			}

			if (value == null) {
				if (Configuration.ContainsKey(key)) Configuration.Remove(key);
				OnConfigurationChanged(key, oldValue, null);
				return;
			}

			Configuration[key] = value;
			OnConfigurationChanged(key, oldValue, value);
			SaveConfigs();
		}

		/// <summary>
		/// Removes the given key from the config data. Does not fire any config changed events, as this is intended for removing legacy values.<para/>
		/// If the key doesn't exist, this does nothing.
		/// </summary>
		/// <param name="key"></param>
		public static void RemoveConfigurationValue(string key) {
			if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
			if (key.Length > 32) throw new ArgumentException("The key is too long! Please make sure the key is <= 32 characters long.", nameof(key));
			if (!key.IsAlphanumeric()) throw new ArgumentException("The key contains illegal characters! Please only use alphanumeric characters.", nameof(key));
			if (Configuration == null) LoadConfigs();
			if (Configuration!.ContainsKey(key)) {
				Configuration.Remove(key);
			}
			SaveConfigs();
		}
	}
}
