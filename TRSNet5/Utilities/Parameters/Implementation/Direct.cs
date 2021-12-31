using OOOReader.Reader;
using OOOReader.Utility.Extension;
using SKAnimatorTools.PrimaryInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.Utilities.Parameters.Implementation {
	public class Direct : Parameter {

		/// <summary>
		/// Temporary, and subject to removal: Whether or not to allow direct traversal to initialize ShadowClass or object instances before indexing them.
		/// </summary>
		public static bool AllowInitializingShadows { get; set; } = true;

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
			dynamic? secondToLastElement = null;
			for (int idx = 0; idx < pathElements.Length; idx++) {
				string currentPathElement = pathElements[idx];

				// PREREQ: Is it an indexed path element?
				// If so, is it a simple array access or a parameter x-ref?
				// We care about the latter.
				if (IsIndexedPathElement(currentPathElement)) {
					(string element, string index) = GetIndexedElementAndParameter(currentPathElement);
					string elementName = element.SnakeToCamel();
					if (int.TryParse(index, out int numericIndex)) {
						// This is an array access. Phew!
						// Let's do a 2-for-1 by indexing this element and then accessing that as an array, and set the current element ref to the result of that
						
						secondToLastElement = elementOrImplementation[elementName];
						ShadowClass? templateType = null;
						if (secondToLastElement is ShadowClassArrayTemplate arrayTemplate) {
							if (AllowInitializingShadows) {
								templateType = arrayTemplate.ElementType;
								secondToLastElement = arrayTemplate.NewInstance(numericIndex);
								elementOrImplementation[elementName] = secondToLastElement;
								Debug.WriteLine($"WARNING: Encountered an uninitialized shadow array in direct path \"{path}\"! It has been initialized with the minimum number of elements required, but this could cause issues!");
							} else {
								throw new InvalidOperationException($"A direct ({path}) attempted to traverse {elementName}[{numericIndex}], but {elementName} was an uninitialized shadow array.");
							}
						}
						if (numericIndex >= secondToLastElement.Length) {
							if (secondToLastElement is ShadowClass[] scArray) {
								ShadowClass? instance = scArray.First(sc => sc != null);
								if (instance != null) {
									templateType = instance.TemplateType;
									secondToLastElement = ArrayExtensions.ResizeByReallocationSC(secondToLastElement, templateType, numericIndex + 1);
									elementOrImplementation = secondToLastElement![numericIndex];
									continue; // Go to the next iteration.
								}
							}
							secondToLastElement = ArrayExtensions.ResizeByReallocation(secondToLastElement, numericIndex + 1);
							elementOrImplementation[elementName] = secondToLastElement;
						}
						elementOrImplementation = secondToLastElement![numericIndex];
						continue; // Go to the next iteration.
					}

					// https://youtu.be/9q_rYCEUYfM?t=623
					// Index [] is not numeric, so it's a reference.
					index = index.BetweenQuotes();
					dynamic referencedElement = elementOrImplementation[elementName];
					if (referencedElement is ShadowClass shadow && shadow.IsA("com.threerings.config.ConfigReference")) {
						secondToLastElement = referencedElement;
						ConfigReference asCfgRef = new ConfigReference(shadow);
						elementOrImplementation = asCfgRef.Arguments[index]!;

						continue;
					}
					Debug.WriteLine("WARNING: Failed to resolve Direct \"" + path + "\"!");
					Debugger.Break();
				}


				secondToLastElement = elementOrImplementation;
				// Standard path?
				if (IsPathElementAnIndex(currentPathElement)) {
					currentPathElement = currentPathElement.BetweenBrackets();
					if (int.TryParse(currentPathElement, out int index)) {
						elementOrImplementation = secondToLastElement[index];
						continue;
					} else {
						string strIndex = currentPathElement.BetweenQuotes();
						if (elementOrImplementation is ShadowClass shadow && shadow.IsA("com.threerings.config.ConfigReference")) {
							ConfigReference asCfgRef = new ConfigReference(shadow);
							elementOrImplementation = asCfgRef.Arguments[strIndex]!;
							continue;
						} else {
							Debug.WriteLine("WARNING: Failed to resolve Direct \"" + path + "\"!");
							Debugger.Break();
						}
					}
				} else {
					elementOrImplementation = elementOrImplementation[currentPathElement.SnakeToCamel()];
				}
			}
			string fieldName = pathElements[^1].SnakeToCamel().BetweenBrackets().BetweenQuotes();
			if (secondToLastElement is ShadowClass scLastElement) {
				return new DirectPointer(fieldName, path, scLastElement);
				//} else if (secondToLastElement is ShadowClass[] scArray) {
				//	return new DirectPointer(fieldName, path, scArray);
			} else if (secondToLastElement is Array otherArray) {
				if (int.TryParse(fieldName, out int index)) {
					return new ArrayDirectPointer(secondToLastElement!, index);
				} else {
					throw new InvalidOperationException($"Unexpected index for the second to last element (value container) of a Direct that pointed to an array. Attempted to turn \"{fieldName}\" into a number to index an array.");
				}
			} else if (secondToLastElement != null && TypeExtensions.IsDictionary(secondToLastElement!.GetType())) {
				// TODO: Do directs actually support maps?
				// After testing in-engine, it's possible for Directs to reference a map of some sort, but the editor does not support displaying this information, 
				// and I have no clue how things like Choice instances handle them. I guess it's good idea to add support for this anyway.
				return new DictionaryDirectPointer(secondToLastElement!, fieldName);
			} else {
				throw new InvalidOperationException($"Unexpected type for the second to last element (value container) of a Direct. Type: {secondToLastElement?.GetType()}");
			}
		}

		/// <summary>
		/// Returns whether or not the given path element is indexed, or, ends in square brackets to index an array element or parameter, e.g. a[0] or a["b"]
		/// </summary>
		/// <param name="pathElement"></param>
		/// <returns></returns>
		private static bool IsIndexedPathElement(string pathElement) {
			// .+(\["?.+"?\])
			return Regex.IsMatch(pathElement, ".+(\\[\"?.+\"?\\])");
		}

		/// <summary>
		/// Returns whether or not the path element is a literal indexer, e.g. [0] or ["Something"]
		/// </summary>
		/// <param name="pathElement"></param>
		/// <returns></returns>
		private static bool IsPathElementAnIndex(string pathElement) {
			return Regex.IsMatch(pathElement, "(\\[\"?.+\"?\\])");
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
		private static (string, string) GetIndexedElementAndParameter(string pathElement) {
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
		public class DirectPointer {

			private readonly bool _isContainerCfgRef;

			private readonly ShadowClass _container;

			private readonly ConfigReference? _asCfgRef;

			private readonly string _fieldName;

			/// <summary>
			/// The value pointed at by this <see cref="DirectPointer"/>.
			/// </summary>
			public virtual object? Value {
				get {
					if (_isContainerCfgRef) {
						return _asCfgRef!.Arguments[_fieldName];
					} else {
						return _container[_fieldName];
					}
				}
				set {
					if (_isContainerCfgRef) {
						_asCfgRef!.Arguments[_fieldName] = value;
					} else {
						_container[_fieldName] = value;
					}
				}
			}

			/// <summary>
			/// The path that this direct occupies, with its parameter references stripped away.
			/// </summary>
			public string Path { get; }

			public DirectPointer(string fieldName, string path, ShadowClass container) {
				_container = container;
				_fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
				Path = path ?? throw new ArgumentNullException(nameof(path));
				if (_container.IsA("com.threerings.config.ConfigReference")) {
					_isContainerCfgRef = true;
					_asCfgRef = new ConfigReference(_container);
				} else {
					_isContainerCfgRef = false;
					_asCfgRef = null;
				}
			}

#pragma warning disable CS8618, CS8625
			protected internal DirectPointer() {
				_container = null;
				_asCfgRef = null;
				_fieldName = null;
				Path = string.Empty;
			}
#pragma warning restore CS8618, CS8625
		}

		/// <summary>
		/// A variation of <see cref="DirectPointer"/> for use when the last element of a direct is an array.
		/// </summary>
		public class ArrayDirectPointer : DirectPointer {

			private readonly Array _indexedArray;

			private readonly int _index;

			//private readonly bool _isShadowArray = false;

			public override object? Value {
				get => _indexedArray.GetValue(_index);
				set {
					try {
						_indexedArray.SetValue(value, _index);
					} catch (InvalidCastException) {
						/*
						if (Value is ShadowClass sc) {
							if (sc.IsA("com.threerings.opengl.renderer.config.ColorizationConfig")) {
								
							}
						}*/
						Debug.WriteLine($"Failed to store value \"{value}\" ({value?.GetType()}) in the place of an instance of {Value?.GetType()}");
					}
				}
			}

			public ArrayDirectPointer(Array indexedArray, int index) {
				_indexedArray = indexedArray;
				_index = index;
				//if (indexedArray.GetType().GetElementType() == typeof(ShadowClass)) {
				//	_isShadowArray = true;
				//}
			}

		}

		public class DictionaryDirectPointer : DirectPointer {

			private readonly IDictionary _indexedDictionary;

			private readonly object _indexerObject;

			public override object? Value {
				get => _indexedDictionary[_indexerObject];
				set => _indexedDictionary[_indexerObject] = value;
			}

			public DictionaryDirectPointer(IDictionary indexedDictionary, object indexerObject) {
				_indexedDictionary = indexedDictionary;
				_indexerObject = indexerObject;
			}

		}

		public class FaultyDirectPointer : DirectPointer {

			public FaultyDirectPointer() : base() { }

			private object? _valueInternal = null;

			public override object? Value { get => _valueInternal; set => _valueInternal = value; }

		}

	}
}
