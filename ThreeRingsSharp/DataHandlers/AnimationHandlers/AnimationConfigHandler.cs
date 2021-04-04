﻿using com.threerings.config;
using com.threerings.expr;
using com.threerings.expr.util;
using com.threerings.math;
using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using ThreeRingsSharp.DataHandlers.AnimationHandlers.Expressions;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers {
	public static class AnimationConfigHandler {

		private const string ERR_IMPL_NOT_SUPPORTED = "AnimationConfig type [{0}] is not yet supported!";

		// private const string ERR_PROC_NOT_YET_SUPPORTED = "This Procedural animation [{0}] cannot be loaded yet! Only certain instances of this type work right now.";

		/// <summary>
		/// Handles the given <see cref="AnimationConfig"/> and applies it to the already loaded <see cref="Model3D"/>s.
		/// </summary>
		/// <param name="srcConfig">The ConfigReference that points to this animation.</param>
		/// <param name="name">The name of the animation.</param>
		/// <param name="original">For sequential animations, this is the animation that contains it.</param>
		/// <param name="animationImplementation">The actual implementation of the animation.</param>
		/// <param name="attachToModels">All models that will use this animation.</param>
		/// <param name="source">The ArticulatedConfig that had this animation, which is necessary for scopes.</param>
		public static void HandleAnimationImplementation(ConfigReference srcConfig, string name, AnimationConfig original, AnimationConfig.Implementation animationImplementation, List<Model3D> attachToModels, ArticulatedConfig source) {
			// FileInfo srcFile = new FileInfo(ResourceDirectoryGrabber.ResourceDirectoryPath + srcConfig.getName());
			SKAnimatorToolsProxy.IncrementEnd();
			// Clear out any derived references all the way until we dig down to an original implementation.
			if (animationImplementation is AnimationConfig.Derived derived) {
				animationImplementation = Dereference(derived);
			}

			object nodeValue = srcConfig.getArguments().get("Node");
			object speedValue = srcConfig.getArguments().get("Speed");
			string argumentProvidedNode = (nodeValue is string node) ? node : null;
			float? argumentProvidedSpeed = (speedValue is float speed) ? (float?)speed : null;
			// Scope scope = original.getAnimationImplementation(null, null, null);



			if (animationImplementation is AnimationConfig.Imported imported) {

				// As OOO says:
				// The transforms for each target, each frame.
				Transform3D[][] transforms = imported.transforms;

				// So presumably this means that
				// First dimension is the frame number, second dimension is the transform for each target.
				float fps = imported.rate;

				if (argumentProvidedSpeed.HasValue) {
					fps *= argumentProvidedSpeed.Value;
				}

				Animation animation = new Animation(name);
				int numIterations = transforms.Length;
				if (imported.skipLastFrame)
					numIterations--;

				SKAnimatorToolsProxy.IncrementEnd(numIterations);
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
					SKAnimatorToolsProxy.IncrementProgress();
				}

				foreach (Model3D model in attachToModels) {
					model.Animations.Add(animation);
				}

			} else if (animationImplementation is AnimationConfig.Sequential sequential) {
				//AnimationConfig.Implementation[] subs = new AnimationConfig.Implementation[sequential.animations.Length];

				SKAnimatorToolsProxy.IncrementEnd(sequential.animations.Length);
				for (int index = 0; index < sequential.animations.Length; index++) {
					AnimationConfig.ComponentAnimation component = sequential.animations[index];
					HandleAnimationImplementation(srcConfig, name, original, Dereference(component), attachToModels, source);
					SKAnimatorToolsProxy.IncrementProgress();
				}

			} else if (animationImplementation is AnimationConfig.Procedural proc) {

				AnimationConfig.TargetTransform[] targets = proc.transforms;
				Animation animation = new Animation(name);
				float timeIncrement = proc.duration / targets.Length;
				if (argumentProvidedSpeed.HasValue) {
					// new behavior:
					if (argumentProvidedSpeed.Value > 0) {
						timeIncrement /= argumentProvidedSpeed.Value;
					} else if (argumentProvidedSpeed.Value < 0) {
						XanLogger.WriteLine("Speed was negative on a procedural animation, this is not yet supported. Using absolute value of speed... This animation will play backwards relative to what you see in-game!");
						timeIncrement /= -argumentProvidedSpeed.Value;
					}
				}
				int currentFrame = 0;
				// Scope scope = new SimpleReferenceScope(source);
				foreach (AnimationConfig.TargetTransform targetTrs in targets) {
					Transform3DExpression expr = targetTrs.expression;

					for (int bakeNFrames = 0; bakeNFrames < 60; bakeNFrames++) {
						Transform3D transform = Transform3DExpressionHandler.Compute(expr, currentFrame);
						if (transform == null) {
							XanLogger.WriteLine("Expression evaluation came out to a null animation! Setting to identity transform.", XanLogger.DEBUG);
							transform = new Transform3D(0);
						}

						Animation.Keyframe keyframe = new Animation.Keyframe();
						for (int targetIndex = 0; targetIndex < targetTrs.targets.Length; targetIndex++) {

							string target = targetTrs.targets[targetIndex];
							if (string.IsNullOrEmpty(target))
								target = argumentProvidedNode;
							if (bakeNFrames == 0) XanLogger.WriteLine("Animation target node: " + target, color: Color.Blue);
							keyframe.Keys.Add(new Animation.Key {
								Node = target,
								Transform = transform
							});
							keyframe.Time = currentFrame * timeIncrement;
						}
						animation.Keyframes.Add(keyframe);
						currentFrame++;
					}
				}

				foreach (Model3D model in attachToModels) {
					model.Animations.Add(animation);
				}
			} else {
				XanLogger.WriteLine(string.Format(ERR_IMPL_NOT_SUPPORTED, animationImplementation.GetType().Name), color: Color.DarkGoldenrod);
			}

			SKAnimatorToolsProxy.IncrementProgress();
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
