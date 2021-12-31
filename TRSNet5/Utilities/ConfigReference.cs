﻿using OOOReader.Clyde;
using OOOReader.Reader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// A wrapper-implementation of OOO's ConfigReference type, which is a way to reference another file without loading it until it's needed.
	/// </summary>
	public class ConfigReference {

		/// <summary>
		/// Returns a new <see cref="ConfigReference"/> pointing to no file, with an empty name, and no arguments.
		/// </summary>
		public static ConfigReference Empty => new ConfigReference();
		
		/// <summary>
		/// Whether or not this <see cref="ConfigReference"/> was created via referencing <see cref="Empty"/>.
		/// </summary>
		public readonly bool IsEmpty = false;

		/// <summary>
		/// Whether or not this <see cref="ConfigReference"/> has been resolved (that is, <see cref="Resolve"/> has been called).
		/// </summary>
		public bool Resolved { get; protected set; } = false;

		/// <summary>
		/// The name of this reference, which is either the file path to it (relative to rsrc) or a prenamed config object as seen within rsrc/config's
		/// many files.
		/// </summary>
		public string Name { get; protected set; } = string.Empty;

		/// <summary>
		/// An actual <see cref="FileInfo"/> to the file this <see cref="ConfigReference"/>, including named configs.<para/>
		/// <strong>This will be <see langword="null"/> until <see cref="Resolve"/> is called.</strong>
		/// </summary>
		public FileInfo? FileReference { get; protected set; }

		/// <summary>
		/// The arguments that will be applied to the referenced model's parameters once instantiated.
		/// </summary>
		public Dictionary<string, object?> Arguments { get; } = new Dictionary<string, object?>();

		/// <summary>
		/// The original <see cref="ShadowClass"/> whose <see cref="ShadowClass.Signature"/> is <c>com.threerings.config.ConfigReference</c>.
		/// </summary>
		private readonly ShadowClass Original;

		/// <summary>
		/// Construct a new <see cref="ConfigReference"/> from a <see cref="ShadowClass"/> shadowing a <c>com.threerings.config.ConfigReference</c>
		/// </summary>
		/// <param name="shadow"></param>
		public ConfigReference(ShadowClass shadow) {
			shadow.AssertIsInstanceOf("com.threerings.config.ConfigReference");
			Name = shadow["_name"]!;
			Dictionary<string, object?> args = ConfigReferenceResolver.GetArgumentMap(shadow["_arguments"]!);
			foreach (KeyValuePair<string, object?> argData in args) {
				Arguments[argData.Key] = argData.Value;
			}
			Original = shadow;
		}

		private ConfigReference() : this(ShadowClass.CreateInstanceOf("com.threerings.config.ConfigReference")) {
			IsEmpty = true;
		}

		/// <summary>
		/// Resolve this <see cref="ConfigReference"/>, returning the <see cref="ShadowClass"/> from the file it referenced. This will return null if it could not be resolved.
		/// </summary>
		/// <remarks>
		/// Under normal Clyde engine behavior, the returned <see cref="ShadowClass"/> should either directly or indierectly extend <c>com.threerings.config.ParameterizedConfig</c>.
		/// </remarks>
		/// <returns></returns>
		public ShadowClass? Resolve() {
			if (IsEmpty) return null;
			(ShadowClass? retn, FileInfo? target) = ConfigReferenceResolver.ResolveConfigReference(Original);
			FileReference = target;
			Resolved = true;
			return retn;
		}

	}
}
