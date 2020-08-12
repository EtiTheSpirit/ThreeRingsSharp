using com.threerings.config;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.Drawing;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers {
	public static class AnimationConfigHandler {

		private const string ERR_IMPL_NOT_SUPPORTED = "AnimationConfig type [{0}] is not yet supported!";

		/// <summary>
		/// Handles the given <see cref="AnimationConfig"/> and applies it to the already loaded <see cref="Model3D"/>s.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="animationImplementation"></param>
		/// <param name="attachToModels"></param>
		public static void HandleAnimationImplementation(string name, AnimationConfig.Implementation animationImplementation, List<Model3D> attachToModels) {

			SKAnimatorToolsTransfer.IncrementEnd();
			// Clear out any derived references all the way until we dig down to an original implementation.
			if (animationImplementation is AnimationConfig.Derived derived) {
				animationImplementation = Dereference(derived);
			}

			if (animationImplementation is AnimationConfig.Imported imported) {

				// As OOO says:
				// The transforms for each target, each frame.
				Transform3D[][] transforms = imported.transforms;

				// So presumably this means that...
				// ...It's backwards! First dimension is the frame number, second dimension is the transform for each target.
				// big brain time

				// ... I swear they must've been waiting for someone like me to come along. Purposely being confusing.
				float fps = imported.rate;

				Animation animation = new Animation(name);
				int numIterations = transforms.Length;
				if (imported.skipLastFrame) numIterations--;

				SKAnimatorToolsTransfer.IncrementEnd(numIterations);
				for (int frameIndex = 0; frameIndex < numIterations; frameIndex++) {
					Transform3D[] targetFrames = transforms[frameIndex];
					Animation.Keyframe keyframe = new Animation.Keyframe();
					for (int targetIndex = 0; targetIndex < imported.targets.Length; targetIndex++) {
						string target = imported.targets[targetIndex];
						Transform3D transform = targetFrames[targetIndex];

						// Catch case: Might be null.
						// Since I'm hellbent on doing things oddly for this part of the program, I'll manually interpolate lol
						if (transform == null) {
							Transform3D nextTransform = null;
							Transform3D previousTransform = null;
							int prevIndex = 0;
							int nextIndex = 0;
							// Start at 1 because if there's a single frame gap, then the result of ^ will be 2
							// And then this will count as frame 1 instead of 0, causing 1/2 or 0.5.
							for (int aheadIndex = 0; aheadIndex < numIterations; aheadIndex++) {
								if (aheadIndex > frameIndex) {
									nextTransform = transforms[aheadIndex][targetIndex];
									if (nextTransform != null) {
										nextIndex = aheadIndex;
									}
								} else {
									Transform3D prev = transforms[aheadIndex][targetIndex];
									if (prev != null) {
										previousTransform = prev;
										prevIndex = aheadIndex;
									}
								}
							}

							int max = nextIndex - prevIndex;
							for (float progress = 0; progress < max; progress++) {
								int trsIndex = prevIndex + (int)progress + 1;
								transforms[trsIndex][targetIndex] = previousTransform.lerp(nextTransform, progress / max);
							}
						}

						keyframe.Keys.Add(new Animation.Key {
							Node = target,
							Transform = transform
						});
						keyframe.Time = frameIndex / fps;
					}
					animation.Keyframes.Add(keyframe);
					SKAnimatorToolsTransfer.IncrementProgress();
				}

				foreach (Model3D model in attachToModels) {
					model.Animations.Add(animation);
				}

			} else if (animationImplementation is AnimationConfig.Sequential sequential) {
				//AnimationConfig.Implementation[] subs = new AnimationConfig.Implementation[sequential.animations.Length];

				SKAnimatorToolsTransfer.IncrementEnd(sequential.animations.Length);
				for (int index = 0; index < sequential.animations.Length; index++) {
					AnimationConfig.ComponentAnimation component = sequential.animations[index];
					HandleAnimationImplementation(name, Dereference(component), attachToModels);
					SKAnimatorToolsTransfer.IncrementProgress();
				}
				/*


					// Something's wrong with directs that I gotta fix.

				} else if (animationImplementation is AnimationConfig.Procedural proc) {
					AnimationConfig.TargetTransform[] targets = proc.transforms;


					Animation animation = new Animation(name);
					float timeIncrement = proc.duration / targets.Length;
					int currentFrame = 0;
					foreach (AnimationConfig.TargetTransform targetTrs in targets) {
						Transform3DExpression expr = targetTrs.expression;
						Transform3D transform = (Transform3D)expr.createEvaluator(null).evaluate();

						Animation.Keyframe keyframe = new Animation.Keyframe();
						for (int targetIndex = 0; targetIndex < targetTrs.targets.Length; targetIndex++) {

							string target = targetTrs.targets[targetIndex];
							XanLogger.WriteLine(target, color: Color.Blue);
							keyframe.Keys.Add(new Animation.Key {
								Node = target,
								Transform = transform
							});
							keyframe.Time = currentFrame * timeIncrement;
						}
						animation.Keyframes.Add(keyframe);
						currentFrame++;
					}

					foreach (Model3D model in attachToModels) {
						model.Animations.Add(animation);
					}
				*/
			} else {
				XanLogger.WriteLine(string.Format(ERR_IMPL_NOT_SUPPORTED, animationImplementation.GetType().Name), color: Color.DarkGoldenrod);
			}

			SKAnimatorToolsTransfer.IncrementProgress();
		}

		/// <summary>
		/// Takes in a derived animation implementation and resolves its references until it gets to a non-derived type.<para/>
		/// This will also handle if the reference is a <see cref="AnimationConfig.ComponentAnimation"/>.
		/// </summary>
		/// <param name="derived">The derived animation implementation to get a real animation from.</param>
		/// <returns></returns>
		private static AnimationConfig.Implementation Dereference(AnimationConfig.Derived derived) {
			if (!derived.animation.IsFileReference()) {
				XanLogger.WriteLine("Cannot access animations from configs yet. This requires a direct file reference.", XanLogger.DEBUG, System.Drawing.Color.Orange);
				return null;
			}
			return FinalDereference(derived.animation);
		}

		/// <summary>
		/// Takes in a derived animation implementation and resolves its references until it gets to a non-derived type.<para/>
		/// This will also handle if the reference is a <see cref="AnimationConfig.Derived"/>.
		/// </summary>
		/// <param name="component">The component animation instance to get a real animation from.</param>
		/// <returns></returns>
		private static AnimationConfig.Implementation Dereference(AnimationConfig.ComponentAnimation component) {
			if (!component.animation.IsFileReference()) {
				XanLogger.WriteLine("Cannot access animations from configs yet. This requires a direct file reference.", XanLogger.DEBUG, System.Drawing.Color.Orange);
				return null;
			}
			return FinalDereference(component.animation);
		}


#pragma warning disable CS0419 // Ambiguous reference in cref attribute
		/// <summary>
		/// A helper method for the two <see cref="Dereference"/> methods that takes in a Clyde object which is expected to be an animation of any type and resolves its refs.
		/// </summary>
		/// <param name="animationRef">A <see cref="ConfigReference"/> pointing to an animation.</param>
		/// <returns></returns>
		private static AnimationConfig.Implementation FinalDereference(ConfigReference animationRef) {
			object animationObj = animationRef.ResolveFile();
			if (animationObj is AnimationConfig animation) {
				AnimationConfig.Implementation impl = animation.implementation;
				if (impl is AnimationConfig.Derived newDerived) {
					return Dereference(newDerived);
				} else {
					return impl;
				}
			} else if (animationObj is AnimationConfig.ComponentAnimation newComponent) {
				return Dereference(newComponent);
			} else {
				XanLogger.WriteLine(string.Format(ERR_IMPL_NOT_SUPPORTED, animationObj.GetType().Name), color: Color.DarkGoldenrod);
				return null;
			}
		}

	}
}
