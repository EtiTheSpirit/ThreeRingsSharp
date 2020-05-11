using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.DataHandlers.Model.ArticulatedConfigHandler;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;

namespace ThreeRingsSharp.DataHandlers {

	/// <summary>
	/// A class that takes in a <see cref="ModelConfig"/>, determines its subtype (e.g. <see cref="ArticulatedConfig"/>, <see cref="StaticConfig"/>, etc.), and handles the data appropriately.
	/// </summary>
	public class ModelConfigBrancher {

		/// <summary>
		/// A <see cref="Func{TResult}"/> that returns a reference to the RootDataTreeObject in the base SKAnimatorTools program. The program's main form will set this on its own.<para/>
		/// Consider referencing <see cref="RootDataTreeObject"/> directly rather than calling <see cref="Func{TResult}.Invoke"/> on this.
		/// </summary>
		public static Func<DataTreeObject> GetRootDataTreeObject { private get; set; }

		/// <summary>
		/// A reference to the <see cref="RootDataTreeObject"/> property of the GUI, which contains the model hierarchy.
		/// </summary>
		public static DataTreeObject RootDataTreeObject => GetRootDataTreeObject.Invoke();

		/// <summary>
		/// Sends an arbitrary <see cref="ModelConfig"/> into the data brancher and processes it.
		/// </summary>
		/// <param name="model"></param>
		public static void HandleDataFrom(ModelConfig model) {
			ModelConfig.Implementation implementation = model.implementation;
			if (implementation == null) {
				XanLogger.WriteLine("ALERT: Implementation is null! Sending error.");
				AsyncMessageBox.Show("This specific model does not have an implementation, which is the data for the model itself. The program cannot continue.", "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string implName = implementation.getClass().getName();
			if (implName.Contains(".")) {
				implName = implName.Substring(implName.LastIndexOf(".") + 1);
			}
			RootDataTreeObject.Text = implName;

			if (implementation is ArticulatedConfig articulatedModel) {
				XanLogger.WriteLine("Model is of the type 'ArticulatedConfig'. Accessing handlers...");
				RootDataTreeObject.ImageKey = SilkImage.Articulated;
				ArticulatedConfigHandler.HandleArticulatedConfig(articulatedModel);

			} else {
				XanLogger.WriteLine("Known core type, but no code present to handle it. Sending warning.");
				AsyncMessageBox.Show("This specific implementation exists, but it has no handler (so while it *is* valid, there's just no code that knows how to work with its data).\nImplementation: " + implementation.getClass().getTypeName(), "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				RootDataTreeObject.ImageKey = SilkImage.Generic;
			}
		}

	}
}
