using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using ThreeRingsSharp.ConfigHandlers.ModelConfigs;
using ThreeRingsSharp.XansData;
using XDataTree;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// Represents the context of an opened file.
	/// </summary>
	public class ReadFileContext {

		/// <summary>
		/// The file that was opened to create this.
		/// </summary>
		public FileInfo File { get; }

		/// <summary>
		/// The current transform of the latest model.
		/// </summary>
		public Transform3D CurrentSceneTransform { get; set; }

		/// <summary>
		/// All loaded models and empty nodes.
		/// </summary>
		public List<Model3D> AllModelsAndNodes { get; set; }

		/// <summary>
		/// All loaded models.
		/// </summary>
		public List<Model3D> AllModels { get; set; }

		/// <summary>
		/// All loaded armatures by name.
		/// </summary>
		public Dictionary<string, Armature> AllArmatures { get; set; }

		/// <summary>
		/// Specifically made for implementations of <see cref="StaticSetConfig"/>, this contains a list of all <see cref="Model3D"/>s created from a <see cref="StaticSetConfig"/>'s variants.
		/// </summary>
		/// <remarks>
		/// Use the <see cref="RegisterStaticSetVariantModel"/> method to easily register items to this.
		/// </remarks>
		public Dictionary<ShadowClass, Dictionary<string, List<Model3D>>> StaticSetConfigVariants { get; set; } = new(); // the one time I use this lol

		/// <summary>
		/// The root element representing the currently loaded file(s).
		/// </summary>
		public TreeElement? Root { get; set; } = new RootSubstituteElement();

		/// <summary>
		/// The current parent node for the currently loaded file(s), which is the top of the <see cref="PreviousParentCache"/> <see cref="Stack{T}"/>.
		/// </summary>
		public TreeElement? CurrentParent => PreviousParentCache.Peek();

		/// <summary>
		/// For nested models, this is a stack of the current parent node for a given model. Loading a new model should push something onto this stack,
		/// and finishing the processing on that model should pop something off of this stack.
		/// </summary>
		private Stack<TreeElement> PreviousParentCache { get; } = new Stack<TreeElement>();

		public ReadFileContext(FileInfo file) {
			File = file;
			CurrentSceneTransform = new Transform3D(Transform3D.GENERAL);
			AllModelsAndNodes = new List<Model3D>();
			AllModels = new List<Model3D>();
			AllArmatures = new Dictionary<string, Armature>();
			PreviousParentCache.Push(Root);
		}

		/// <summary>
		/// Pushes a new TreeElement onto the stack of parent elements for a given model, which allows for easy chaining of several nested models.
		/// That is, <see cref="CurrentParent"/> will always be the parent node for the current model (where all necessary data for this model goes)
		/// granted this is used properly. If the stack is empty prior to calling this, <see cref="Root"/> is set to the given <paramref name="element"/>.
		/// </summary>
		/// <remarks>
		/// This requires the previous element to allow children.
		/// </remarks>
		/// <param name="element">The element to push onto the stack.</param>
		/// <returns>The element passed into <paramref name="element"/> for chaining.</returns>
		public T Push<T>(T element) where T : TreeElement {
			if (!PreviousParentCache.Any()) {
				Root = element;
			} else {
				PreviousParentCache.Peek().Add(element);
			}
			PreviousParentCache.Push(element);
			return element;
		}

		/// <inheritdoc cref="Push{T}(T)"/>
		/// <remarks>
		/// Unlike the alternate variant of this method, this creates a new <see cref="GenericElement"/> with the given text and default icon (<see cref="SilkImage.Generic"/>).
		/// This requires the previous element to allow children.
		/// </remarks>
		public GenericElement Push(string text, SilkImage icon = SilkImage.Generic) => Push(new GenericElement(text, icon));


		/// <summary>
		/// Pops the latest element off of the stack, or returns null if no elements are on the stack. 
		/// If this call emptied the stack, <see cref="Root"/> will <strong>not</strong> be unset.
		/// </summary>
		/// <returns></returns>
		public TreeElement? Pop() {
			if (PreviousParentCache.TryPop(out TreeElement? result)) {
				return result;
			}
			return null;
		}

		/// <summary>
		/// Register an entity to <see cref="StaticSetConfigVariants"/> by its parent <see cref="StaticSetConfig"/>, the name of the submodel, and one of the submodel's meshes (or the only mesh it has, if applicable).<para/>
		/// This also sets information on the <see cref="Model3D"/> stating that it's part of a <see cref="StaticSetConfig"/>.
		/// </summary>
		/// <param name="staticSetConfig"></param>
		/// <param name="variantName"></param>
		/// <param name="model"></param>
		public void RegisterStaticSetVariantModel(ShadowClass staticSetConfig, string variantName, Model3D model) {
			staticSetConfig.AssertIsInstanceOf("com.threerings.opengl.model.config.StaticSetConfig");
			if (!StaticSetConfigVariants.TryGetValue(staticSetConfig, out Dictionary<string, List<Model3D>>? namedContainer)) {
				namedContainer = new Dictionary<string, List<Model3D>>();
				StaticSetConfigVariants[staticSetConfig] = namedContainer;
			}
			if (!namedContainer!.TryGetValue(variantName, out List<Model3D>? models)) {
				models = new List<Model3D>();
				StaticSetConfigVariants[staticSetConfig][variantName] = models;
			}
			model.ExtraData["Parent"] = staticSetConfig;
			model.ExtraData["ContainingVariantName"] = variantName;
			models!.Add(model);
		}
	}
}
