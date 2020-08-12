using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents an animation.
	/// </summary>
	public class Animation {

		/// <summary>
		/// The name of this animation.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The keyframes of this animation.
		/// </summary>
		public List<Keyframe> Keyframes { get; } = new List<Keyframe>();

		/// <summary>
		/// Returns <see cref="Keyframes"/> but in a manner where the order of the list reflects the order of the keyframes in time.<para/>
		/// Note: This can be expensive to reference.
		/// </summary>
		public IReadOnlyList<Keyframe> OrderedKeyframes {
			get {
				Keyframe[] keyframes = Keyframes.ToArray();
				Array.Sort(keyframes);
				return keyframes.ToList().AsReadOnly();
			}
		}

		public Animation() { }
		public Animation(string name) {
			Name = name;
		}

		/// <summary>
		/// Represents a keyframe in the animation.
		/// </summary>
		public class Keyframe : IComparable<Keyframe> {

			/// <summary>
			/// The time that this <see cref="Keyframe"/> occurs at.
			/// </summary>
			public float Time { get; set; } = 0f;

			/// <summary>
			/// All of the individual animation keys that this <see cref="Keyframe"/> collectively contains.
			/// </summary>
			public List<Key> Keys { get; } = new List<Key>();

			public int CompareTo(Keyframe other) {
				if (Time < other.Time) {
					return -1;
				} else if (Time > other.Time) {
					return 1;
				}
				return 0;
			}
		}

		/// <summary>
		/// Represents an individual animation key for a specific node. Intended to be parented to a <see cref="Keyframe"/> to get information like time.
		/// </summary>
		public class Key {

			/// <summary>
			/// The bone this <see cref="Key"/> applies to.
			/// </summary>
			public string Node { get; set; }

			/// <summary>
			/// The <see cref="Transform3D"/> that should be applied to the bone this <see cref="Key"/> applies to.
			/// </summary>
			public Transform3D Transform { get; set; }

		}

	}
}
