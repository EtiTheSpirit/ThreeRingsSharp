using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OOOReader.Clyde;
using OOOReader.Reader;
using OOOReader.Utility.Mathematics;
using ThreeRingsSharp.ConfigHandlers.ModelConfigs;
using ThreeRingsSharp.XansData;
using XDataTree;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.Utilities {

	/// <summary>
	/// Represents the context of an opened file. Since files may be read in a chain (that is, file A might reference B and C, which reference D, E, F, and G, so on),
	/// this contains the cumulative data of all read operations for every file together. A new instance of this should be created and passed in when reading data out of
	/// a <see cref="ShadowClass"/> acquired from a <see cref="ClydeFile"/>.<para/>
	/// This class additionally manages the data tree display once the model has loaded. To do this, it uses a stack-based methodology to add items to the tree. Remember
	/// that there are two trees in TRS, one for the loaded models, and the other for the properties of a given loaded model.
	/// </summary>
	public sealed class ReadFileContext : IDisposable {

		/// <summary>
		/// The file that was opened to create this <see cref="ReadFileContext"/> originally.
		/// </summary>
		public FileInfo OriginalFile { get; }

		/// <summary>
		/// The latest file that this <see cref="ReadFileContext"/> has been used as a container for (useful for <see cref="ConfigReference"/>s).
		/// </summary>
		public FileInfo File { get; set; }

		/// <summary>
		/// The current transform of the latest model.
		/// </summary>
		public Transform3D CurrentSceneTransform { get; set; }

		/// <summary>
		/// All loaded models and empty nodes.
		/// </summary>
		public List<Model3D> AllModelsAndNodes { get; }

		/// <summary>
		/// All loaded models. Unlike <see cref="AllModelsAndNodes"/>, this does not include empty objects.
		/// </summary>
		public List<Model3D> AllModels { get; }

		/// <summary>
		/// All loaded armatures by name.
		/// </summary>
		public Dictionary<string, Armature> AllArmatures { get; }

		/// <summary>
		/// A reference to the top of <see cref="AttachmentNodes"/>, or null if it's empty.
		/// </summary>
		public Armature? CurrentAttachmentNode {
			get {
				if (AttachmentNodes.TryPeek(out Armature? latest)) {
					return latest;
				}
				return null;
			}
		}

		/// <summary>
		/// For use when loading an ArticualtedConfig's attachments or components. This is a stack of the latest root node that an attached or component model might reference.
		/// </summary>
		public Stack<Armature> AttachmentNodes { get; } = new Stack<Armature>();

		/// <summary>
		/// Specifically made for implementations of <see cref="StaticSetConfig"/>, this contains a list of all <see cref="Model3D"/>s 
		/// created from a <see cref="StaticSetConfig"/>'s variants. The keys here are the <see cref="ShadowClass"/>es representing <see cref="StaticSetConfig"/>s,
		/// and the values are a lookup from variant name => all models associated with that variant.<para/>
		/// 
		/// Use <see cref="RegisterStaticSetVariantModel"/> to easily register items to this lookup.
		/// </summary>
		public Dictionary<ShadowClass, Dictionary<string, List<Model3D>>> StaticSetConfigVariants { get; } = new(); // the one time I use this lol

		/// <summary>
		/// The root element representing the currently loaded file(s).
		/// </summary>
		public TreeElement? Root { get; set; } = new RootSubstituteElement();

		/// <summary>
		/// The current parent node for the currently loaded file(s), which is the top of the <see cref="_previousParentCache"/> <see cref="Stack{T}"/>.
		/// </summary>
		public TreeElement? CurrentParent => _previousParentCache.Peek();

		/// <summary>
		/// For nested models, this is a stack of the current parent node for a given model. Loading a new model should push something onto this stack,
		/// and finishing the processing on that model should pop something off of this stack.
		/// </summary>
		private readonly Stack<TreeElement> _previousParentCache = new Stack<TreeElement>();

		/// <summary>
		/// Whether or not any <see cref="StaticSetConfig"/> has ever been loaded. Expensive to reference.
		/// </summary>
		public bool HasStaticSetConfig => AllModels.Where(mdl => mdl.ExtraData.ContainsKey("StaticSetConfig") && ((bool)mdl.ExtraData["StaticSetConfig"] == true)).Any();

		public ReadFileContext(FileInfo file) {
			OriginalFile = file;
			File = file;
			CurrentSceneTransform = Transform3D.NewIdentity();
			AllModelsAndNodes = new List<Model3D>();
			AllModels = new List<Model3D>();
			AllArmatures = new Dictionary<string, Armature>();
			_previousParentCache.Push(Root);
		}

		/// <summary>
		/// An alias to <see cref="Transform3D.ComposeSelf(Transform3D)"/> (called on <see cref="CurrentSceneTransform"/>), but with zero-scale protection.
		/// </summary>
		/// <param name="toApplyToThis"></param>
		public void ComposeTransform(Transform3D toApplyToThis) {
			Vector3f scaleBy = toApplyToThis.Scale;
			if (Model3D.ProtectAgainstZeroScale && scaleBy.LengthSquared() == 0) {
				switch (toApplyToThis.Rank) {
					case Transform3D.UNIFORM:
						toApplyToThis.SetUniformScale(1f);
						break;
					default:
						Debug.WriteLine("Cannot resolve zero scale value: Non-uniform scale detected; op currently not supported.");
						break;
				}
			}

			CurrentSceneTransform.ComposeSelf(toApplyToThis);
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
			if (!_previousParentCache.Any()) {
				Root = element;
			} else {
				_previousParentCache.Peek().Add(element);
			}
			_previousParentCache.Push(element);
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
			if (_previousParentCache.TryPop(out TreeElement? result)) {
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
			model.ExtraData["StaticSetConfig"] = true;
			models!.Add(model);
		}

		/// <summary>
		/// Intended to be called just before export, this sets the Skip flag based on user prefs.
		/// </summary>
		public void UpdateExportabilityOfStaticSets(bool exportOnlyEnabled) {
			if (exportOnlyEnabled) {
				foreach (Model3D mdl in AllModels) {
					if (!mdl.ExtraData.ContainsKey("Parent") || !mdl.ExtraData.ContainsKey("ContainingVariantName")) continue;
					if (mdl.ExtraData["Parent"] is ShadowClass staticSet && mdl.ExtraData["ContainingVariantName"] is string variantOfModel && staticSet.IsA("com.threerings.opengl.model.config.StaticSetConfig")) {
						string variantName = staticSet["model"]!;
						mdl.ExtraData["SkipExport"] = variantName != variantOfModel;
					}
				}
			} else {
				foreach (Model3D mdl in AllModels) {
					if (!mdl.ExtraData.ContainsKey("Parent") || !mdl.ExtraData.ContainsKey("ContainingVariantName")) continue;
					if (mdl.ExtraData["Parent"] is ShadowClass staticSet && staticSet.IsA("com.threerings.opengl.model.config.StaticSetConfig")) {
						mdl.ExtraData["SkipExport"] = false;
					}
				}
			}
		}

		public void Dispose() {
			foreach (Model3D mdl in AllModelsAndNodes) {
				AllModels.Remove(mdl);
				mdl.Dispose();
			}
			foreach (Model3D mdl in AllModels) {
				mdl.Dispose();
			}
			AllModelsAndNodes.Clear();
			AllModels.Clear();
			AllArmatures.Clear();
			AttachmentNodes.Clear();
			StaticSetConfigVariants.Clear();
			Root = null;
		}
	}
}
