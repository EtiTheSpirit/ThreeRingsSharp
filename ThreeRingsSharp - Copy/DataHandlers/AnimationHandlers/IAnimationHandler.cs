using com.threerings.opengl.model.config;
using System.Collections.Generic;
using System.IO;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp.DataHandlers.AnimationHandlers {
	public interface IAnimationHandler {

		/// <summary>
		/// Handles the given <see cref="AnimationConfig"/>.
		/// </summary>
		/// <param name="sourceFile"></param>
		/// <param name="animation"></param>
		/// <param name="targetModel"></param>
		/// <param name="extraData"></param>
		void HandleAnimationConfig(FileInfo sourceFile, AnimationConfig animation, Model3D targetModel, Dictionary<string, dynamic> extraData = null);

	}
}
