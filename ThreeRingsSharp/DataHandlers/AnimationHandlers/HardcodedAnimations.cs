using com.threerings.math;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers {

	/// <summary>
	/// Intended to be used for certain procedural animations that are common, such as <c>rotate_x.dat</c> (and its two other axes)
	/// </summary>
	public static class HardcodedAnimations {

		/// <summary>
		/// The mathematical constant τ, or 2π
		/// </summary>
		// tau is better than pi. Change my mind.
		private const float TAU = (float)Math.PI * 2;

		/// <summary>
		/// Creates a new animation for rotation on the X (left/right) axis.
		/// </summary>
		/// <param name="onNode">The node that the animation targets.</param>
		/// <param name="speed">The speed multiplier of the animation.</param>
		/// <param name="durationSeconds">How long the animation lasts. It is recommended to keep this relatively low.</param>
		/// <param name="framerate">The framerate to generate the animation with.</param>
		/// <returns></returns>
		public static Animation CreateRotateX(string onNode, float speed = 1f, float durationSeconds = 5f, int framerate = 30) {
			return CreateRotate(onNode, speed, durationSeconds, framerate, new Vector3f(1, 0, 0));
		}

		/// <summary>
		/// Creates a new animation for rotation on the Y (up/down) axis.
		/// </summary>
		/// <param name="onNode">The node that the animation targets.</param>
		/// <param name="speed">The speed multiplier of the animation.</param>
		/// <param name="durationSeconds">How long the animation lasts. It is recommended to keep this relatively low.</param>
		/// <param name="framerate">The framerate to generate the animation with.</param>
		/// <returns></returns>
		public static Animation CreateRotateY(string onNode, float speed = 1f, float durationSeconds = 5f, int framerate = 30) {
			return CreateRotate(onNode, speed, durationSeconds, framerate, new Vector3f(0, 1, 0));
		}

		/// <summary>
		/// Creates a new animation for rotation on the Z (forward/backward) axis.
		/// </summary>
		/// <param name="onNode">The node that the animation targets.</param>
		/// <param name="speed">The speed multiplier of the animation.</param>
		/// <param name="durationSeconds">How long the animation lasts. It is recommended to keep this relatively low.</param>
		/// <param name="framerate">The framerate to generate the animation with.</param>
		/// <returns></returns>
		public static Animation CreateRotateZ(string onNode, float speed = 1f, float durationSeconds = 5f, int framerate = 30) {
			return CreateRotate(onNode, speed, durationSeconds, framerate, new Vector3f(0, 0, 1));
		}

		private static Animation CreateRotate(string onNode, float speed, float durationSeconds, int framerate, Vector3f axis) {
			Animation anim = new Animation("RotateX[speed=" + speed + "]");
			int numKeyframes = (int)Math.Ceiling(durationSeconds * framerate);
			float timeInc = 1f / framerate;

			// Turn speed of rotation animations is 1 turn / 12 sec
			// I THINK quaternions use radians so...
			float rotInc = TAU * speed / 12;
			for (int kf = 0; kf < numKeyframes; kf++) {
				Animation.Keyframe keyframe = new Animation.Keyframe() {
					Time = timeInc * kf
				};
				keyframe.Keys.Add(new Animation.Key() {
					Node = onNode,
					Transform = new Transform3D(new Vector3f(), new Quaternion().fromAngleAxis(rotInc * kf, axis))
				});
				anim.Keyframes.Add(keyframe);
			}
			return anim;
		}

	}
}
