using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
		/// A lookup of every <see cref="XDirect"/> that this option will change depending on the selected <see cref="Choice"/>
		/// </summary>
		public IReadOnlyList<XDirect> Directs { get; }

		/// <summary>
		/// The currently selected option applying to whatever this <see cref="XChoice"/> is a part of.<para/>
		/// Setting this value will update the model. Throws <see cref="ArgumentOutOfRangeException"/> if the string is set to something that is not a key in <see cref="Options"/>.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">If the new string is not a valid entry in <see cref="Options"/></exception>
		public string Choice {
			get => _choice;
			set {
				if (!Options.ContainsKey(value)) {
					throw new ArgumentOutOfRangeException(nameof(value));
				}
				_choice = value;
			}
		}
		private string _choice;

		public XChoice(ParameterizedConfig parent, Parameter.Choice source) : base(parent, source) {
			_choice = source.choice;

			Dictionary<string, XOption> options = new Dictionary<string, XOption>();
			for (int idx = 0; idx < source.options.Length; idx++) {
				Parameter.Choice.Option option = source.options[idx];
				options[option.name] = new XOption(this, option);
			}
			Options = options;

			List<XDirect> directs = new List<XDirect>();
			for (int idx = 0; idx < source.directs.Length; idx++) {
				Parameter.Direct direct = source.directs[idx];
				directs.Add(new XDirect(parent, direct));
			}
			Directs = directs.AsReadOnly();
		}



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

			public XOption(XChoice parent, Parameter.Choice.Option source) {
				Parent = parent;
				Name = source.name;
				Original = source;
			}


		}
	}
}
