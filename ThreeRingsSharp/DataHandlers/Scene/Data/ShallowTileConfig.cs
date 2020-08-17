using com.threerings.config;
using com.threerings.opengl.model.config;
using com.threerings.tudey.config;
using com.threerings.tudey.data;
using System;
using System.Collections.Generic;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.DataHandlers.Scene.Data {

	/// <summary>
	/// Represents a <see cref="TileConfig"/> in a non-descriptive manner (that is, as a bare-bones object with the information needed to load the data inside).
	/// </summary>
	[Obsolete("The system can now read actual Tiles, so a shallow config is useless.")] public class ShallowTileConfig {

		/// <summary>
		/// A lookup from <see cref="Name"/> to <see cref="ShallowTileConfig"/> instance.
		/// </summary>
		public static IReadOnlyDictionary<string, ShallowTileConfig> TileLookup => _TileLookup;
		private static readonly Dictionary<string, ShallowTileConfig> _TileLookup = new Dictionary<string, ShallowTileConfig>();

		/// <summary>
		/// The display name of this item, which is what's used in the lookup of <see cref="TileConfig"/>s.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// If the reference points to a <see cref="StaticSetConfig"/>, then this dictates which model from the set is being used.
		/// </summary>
		public string TargetModel { get; }

		/// <summary>
		/// The width of this tile. If this is a derived reference, this will point to the width of the lowest level original tile.<para/>
		/// When attempting to place the tile in the 3D world, consider using <c>TileEntryExtensions</c> and calling <see cref="TileEntryExtensions.GetWidth(TudeySceneModel.TileEntry, ShallowTileConfig)"/>, which accounts for rotation.
		/// </summary>
		public int Width => Derived ? Reference.Width : _Width;
		private readonly int _Width;

		/// <summary>
		/// The height of this tile. If this is a derived reference, this will point to the width of the lowest level original tile.<para/>
		/// When attempting to place the tile in the 3D world, consider using <c>TileEntryExtensions</c> and calling <see cref="TileEntryExtensions.GetHeight(TudeySceneModel.TileEntry, ShallowTileConfig)"/>, which accounts for rotation.
		/// </summary>
		public int Height => Derived ? Reference.Height : _Height;
		private readonly int _Height;

		/// <summary>
		/// If <see cref="Derived"/> is <see langword="true"/>, this is the <see cref="ShallowTileConfig"/> that this points to to get its model reference.<para/>
		/// If <see cref="Derived"/> is <see langword="false"/>, this is <see langword="null"/>.<para/>
		/// Note: It is safe to reference <see cref="ModelPath"/> even if this exists, as it will chain down on its own (so if there's a chain of Derived implementations, it will go down all of them for you). You should reference <see cref="ModelPath"/> if that's the data you want.
		/// </summary>
		public ShallowTileConfig Reference {
			get {
				if (_ReferenceName != null) _Reference = TileLookup.GetOrDefault(_ReferenceName);
				return _Reference;
			}
		}
		private ShallowTileConfig _Reference;
		private readonly string _ReferenceName;

		/// <summary>
		/// This is equivalent to the return value of <see cref="ConfigReference.getName()"/>, which generally returns a filepath relative to the spiral knights rsrc directory (in the case of this property, this is exactly what it does).<para/>
		/// This is always populated even if <see cref="Derived"/> is true (derived implementations don't contain their own model path), because this climbs down any derived chains until it finds an instance of <see cref="ShallowTileConfig"/> where this is set.
		/// </summary>
		public string ModelPath => Derived ? Reference.ModelPath : _ModelPath;
		private readonly string _ModelPath;

		/// <summary>
		/// If <see langword="true"/>, this was declared via a Derived type, and <see cref="Reference"/> is populated.
		/// </summary>
		public bool Derived => Reference != null;

		/// <summary>
		/// Construct a new <see cref="ShallowTileConfig"/>.
		/// </summary>
		/// <param name="name">The name used in the lookup from a <see cref="TudeySceneModel.TileEntry"/>.</param>
		/// <param name="modelOrPointedName">The path to the model this <see cref="ShallowTileConfig"/> references, or if <paramref name="isDerived"/> is true, the name of the <see cref="ShallowTileConfig"/> that this points to.</param>
		/// <param name="modelReferenceForSets">If the reference is a <see cref="StaticSetConfig"/>, this dictates the target default model.</param>
		/// <param name="isDerived">If true, this is a derived <see cref="ShallowTileConfig"/> and points to another instance.</param>
		/// <param name="width">The width of this tile, or null if it is not defined.</param>
		/// <param name="height">The height of this tile, or null if it is not defined.</param>
		private ShallowTileConfig(string name, string modelOrPointedName, string modelReferenceForSets, bool isDerived, int? width, int? height) {
			Name = name;
			TargetModel = modelReferenceForSets;
			if (isDerived) {
				_ReferenceName = modelOrPointedName;
				_ModelPath = null;
			} else {
				_ReferenceName = null;
				_ModelPath = modelOrPointedName;
			}
			_Width = width.GetValueOrDefault(1);
			_Height = height.GetValueOrDefault(1);
		}

		/// <summary>
		/// Construct a new <see cref="ShallowTileConfig"/> and register it in <see cref="TileLookup"/>.
		/// </summary>
		/// <param name="name">The name used in the lookup from a <see cref="TudeySceneModel.TileEntry"/>.</param>
		/// <param name="modelOrPointedName">The path to the model this <see cref="ShallowTileConfig"/> references, or if <paramref name="isDerived"/> is true, the name of the <see cref="ShallowTileConfig"/> that this points to.</param>
		/// <param name="modelReferenceForSets">If the reference is a <see cref="StaticSetConfig"/>, this dictates the target default model.</param>
		/// <param name="isDerived">If true, this is a derived <see cref="ShallowTileConfig"/> and points to another instance.</param>
		/// <param name="width">The width of this tile, or null if it is not defined.</param>
		/// <param name="height">The height of this tile, or null if it is not defined.</param>
		public static ShallowTileConfig FromData(string name, string modelOrPointedName, string modelReferenceForSets = null, bool isDerived = false, int? width = null, int? height = null) {
			ShallowTileConfig instance = new ShallowTileConfig(name, modelOrPointedName, modelReferenceForSets, isDerived, width, height);
			_TileLookup[name] = instance;
			return instance;
		}

	}
}
