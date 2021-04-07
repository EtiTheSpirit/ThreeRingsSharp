using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.Parameters {

	/// <summary>
	/// A wrapped variant of <see cref="Parameter.Choice"/>
	/// </summary>
	public class XChoice : XParameter {

		/// <summary>
		/// A lookup for every option by its name.
		/// </summary>
		public IReadOnlyDictionary<string, XOption> Options { get; }

		/// <summary>
		/// A lookup of every <see cref="XDirect"/> that this <see cref="XChoice"/> will change depending on the selected <see cref="Choice"/>. Keys are equivalent to the associated <see cref="XParameter.Name"/> of the <see cref="XDirect"/>s.
		/// </summary>
		public IReadOnlyDictionary<string, XDirect> Directs { get; }

		/// <summary>
		/// The currently selected option applying to whatever this <see cref="XChoice"/> is a part of.<para/>
		/// Setting this value will update the model. Throws <see cref="ArgumentOutOfRangeException"/> if the string is set to something that is not a key in <see cref="Options"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">If the new string is not a valid entry in <see cref="Options"/></exception>
		public string Choice {
			get => (Original as Parameter.Choice).choice;
			set {
				if (!Options.ContainsKey(value)) {
					throw new ArgumentOutOfRangeException(nameof(value), value, null);
				}
				(Original as Parameter.Choice).choice = value;
			}
		}

		/// <summary>
		/// The default option.
		/// </summary>
		public XOption Default { get; }

		/// <summary>
		/// The currently selected option, as denoted by <see cref="Choice"/>. This may be <see langword="null"/>.
		/// </summary>
		public XOption Current => Options.GetOrDefault(Choice);

		/// <summary>
		/// The value(s) associated with the default choice's arguments. The keys correspond to that of <see cref="Directs"/>. Identical to referencing <see cref="Default"/><c>.Values</c>
		/// </summary>
		public IReadOnlyDictionary<string, object> DefaultValues => Default?.Values ?? new Dictionary<string, object>();

		/// <summary>
		/// The value(s) associated with the current choice's arguments. The keys correspond to that of <see cref="Directs"/>. Identical to referencing <see cref="Current"/><c>.Values</c>
		/// </summary>
		public IReadOnlyDictionary<string, object> CurrentValues => Current?.Values ?? new Dictionary<string, object>();

		/// <summary>
		/// Construct a new <see cref="XChoice"/> for the given <see cref="ParameterizedConfig"/> and source <see cref="Parameter.Choice"/> object.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="source"></param>
		public XChoice(ParameterizedConfig parent, Parameter.Choice source) : base(parent, source) {
			// This needs to be set first because the options reference Directs early (when they are constructed)
			Dictionary<string, XDirect> directs = new Dictionary<string, XDirect>();
			for (int idx = 0; idx < source.directs.Length; idx++) {
				Parameter.Direct direct = source.directs[idx];
				//directs.Add(new XDirect(parent, direct));
				directs[direct.name] = new XDirect(parent, direct);
			}
			Directs = directs;

			Dictionary<string, XOption> options = new Dictionary<string, XOption>();
			for (int idx = 0; idx < source.options.Length; idx++) {
				Parameter.Choice.Option option = source.options[idx];
				options[option.name] = new XOption(this, option);
			}
			Options = options;
			Default = options.GetOrDefault(source.choice, options.Values.First());
		}


		/// <summary>
		/// An option within this <see cref="XChoice"/>
		/// </summary>
		public class XOption {

			/// <summary>
			/// The name of this option.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// The parent <see cref="XChoice"/> that contains this <see cref="XOption"/>.
			/// </summary>
			public XChoice Parent { get; }

			/// <summary>
			/// The original <see cref="Parameter.Choice.Option"/> that this was created around.
			/// </summary>
			public Parameter.Choice.Option Original { get; }

			/// <summary>
			/// The values that this <see cref="XOption"/> will apply when selected. Keys are the names of <see cref="XDirect"/>s present in the <see cref="Parent"/>, and values are what all <see cref="XDirect"/> paths associated with this argument are set to.
			/// </summary>
			public IReadOnlyDictionary<string, object> Values { get; }

			// n.b. use internal because only XChoice ctor should use this.
			internal XOption(XChoice parent, Parameter.Choice.Option source) {
				Parent = parent;
				Name = source.name;
				Original = source;

				Dictionary<string, object> values = new Dictionary<string, object>();
				object[] keys = source.arguments.keySet().toArray();
				foreach (object key in keys) {
					values[key.ToString()] = source.arguments.get(key);
				}
				Values = values;
			}

			/// <summary>
			/// Applies <see cref="Values"/> to all directs in the <see cref="Parent"/>.
			/// </summary>
			public void Apply() {
				foreach (KeyValuePair<string, object> kvp in Values) {
					Parent.Directs[kvp.Key].SetAllValuesTo(kvp.Value);
				}
			}
			

		}
	}
}
