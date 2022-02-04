using System;

namespace XDataTree.Data {
	/// <summary>
	/// An enum that represents the available icons in the data tree. Certain images are not part of the stock Silk image package.<para/>
	/// Silk Images come from https://famfamfam.com/ <para/>
	/// To determine the icon of a TreeNode, simply cast one of these enums to an int and set ImageIndex to the int value. As such, this requires that the indices
	/// of images in the TreeNode (see WinForms editor) match up to these.
	/// </summary>
	public enum SilkImage {
		/// <summary>
		/// A blue square representing a generic object. Intended to represent an object of an unknown type.
		/// </summary>
		Generic,

		/// <summary>
		/// An icon of a data tree with a single parent object containing two children. An alternative to <see cref="Generic"/> that serves the same purpose.
		/// </summary>
		Object,

		/// <summary>
		/// A picture of a the globe, intended to represent an entire scene.
		/// </summary>
		Scene,

		/// <summary>
		/// A picture of the sky, intended to represent a skybox object.
		/// </summary>
		Sky,

		/// <summary>
		/// A wooden box representing an entire model. Intended to represent a model.
		/// </summary>
		Model,

		/// <summary>
		/// A set of three bricks, each with a unique color. Intended to represent a set of several models grouped together.
		/// </summary>
		ModelSet,

		/// <summary>
		/// An icon of a person. Intended to represent articulated models, which have bones and often animations, specifically an ArticulatedConfig.
		/// </summary>
		Articulated,

		/// <summary>
		/// An icon of an application window. Intended to represent a billboard GUI element.
		/// </summary>
		Billboard,

		/// <summary>
		/// A single building brick. Intended to represent a static model (just geometry and nothing else), specifically a StaticConfig.
		/// </summary>
		Static,

		/// <summary>
		/// A set of three building bricks. Intended to represent a bulk collection of several static models, specifically a MergedStaticConfig.
		/// </summary>
		MergedStatic,

		/// <summary>
		/// A picture of a speaker. Intended to represent a sound emitter object.
		/// </summary>
		Sound,

		/// <summary>
		/// A picture of a speaker with a warning icon in the lower right. Intended to represent the inability to load a sound.
		/// </summary>
		SoundWarning,

		/// <summary>
		/// A picture of an electric plug. Intended to represent an external object that attaches to another model in its current context, for instance, the Thwacker Gremlin model has an attachment for their hammer. The hammer should use this icon in this context.
		/// </summary>
		Attachment,

		/// <summary>
		/// A picture of two objects with arrows pointing between them. Intended to represent a derived model class, specifically a ModelConfig.Derived.
		/// </summary>
		Derived,

		/// <summary>
		/// A picture of an arrow that diverges. Intended to represent an element that determines conditional attributes in gameplay, specifically a ConditionalConfig.
		/// </summary>
		Conditional,

		/// <summary>
		/// A picture of a camera with a small lightning bolt in the lower right corner. Intended to represent something that affects rendering in some way, or a visual effect.
		/// </summary>
		CameraBolt,

		/// <summary>
		/// A picture of a PC with a small plus symbol in the lower right corner. Intended to represent a model that is generated on the fly from a set of parameters.
		/// </summary>
		Generated,

		/// <summary>
		/// A picture of an application window with a small pencil in the lower right corner. Intended to represent a schemed object, specifically a ModelConfig.Schemed.
		/// </summary>
		Schemed,

		/// <summary>
		/// A picture of a single building brick with a small pencil in the lower right corner. Intended to represent a schemed model, specifically a ModelConfig.SchemedModel.
		/// </summary>
		SchemedModel,

		/// <summary>
		/// A picture of a computer monitor with a small pencil in the lower right corner. This is intended to represent an actual render scheme. This should be used on any instances of a class like ViewerEffectConfig that have no model reference and are direct descendants to a ModelConfig.Schemed.
		/// </summary>
		Scheme,

		/// <summary>
		/// A picture of a snippet of film. Intended to represent an animation, specifically a AnimationConfig.
		/// </summary>
		Animation,

		/// <summary>
		/// Intended to represent a scene object that has code running within it, usually for gameplay purposes.
		/// </summary>
		Scripted,

		/// <summary>
		/// Intended to represent an extension of <see cref="Scripted"/> that runs on a timer ScriptedConfig.TimeAction
		/// </summary>
		TimeAction,

		/// <summary>
		/// A generic icon of a cog. Used for any extra data that is best described as configuration data.
		/// </summary>
		Config,

		/// <summary>
		/// A generic icon of diagonal hashes. Internally this is used to represent fog.
		/// </summary>
		Shading,

		/// <summary>
		/// An icon of a hand pointing to a miniature variant of <see cref="ModelSet"/>, intended to represent an external reference to another model.
		/// </summary>
		Reference,

		/// <summary>
		/// An icon of a piece of paper with a blue &lt;&gt; pattern on it.
		/// </summary>
		Array,

		/// <summary>
		/// An icon of a framed picture intended to represent an image texture.
		/// </summary>
		Texture,

		/// <summary>
		/// An icon of a cardboard box intended to represent a variation of a model.
		/// </summary>
		Variant,

		/// <summary>
		/// A picture of a lightbulb. Intended to represent a source of light.
		/// </summary>
		Light,

		/// <summary>
		/// A picture of a hand pointing to a text box containing an arbitrary value.
		/// </summary>
		Value,

		/// <summary>
		/// A blue triangle, intended to represent more rudimentary components of models (e.g. sub-models in a static model).
		/// </summary>
		Triangle,

		/// <summary>
		/// A green square of grass that represents a tile in a TudeySceneModel
		/// </summary>
		Tile,

		/// <summary>
		/// A 3x3 grid of blocks within square brackets [ ] that represents a transformation matrix, intended for Transform3D
		/// </summary>
		Matrix,

		/// <summary>
		/// A row of three colored blocks, red, green, and blue for X, Y, and Z respectively. Intended to represent a Vector3f
		/// </summary>
		Vector3,

		/// <summary>
		/// A row of three colored blocks, red, green, blue, and magenta for X, Y, Z, and W respectively. Intended to represent a Vector4f or Quaternion
		/// </summary>
		Vector4,

		/// <summary>
		/// A transparent red building brick intended to represent a missing asset.
		/// </summary>
		Missing,

		/// <summary>
		/// A tag, like a cardboard price tag that hangs off of an object.
		/// </summary>
		Tag,

		/// <summary>
		/// An orange wrench.
		/// </summary>
		Wrench,

		/// <summary>
		/// Identical to <see cref="Config"/> but with a warning icon on it.
		/// </summary>
		MissingConfig,

		/// <summary>
		/// Identical to <see cref="Value"/> but it has a pencil on it.
		/// </summary>
		Editable,

		/// <summary>
		/// Identical to <see cref="Tag"/> but the inner part is red instead of blue.
		/// </summary>
		RedTag,

		/// <summary>
		/// A transparent space. Note that it is not advised to use this when no icon is applicable. Consider using <see cref="Generic"/> instead.
		/// </summary>
		None
	}
}
