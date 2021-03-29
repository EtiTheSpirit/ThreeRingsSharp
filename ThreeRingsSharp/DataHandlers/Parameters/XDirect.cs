using com.threerings.config;
using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.Parameters {
	public class XDirect : XParameter {

		/// <summary>
		/// Every path that is a part of this <see cref="XDirect"/>, which is a complex identifier for the data it changes.
		/// </summary>
		public IReadOnlyList<string> Paths { get; }

		/// <summary>
		/// Corresponding directly to <see cref="Paths"/>, this is the value of what each path pointed to.<para/>
		/// Please note that choosing between one of these for the "right value" is a non-problem. Think of a direct's paths like a list of instructions.
		/// If I tell you to write your name on 5 different papers, your name isn't going to change between those pages, so if I need to find your name, I can pick any.
		/// Likewise, you can pick anything out of this and it'll be the "right value" you are looking for.<para/>
		/// If you are feeling particularly indecisive, you can use <see cref="GetValue"/>.
		/// </summary>
		public IReadOnlyList<DirectPointer> Values { get; }

		public XDirect(ParameterizedConfig parent, Parameter.Direct source) : base(parent, source) {
			Paths = source.paths.ToList().AsReadOnly();
			List<DirectPointer> results = new List<DirectPointer>();
			foreach (string path in Paths) {
				results.Add(Traverse(path));
			}
			Values = results.AsReadOnly();
		}

		/// <summary>
		/// Returns the first entry in <see cref="Values"/> (all values are identical), or <see langword="null"/> if there are no values due to <see cref="Paths"/> being empty.
		/// </summary>
		/// <returns></returns>
		public object GetValue() {
			return GetValuePointer().Value;
		}

		/// <summary>
		/// Returns a <see cref="DirectPointer"/> to the value this <see cref="XDirect"/> represents.
		/// </summary>
		/// <returns></returns>
		public DirectPointer GetValuePointer() {
			if (Values.Count == 0)
				return null;
			return Values[0];
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

			object elementOrImplementation = Parent;
			object secondToLast = null;
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
						secondToLast = ReflectionHelper.Get(elementOrImplementation, element.SnakeToCamel());
						elementOrImplementation = ReflectionHelper.GetArray(
							secondToLast,
							numericIndex
						);
						continue; // Go to the next iteration.
					}

					// https://youtu.be/9q_rYCEUYfM?t=623
					index = index.BetweenQuotes();
					object referencedElement = ReflectionHelper.Get(elementOrImplementation, element.SnakeToCamel());
					if (referencedElement is ConfigReference cfgRef) {
						ManagedConfig cfg = (ManagedConfig)cfgRef.ResolveAuto();
						if (cfg is ParameterizedConfig par) {
							Parameter p = par.parameters.FirstOrDefault(param => param.name == index);
							if (p is Parameter.Choice choice) {
								p = choice.directs[0];
							}

							if (p is Parameter.Direct direct) {
								// This is where things get STANKY.
								// Snag the first direct path. We're interested in simply getting the value out of it,
								// and since a direct with multiple paths will simply apply the change to every single path,
								// we can literally just pick a random one and the result won't be different. So just pick the first.
								string substitutePath = direct.paths[0];

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
								elementOrImplementation = par;

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
					Debugger.Break();
					throw new Exception();
				}

				// Standard path.
				secondToLast = elementOrImplementation;
				elementOrImplementation = ReflectionHelper.Get(elementOrImplementation, currentPathElement.SnakeToCamel());
			}

			return new DirectPointer(secondToLast, pathElements[pathElements.Length - 1].SnakeToCamel(), path);
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
			return (target.Groups[1].Value, target.Groups[2].Value.BetweenBrackets());
			// Friendly self-reminder that groups[0] is the whole match.
		}

		/// <summary>
		/// Points to a value pointed at by a <see cref="XDirect"/>. This is, hence its name, best compared to a pointer.<para/>
		/// When a Direct in OOO points to something's value, that Direct becomes a means of reading and writing to that value. As such, this class aims
		/// to mirror that behavior in the most minimal way possible. This class does not care about the path or parameters that stand between the model and its affected property.
		/// This class exclusively provides access to the value.<para/>
		/// Note that this behaves as more than a simple container, as it actually <em>modifies</em> the value on the model if you change it through this class.
		/// </summary>
		public sealed class DirectPointer {

			private readonly object Container;

			private readonly string FieldName;

			/// <summary>
			/// The value pointed at by this <see cref="DirectPointer"/>.
			/// </summary>
			public object Value {
				get => ReflectionHelper.Get(Container, FieldName);
				set => ReflectionHelper.Set(Container, FieldName, value);
			}

			/// <summary>
			/// The path that this direct occupies, with its parameter references stripped away.
			/// </summary>
			public string DereferencedPath { get; }

			public DirectPointer(object container, string fieldName, string dereferencedPath) {
				if (container == null) throw new ArgumentNullException(nameof(container));
				if (fieldName == null) throw new ArgumentNullException(nameof(fieldName));
				if (dereferencedPath == null) throw new ArgumentNullException(nameof(dereferencedPath));
				Container = container;
				FieldName = fieldName;
				DereferencedPath = dereferencedPath;
			}

		}

	}
}
