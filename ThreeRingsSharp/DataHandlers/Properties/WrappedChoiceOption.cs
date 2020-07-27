using com.threerings.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.Properties {

	/// <summary>
	/// An option within a <see cref="WrappedChoice"/>.<para/>
	/// <para/>
	/// WARNING: Object not functional entirely.
	/// </summary>
	public class WrappedChoiceOption {

		/// <summary>
		/// The <see cref="WrappedChoice"/> that contains this as one of its options.
		/// </summary>
		public WrappedChoice ParentChoice { get; }

		/// <summary>
		/// The name of this option.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The underlying <see cref="Parameter.Choice.Option"/> that this <see cref="WrappedChoiceOption"/> was created from.
		/// </summary>
		public Parameter.Choice.Option BaseOption { get; }

		/// <summary>
		/// A reference to the <see cref="WrappedDirect"/>s defined by the parent <see cref="WrappedChoice"/>. Unlike typical behavior, this actually clones the directs for this option specifically and then returns the unique directs that contain this option's data.<para/>
		/// (Under stock OOO behavior, a choice contains the data to apply to the direct, and then when the choice is selected, it applies its data to the parent directs contained in the choice object. In THIS object, each option contains its own personal directs rather than relying on editing the parent choice object.)
		/// </summary>
		public WrappedDirect[] Directs { get; }

		/// <summary>
		/// The arguments associated with this <see cref="WrappedChoice"/> that dictate what it does to the object it is tied to.
		/// </summary>
		public ArgumentMap Arguments { get; }

		/// <summary>
		/// Construct a new <see cref="WrappedChoiceOption"/> from the given parent <see cref="WrappedChoice"/> and the given option within said choice.
		/// </summary>
		/// <param name="choice"></param>
		/// <param name="option"></param>
		/// <param name="args"></param>
		public WrappedChoiceOption(WrappedChoice choice, Parameter.Choice.Option option, ArgumentMap args = null) {
			Name = option.name;
			ParentChoice = choice;
			BaseOption = option;
			Arguments = args ?? option.GetArguments();
			// _arguments was exposed via an IL edit that I did. This is not possible under normal OOO behavior.
			// I aim to undo this soon as a method is better for the sake of uniformity.

			List<WrappedDirect> newDirects = new List<WrappedDirect>();
			foreach (Parameter.Direct direct in choice.BaseChoice.directs) {
				Parameter.Direct newDirect = (Parameter.Direct)direct.clone(); // Thank god for their DeepObject class.
				newDirects.Add(new WrappedDirect(choice.Config, newDirect, choice, Arguments));
			}
			Directs = newDirects.ToArray();
		}
	}
}
