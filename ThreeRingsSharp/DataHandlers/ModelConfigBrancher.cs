using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.threerings.opengl.model.config;
using ThreeRingsSharp.DataHandlers.Model.ArticulatedConfigHandlers;
using ThreeRingsSharp.DataHandlers.Model.StaticConfigHandlers;
using ThreeRingsSharp.DataHandlers.Model.StaticSetConfigHandler;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.Utility.Interface;
using ThreeRingsSharp.XansData;

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
		/// The models that have been loaded by this loader, including all referenced assets, if applicable.
		/// </summary>
		public IReadOnlyList<Model3D> Models => _Models.AsReadOnly();
		private List<Model3D> _Models = new List<Model3D>();

		/// <summary>
		/// If <see langword="false"/>, something has gone wrong when parsing models and this <see cref="ModelConfigBrancher"/> is not fit for use.
		/// </summary>
		public bool OK { get; set; } = true;

		/// <summary>
		/// Sends an arbitrary <see cref="ModelConfig"/> into the data brancher and processes it.
		/// </summary>
		/// <param name="sourceFile">The file that the given <see cref="ModelConfig"/> came from.</param>
		/// <param name="model">The <see cref="ModelConfig"/> itself.</param>
		public void HandleDataFrom(FileInfo sourceFile, ModelConfig model) {
			ModelConfig.Implementation implementation = model.implementation;
			if (implementation == null) {
				XanLogger.WriteLine("ALERT: Implementation is null! Sending error.");
				AsyncMessageBox.Show("This specific model does not have an implementation, which is the data for the model itself. The program cannot continue.", "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			string implName = ClassNameStripper.GetBaseClassName(implementation.getClass()) ?? implementation.getClass().getTypeName();
			RootDataTreeObject.Text = implName;

			_Models.Clear();

			/*
			 * if (implementation is ArticulatedConfig) {
				XanLogger.WriteLine("Model is of the type 'ArticulatedConfig'. Accessing handlers...");
				RootDataTreeObject.ImageKey = SilkImage.Articulated;
				ArticulatedConfigHandler.Instance.HandleModelConfig(sourceFile, model, ref _Models, RootDataTreeObject);

			} else 
			*/
			
			if (implementation is StaticConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticConfig'. Accessing handlers...");
				RootDataTreeObject.ImageKey = SilkImage.Static;
				StaticConfigHandler.Instance.HandleModelConfig(sourceFile, model, ref _Models, RootDataTreeObject);

			} else if (implementation is StaticSetConfig) {
				XanLogger.WriteLine("Model is of the type 'StaticSetConfig'. Accessing handlers...");
				RootDataTreeObject.ImageKey = SilkImage.ModelSet;
				StaticSetConfigHandler.Instance.HandleModelConfig(sourceFile, model, ref _Models, RootDataTreeObject);


			} else {
				XanLogger.WriteLine("Known core type, but no code present to handle it. Sending warning.");
				AsyncMessageBox.Show("This specific implementation is valid, but it has no handler! (There's no code that can translate this data for you :c).\nImplementation: " + implementation.getClass().getTypeName(), "Can't Handle Model", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				RootDataTreeObject.ImageKey = SilkImage.Generic;
				OK = false;
			}


		}

		/// <summary>
		/// Saves all models loaded by this <see cref="ModelConfigBrancher"/> in bulk to the same file as defined by <paramref name="toFile"/>.
		/// </summary>
		/// <param name="toFile">The file to write to.</param>
		/// <param name="targetFormat">The target format to write in.</param>
		public void SaveAllToFile(FileInfo toFile, ModelFormat targetFormat) {
			Model3D.ExportIntoOne(toFile, targetFormat, _Models.ToArray());
		}

		internal void HandleDataFrom() {

		}

	}
}
