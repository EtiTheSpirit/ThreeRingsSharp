using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeRingsSharp.Utilities.Parameters.Implementation {

	/// <summary>
	/// An implementation of a Choice parameter, which has a number of potential preset options that it applies to any number of underlying <see cref="Direct"/>s.
	/// </summary>
	public class Choice : Parameter {

		/// <summary>
		/// The <see cref="Direct"/>s that this modifies.
		/// </summary>
		public Direct[] Directs = Array.Empty<Direct>();

		/// <summary>
		/// The valid <see cref="Option"/>s within this <see cref="Choice"/>.
		/// </summary>
		public Option[] Options = Array.Empty<Option>();

		/// <summary>
		/// The name of every included <see cref="Option"/>.
		/// </summary>
		public string[] OptionNames {
			get {
				if (_OptionNames == null) {
					_OptionNames = new string[Options.Length];
					for (int idx = 0; idx < Options.Length; idx++) {
						_OptionNames[idx] = Options[idx].Name;
					}
				}
				return _OptionNames;
			}
		}
		private string[]? _OptionNames = null;

		/// <summary>
		/// The currently selected <see cref="Option"/>.
		/// </summary>
		/// <remarks>
		/// For simpler access, <see cref="CurrentName"/> may be desirable.
		/// </remarks>
		public Option Current { get; protected set; }

		/// <summary>
		/// The name of the currently selected option.
		/// </summary>
		/// <remarks>
		/// The setter of this property will only work if the input name is a valid <see cref="Option"/> (as defined by <see cref="Options"/>). If it is invalid, it will raise an <see cref="ArgumentException"/>.
		/// </remarks>
		public string CurrentName {
			get => Current.Name;
			set => Current = Options.FirstOrDefault(option => option.Name == value) ?? throw new ArgumentException($"Invalid option \"{value}\"!", nameof(value));
		}

		public Choice(ShadowClass parameterizedConfig, ShadowClass shadow) : base(parameterizedConfig) {
			shadow.AssertIsInstanceOf("com.threerings.config.Parameter$Choice");
			Name = shadow["name"]!;
			ShadowClass[] directs = shadow["directs"]!;
			ShadowClass[] options = shadow["options"]!;

			Directs = new Direct[directs.Length];
			for (int i = 0; i < directs.Length; i++) {
				Directs[i] = new Direct(parameterizedConfig, directs[i]);
			}

			Options = new Option[options.Length];;
			for (int i = 0; i < options.Length; i++) {
				Options[i] = new Option(this, options[i]);
			}

			string choice = shadow["choice"]!;
			Current = Options.FirstOrDefault(opt => opt.Name == choice)!;
		}

		/// <summary>
		/// Returns the first <see cref="Direct"/> that has the given name, or <see langword="null"/> if no <see cref="Direct"/> could be found.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public Direct? GetDirect(string name) {
			return Directs.FirstOrDefault(direct => direct.Name == name);
		}

		public class Option {

			/// <summary>
			/// The <see cref="Choice"/> that houses this <see cref="Option"/>.
			/// </summary>
			public Choice Parent { get; }

			/// <summary>
			/// The name of this <see cref="Option"/> as it is displayed in the Clyde Configuration Editor.
			/// </summary>
			public string Name { get; set; } = string.Empty;

			/// <summary>
			/// The arguments applied to the underlying <see cref="Directs"/> for the parent choice.
			/// </summary>
			public Dictionary<string, object?> Arguments { get; protected set; } = new Dictionary<string, object?>();

			public Option(Choice parent, ShadowClass shadow) {
				shadow.AssertIsInstanceOf("com.threerings.config.Parameter$Choice$Option");
				Parent = parent;
				Name = shadow["name"]!;
				Dictionary<object, object?> args = shadow["_arguments"]!;
				foreach (KeyValuePair<object, object?> data in args) {
					Arguments[data.Key.ToString()!] = data.Value;
				}
			}

			/// <summary>
			/// Applies this option to the applicable ParameterizedConfig that the parent <see cref="Choice"/> is part of.
			/// </summary>
			public void Apply() {
				foreach (KeyValuePair<string, object?> kvp in Arguments) {
					Direct? direct = Parent.GetDirect(kvp.Key);
					if (direct != null) {
						direct.SetAllValuesTo(kvp.Value);
					}
				}
			}

		}

	}
}
