using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.Utilities.Parameters.Implementation {
	public class Direct : Parameter {

		public string[] Paths { get; set; }

		/// <summary>
		/// Corresponding directly to <see cref="Paths"/>, this is the value of what each path pointed to.<para/><para/>
		/// Please note that choosing between one of these for the "right value" to <strong>read from</strong> is a non-problem. Think of a direct's paths like a list of instructions.
		/// If I tell you to write your name on 5 different papers, your name isn't going to change between those pages, so if I need to find your name, I can pick any page I want and still get the right value.
		/// Likewise, you can pick anything out of this and it'll be the "right value" you are looking for.<para/><para/>
		/// If you are feeling particularly indecisive, you can just reference <see cref="Value"/>.<para/><para/>
		/// If you need to set the values however, overriding <em>all</em> entries is required, and this can be done with <see cref="SetAllValuesTo(object)"/>.
		/// </summary>
		public DirectPointer[] Pointers { get; }

		/// <summary>
		/// Returns the value of the first <see cref="DirectPointer"/> this modifies.
		/// </summary>
		/// <remarks>
		/// The actual selected value out of <see cref="Pointers"/> does not matter, hence why this loosely picks the first. See docs on <see cref="Pointers"/> for more information.
		/// </remarks>
		public object? Value => ValuePointer?.Value;

		/// <summary>
		/// The <see cref="DirectPointer"/> to the value this <see cref="Direct"/> represents.<para/>
		/// <strong>Note:</strong> Do <em>NOT</em> use this to modify the value of this <see cref="Direct"/>! To change the value of this <see cref="Direct"/> properly and not create a critical desynchronization, use <see cref="SetAllValuesTo(object)"/>
		/// </summary>
		/// <returns></returns>
		public DirectPointer? ValuePointer => Pointers.FirstOrDefault();

		public Direct(ShadowClass parameterizedConfig, ShadowClass shadow) : base(parameterizedConfig) {
			shadow.AssertIsInstanceOf("com.threerings.config.Parameter$Direct");
			Name = shadow["name"]!;
			Paths = shadow["paths"]!;
			List<DirectPointer> results = new List<DirectPointer>();
			foreach (string path in Paths) {
				results.Add(Traverse(path));
			}
			Pointers = results.ToArray();
		}

		/// <summary>
		/// Modifies all <see cref="Pointers"/> to be the new value, ensuring that the new value is applied without desynchronizing this <see cref="Direct"/>'s values.
		/// </summary>
		/// <param name="value"></param>
		public void SetAllValuesTo(object? value) {
			foreach (DirectPointer ptr in Pointers) {
				ptr.Value = value;
			}
		}

		private DirectPointer Traverse(string path) {
			// Create an array of every element in the path.
			path = path.Replace("\"]", "\"]."); // This helps with chained refs e.g. ["Texture"]["File"] by splitting them.
												// ^ These need to be split because if we don't, it splits as something like asdfg["some"]["thing"] and that double-ending will break the system.

			while (path.EndsWith(".")) path = path.Substring(0, path.Length - 1); // Clear trailing periods that may have been created by ^
			while (path.Contains("..")) path = path.Replace("..", ".");
			string[] pathElements = path.Split('.');

			// Direct paths are composed of three types of accessors, though they can be grouped into two major types:
			// Indices by name or array index:
			//		These look like something.somethingelse (separated with a dot) or something[0] (accessing index #0).
			//		When handled, these can be traversed verbatim via reflection, with a number of name swaps occurring
			//		(namely, snake case names get converted to camel case)
			//
			// Direct x-refs:
			//		Considerably more complicated, these look like thingy["SomethingElse"]
			//		When this occurs, ["SomethingElse"] references a parameter on thingy, which means that its own directs may need traversal.
			//		For obvious reasons, this is where things get very very complicated.
			//		The basic gist is that I need to hot-swap ["SomethingElse"] for the (or all of the, depending on if there's multiple) paths in the
			//		referenced parameter's paths.


			// UPD: From the first iteration of ThreeRingsSharp, which used actual Clyde code, this had to do a lot of hacky reflection garbage.
			// With the transition to OOOReader (my homebrewed solution to reading Clyde files), a new, highly versatile class "ShadowClass" has been
			// introduced. ShadowClass offers an indexer (["this"]) to access its fields. Consequently, this means I can just lazily represent
			// the current element as a dynamic, and then it just works(tm) -- if it's an object, it's guaranteed to be a ShadowClass, so `[]` will
			// index a field just like normal. If it's an array, then `[]` will index an element in said array. Win-win.
			// It allowed me to completely nuke ReflectionHelper

			dynamic elementOrImplementation = ParameterizedConfig;
			dynamic? secondToLast = null;
			for (int idx = 0; idx < pathElements.Length; idx++) {
				string currentPathElement = pathElements[idx];

				// PREREQ: Is it an indexed path element?
				// If so, is it a simple array access or a parameter x-ref?
				// We care about the latter.
				if (IsIndexedPathElement(currentPathElement)) {
					(string element, string index) = GetIndexedElementAndParameter(currentPathElement);
					if (int.TryParse(index, out int numericIndex)) {
						// This is an array access. Phew!
						// Let's do a 2-for-1 by indexing this element and then accessing that as an array, and set the current element ref to the result of that
						secondToLast = elementOrImplementation[element.SnakeToCamel()];
						elementOrImplementation = secondToLast[numericIndex];
						continue; // Go to the next iteration.
					}

					// https://youtu.be/9q_rYCEUYfM?t=623
					index = index.BetweenQuotes()!;
					dynamic referencedElement = elementOrImplementation[element.SnakeToCamel()];
					if (referencedElement is ShadowClass shadow) {
						if (shadow.IsA("com.threerings.config.ConfigReference")) {
							ShadowClass mgCfg = ConfigReferenceResolver.ResolveConfigReference(shadow)!;
							mgCfg.AssertIsInstanceOf("com.threerings.config.ParameterizedConfig");

							ShadowClass? parameter = null;
							foreach (ShadowClass shd in mgCfg["parameters"]!) {
								if (shd["name"] == index) {
									parameter = shd;
									break;
								}
							}
							if (parameter != null) {
								if (parameter.IsA("com.threerings.config.Parameter$Choice")) {
									parameter = parameter["directs"]![0];
								}

								if (parameter.IsA("com.threerings.config.Parameter$Direct")) {
									// This is where things get STANKY.
									// Snag the first direct path. We're interested in simply getting the value out of it,
									// and since a direct with multiple paths will simply apply the change to every single path,
									// we can literally just pick a random one and the result won't be different. So just pick the first.
									string substitutePath = parameter["paths"]![0];

									// Now, take our original path we have right now, but replace our param reference ["Parameter"] with this actual path.
									// More on what this is doing in the huge comment block below. It's safe to skip to reading that.
									path = path.Replace(currentPathElement, element + "." + substitutePath);
									path = path.Replace(".[\"", "[\"");
									// ^ This will undo what was done up on line 28 in case something changed from
									// ["some"]["thing"] => ["some"].["thing"] to blahblahblah.["thing"] which is wrong.
									// And then we re-do it to update it basically, so that any new splits properly count for double references like that.
									path = path.Replace("\"]", "\"].");
									while (path.EndsWith(".")) path = path.Substring(0, path.Length - 1);
									while (path.Contains("..")) path = path.Replace("..", ".");
									// And once again, remove any trailing dots.

									// And now we overwrite the existing pathElements array with a new one from our newly composed path.
									pathElements = path.Split('.');
									secondToLast = elementOrImplementation;
									elementOrImplementation = mgCfg;

									// Read closely!
									// Basically what I'm doing here is hotswapping out the config reference as we go along.
									// Let's use gremlin/null/model.dat for an example, namely it's texture parameter.
									// Take this:
									//	implementation.material_mappings[0].material["Texture"]["File"]
									// Say we're on material["Texture"] right now (currentPathElement=material["Texture"])
									// So what we do is replace the current path element (again, material["Texture"]) with the first path
									// stored in whatever ["Texture"] is.
									// So we read the first path of the referenced parameter. In that case, it's:
									//	implementation.techniques[0].enqueuer.passes[0].texture_state.units[0].texture
									// So again, we take ["Texture"] and swap it out for that path ^^^
									// This gives us:
									//	implementation.material_mappings[0].material.implementation.techniques[0].enqueuer.passes[0].texture_state.units[0].texture["File"]
									// Now, we split the path again just like when we first called this method.
									// Cool. Now, we'll be on the same index, and when we go to the next iteration, it carries on like nothing happened,
									// traversing into techniques[0] of our material none the wiser.
									// For all the system cares, it was just a really convenient path to follow.

									// I can get away with this because we're not relying on actively storing anything along the way, only getting the value out.
									continue;
								}
							}
						}
					}
					
					Debugger.Break();
					throw new Exception();
				}

				// Standard path.
				secondToLast = elementOrImplementation;
				elementOrImplementation = elementOrImplementation[currentPathElement.SnakeToCamel()];
			}

			return new DirectPointer(secondToLast, pathElements[^1].SnakeToCamel(), path);
		}

		/// <summary>
		/// Returns whether or not the given path element is indexed, or, ends in square brackets to index an array element or parameter.
		/// </summary>
		/// <param name="pathElement"></param>
		/// <returns></returns>
		private bool IsIndexedPathElement(string pathElement) {
			// .+(\["?.+"?\])
			return Regex.IsMatch(pathElement, ".+(\\[\"?.+\"?\\])");
		}

		/// <summary>
		/// Given a path element with an index on the end (such as <c>material_mappings[0]</c> or <c>material["Texture"]</c>, this returns a pair of strings.<para/>
		/// Take the example <c>material["Texture"]</c>. This returns:
		/// <list type="number">
		/// <item>material</item>
		/// <item>"Texture" (quotes included in the returned string)</item>
		/// </list>
		/// </summary>
		/// <param name="pathElement"></param>
		/// <returns></returns>
		private (string, string) GetIndexedElementAndParameter(string pathElement) {
			Match target = Regex.Match(pathElement, "(.+)(\\[\"?.+\"?\\])");
			return (target.Groups[1].Value, target.Groups[2].Value.BetweenBrackets())!;
			// Friendly self-reminder that groups[0] is the whole match.
		}

		/// <summary>
		/// Points to a value pointed at by a <see cref="Direct"/>. This is, hence its name, best compared to a pointer.<para/>
		/// When a Direct in OOO points to something's value, that Direct becomes a means of reading and writing to that value. As such, this class aims
		/// to mirror that behavior in the most minimal way possible. This class does not care about the path or parameters that stand between the model and its affected property.
		/// This class exclusively provides access to the value.<para/>
		/// Note that this behaves as more than a simple container, as it actually <em>modifies</em> the value on the model if you change it through this class.
		/// </summary>
		public sealed class DirectPointer {

			private readonly ShadowClass Container;

			private readonly string FieldName;

			/// <summary>
			/// The value pointed at by this <see cref="DirectPointer"/>.
			/// </summary>
			public object? Value {
				get => Container[FieldName];
				set => Container[FieldName] = value;
			}

			/// <summary>
			/// The path that this direct occupies, with its parameter references stripped away.
			/// </summary>
			public string DereferencedPath { get; }

			public DirectPointer(ShadowClass container, string fieldName, string dereferencedPath) {
				Container = container ?? throw new ArgumentNullException(nameof(container));
				FieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
				DereferencedPath = dereferencedPath ?? throw new ArgumentNullException(nameof(dereferencedPath));
			}

		}

	}
}
