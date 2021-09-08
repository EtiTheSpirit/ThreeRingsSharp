using System;
using ThreeRingsSharp.XansData.Exceptions;

namespace ThreeRingsSharp.XansData.IO {
	/// <summary>
	/// A class that can construct <see cref="AbstractModelExporter"/>s. This is used internally for ease of access in <see cref="Model3D"/>, and should not be used on its own.
	/// </summary>
	internal class ModelExporterFactory<TExporter> where TExporter : AbstractModelExporter {

		/// <summary>
		/// Create a new instance of the given <see cref="AbstractModelExporter"/>.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidTypeException">If the user tries to create a <see cref="AbstractModelExporter"/> itself.</exception>
		public virtual TExporter NewInstance() {
			Type exporterType = typeof(TExporter);
			if (exporterType == typeof(AbstractModelExporter)) {
				throw new InvalidTypeException("Cannot directly create an instance of AbstractModelExporter. Create an instance of a class that implements it instead.");
			}

			return (TExporter)Activator.CreateInstance(exporterType)!;
		}
	}
}
