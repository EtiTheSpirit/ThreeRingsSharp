using com.threerings.opengl.model.config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
