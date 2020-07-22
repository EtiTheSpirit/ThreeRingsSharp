﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// A class containing values to toggle various features. Generally speaking, if a feature is disabled, it is broken.
	/// </summary>
	public static class DevelopmentFlags {

		/// <summary>
		/// If <see langword="true"/>, glTF will export bones. If <see langword="false"/>, glTF will export solid meshes only.
		/// </summary>
		public const bool FLAG_DO_BONE_EXPORTS = false;

		/// <summary>
		/// If <see langword="true"/>, texture files will always be imported into glTF even if it's set to reference in the program's configuration.
		/// </summary>
		public const bool FLAG_ALWAYS_EMBED_TEXTURES = false;

	}
}
