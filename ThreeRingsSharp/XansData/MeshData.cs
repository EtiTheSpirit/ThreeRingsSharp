using com.threerings.math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Structs;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents mesh data. This is referenced by <see cref="Model3D"/>s to determine their mesh data.<para/>
	/// In order to prevent formats that support unqiue objects from exporting the same mesh more than once, this class is used.<para/>
	/// This allows multiple objects to represent the same mesh data rather than recreating it for every individual instance of the model.
	/// </summary>
	public class MeshData : IDisposable, ICloneable<MeshData> {

		protected internal bool Disposed = false;

		/// <summary>
		/// A binding from a <see cref="string"/> identifier to a <see cref="MeshData"/>. This can be used to find existing mesh data for a model.
		/// </summary>
		public static IReadOnlyDictionary<string, MeshData> MeshDataBindings => _MeshDataBindings;
		private static readonly Dictionary<string, MeshData> _MeshDataBindings = new Dictionary<string, MeshData>();

		/// <summary>
		/// A list of every <see cref="MeshData"/> that has been instantiated. Unlike <see cref="MeshDataBindings"/>, should any meshes have the same name (which will not happen under normal circumstances), this will store all instances.
		/// </summary>
		public static IReadOnlyList<MeshData> NonUniqueMeshDataInstances => _NonUniqueMeshDataInstances.AsReadOnly();
		private static readonly List<MeshData> _NonUniqueMeshDataInstances = new List<MeshData>();

		/// <summary>
		/// A list of <see cref="Model3D"/> instances that reference this mesh.
		/// </summary>
		internal readonly List<Model3D> Users = new List<Model3D>();

		/// <summary>
		/// They key in <see cref="MeshDataBindings"/> that corresponds to this <see cref="MeshData"/>.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The vertices that make up this 3D model. Generally speaking, this should be used if the model is not rigged.<para/>
		/// Consider using <see cref="VertexGroups"/> to access the geometry of rigged models, do mind some vertices may be duplicated.
		/// </summary>
		public List<Vector3> Vertices { get; set; } = new List<Vector3>();

		/// <summary>
		/// The normals that make up this 3D model.
		/// </summary>
		public List<Vector3> Normals { get; set; } = new List<Vector3>();

		/// <summary>
		/// The UV coordinates.
		/// </summary>
		public List<Vector2> UVs { get; set; } = new List<Vector2>();

		/// <summary>
		/// The indices that define triangles.
		/// </summary>
		public List<short> Indices { get; set; } = new List<short>();

		/// <summary>
		/// All of the vertex groups in this model, represented as a list of indices. These indices reference <see cref="Vertices"/>.
		/// </summary>
		public List<VertexGroup> VertexGroups { get; set; } = new List<VertexGroup>();

		/// <summary>
		/// A list of every bone name in this model. Unlike the list provided in Clyde geometry, the first element (index 0) of this list is null.<para/>
		/// This allows easier bone indexing when observing vertex groups since all that needs to be done is testing if the name is null (0 denotes "not in a bone group")
		/// </summary>
		public string[] BoneNames { get; set; } = new string[0];

		/// <summary>
		/// The indices for bones. These correspond to an entry in the modified <see cref="BoneNames"/> list.<para/>
		/// If you need to find the bone for a given vertex, search <see cref="VertexGroups"/> instead, as <see cref="VertexGroup"/>s contain bindings to bones.
		/// </summary>
		public int[,] BoneIndices { get; set; } = new int[0, 0];
		// NOTE TO SELF: Yes, these vanilla indices correspond to your modded name list.
		// You said it everywhere else, you say it here: OOO models dictate that a bone index of 0 = "no bone"
		// This means that every *other* index is actually subtracted by 1 to get the position in their vanilla bone name list.
		// Not here. You just have null at the start of the list then everything else corresponds linearly. No -1.

		/// <summary>
		/// The weights for bones.<para/>
		/// If you need to find the weight of a vertex for a given bone, search <see cref="VertexGroups"/> instead.
		/// </summary>
		public float[,] BoneWeights { get; set; } = new float[0, 0];

		/// <summary>
		/// This should be <see langword="true"/> if this has bone data. If it is false, <see cref="ConstructGroups"/> will not do anything.<para/>
		/// Ensure this is only set to <see langword="true"/> if <see cref="BoneNames"/>, <see cref="BoneIndices"/>, and <see cref="BoneWeights"/> are all populated properly.
		/// </summary>
		public bool HasBoneData { get; set; } = false;

		/// <summary>
		/// If true, <see cref="ApplyTransform(Transform3D)"/> has already been called.
		/// </summary>
		public bool HasTransformed { get; private set; } = false;

		/// <summary>
		/// If true, <see cref="ApplyAxialTransformationMod"/> has already been called. Axial transformations change the up axis based on <see cref="Model3D.TargetUpAxis"/>.
		/// </summary>
		public bool HasAxialTransformed { get; private set; } = false;

		/// <summary>
		/// Creates a new <see cref="MeshData"/>.<para/>
		/// WARNING: This <see cref="MeshData"/> will NOT be added to <see cref="MeshDataBindings"/>!
		/// </summary>
		public MeshData() { }

		/// <summary>
		/// Creates a new <see cref="MeshData"/> and assigns <see cref="MeshDataBindings"/>[<paramref name="name"/>] to the new instance.
		/// </summary>
		/// <param name="name">The name to assign.</param>
		/// <exception cref="ArgumentNullException">If <paramref name="name"/> is null.</exception>
		public MeshData(string name) {
			Name = name ?? throw new ArgumentNullException("name");
			_MeshDataBindings[name] = this;
			_NonUniqueMeshDataInstances.Add(this);
		}

		/// <summary>
		/// Iterates through <see cref="VertexGroups"/> and returns the first <see cref="VertexGroup"/> whose <see cref="VertexGroup.Name"/> is equal to <paramref name="name"/>, or <see langword="null"/> if one could not be found.
		/// </summary>
		/// <param name="name">The name to search for.</param>
		/// <returns></returns>
		/// <exception cref="ObjectDisposedException">If this <see cref="MeshData"/> has been disposed.</exception>
		public VertexGroup GetVertexGroupByName(string name) {
			if (Disposed) throw new ObjectDisposedException("MeshData");
			return VertexGroups.Where(vtxGroup => vtxGroup.Name == name).FirstOrDefault();
		}

		/// <summary>
		/// Constructs all <see cref="VertexGroup"/> instances automatically.
		/// </summary>
		/// <exception cref="ObjectDisposedException">If this <see cref="MeshData"/> has been disposed.</exception>
		public void ConstructGroups() {
			if (Disposed) throw new ObjectDisposedException("MeshData");
			if (!HasBoneData) return;
			// To reiterate this from GeometryConfigTranslater since this is where looking back at the program is going to get confusing...

			// Consider it literally: boneIndices and boneWeights for bones are vertex *attribute* arrays.
			// This means that we iterate through the indices of the model itself, then...
			// The vertex at vertices[index] is part of up to four bone groups.
			// Why four? Bone indices are quadruplets. The returned indices point to a bone name in the name array.
			// A bone index of 0 means "no associated bone".
			// A note to self: SkinnedIndexedStored geometry contains a bone name list. You have altered this list so that [0] is null, then everything else starts at [1] and after.
			// You did this so that you could check if the returned bone name was null.

			// Apparently, this concept went way over my head in SK Animator Tools and it was a disaster. Part of why the code was so horrifying.

			foreach (string boneName in BoneNames) {
				if (boneName != null) {
					VertexGroups.Add(new VertexGroup(boneName));
				}
			}

			foreach (short index in Indices) {
				int[] boneIndices = BoneIndices.GetSecondDimensionAt(index);
				float[] boneWeights = BoneWeights.GetSecondDimensionAt(index);
				Vector3 point = Vertices[index];

				// Iterate 4x because again, quadruplets.
				for (int idx = 0; idx < 4; idx++) {
					int boneIndex = boneIndices[idx];
					float boneWeight = boneWeights[idx];
					string boneName = BoneNames[boneIndex];
					if (boneName == null) break;
					VertexGroup groupForBone = GetVertexGroupByName(boneName);

					// Populate the group.
					groupForBone.Vertices.Add(new Vertex(point, boneWeight));
					groupForBone.Indices.Add(index);
				}
			}
		}

		/// <summary>
		/// Performs the actual application of a <see cref="Transform3D"/> on this model's data, and also calls <see cref="ApplyAxialTransformationMod"/>.<para/>
		/// This will do nothing if <see cref="HasTransformed"/> is <see langword="true"/>.
		/// </summary>
		/// <param name="transform"></param>
		public void ApplyTransform(Transform3D transform) {
			if (HasTransformed) return;

			// Transform the referenced geometry here.
			for (int idx = 0; idx < Vertices.Count; idx++) {
				Vector3 vtx = transform.transformPoint(Vertices[idx]);
				Vertices[idx] = vtx;
			}

			foreach (VertexGroup vtxGroup in VertexGroups) {
				for (int idx = 0; idx < vtxGroup.Vertices.Count; idx++) {
					Vertex vtx = vtxGroup.Vertices[idx];
					vtx.Point = transform.transformPoint(vtx.Point);
					vtxGroup.Vertices[idx] = vtx;
				}
			}

			// ApplyAxialTransformationMod();

			HasTransformed = true;
		}

		/// <summary>
		/// Rearranges all of the data in this <see cref="MeshData"/> based on <see cref="Model3D.TargetUpAxis"/>.<para/>
		/// This will do nothing if <see cref="HasAxialTransformed"/> is <see langword="true"/>.
		/// </summary>
		[Obsolete("This causes malformed and incorrect exports to occur. It's also now set directly when exporting as to not mutate model data.", true)]
		public void ApplyAxialTransformationMod() {
			if (HasAxialTransformed) return;
			
			// Transform the referenced geometry here.
			for (int idx = 0; idx < Vertices.Count; idx++) {
				Vector3 vtx = Vertices[idx];
				Vertices[idx] = vtx.RotateToAxis(Model3D.TargetUpAxis);
			}

			for (int idx = 0; idx < Normals.Count; idx++) {
				Vector3 vtx = Normals[idx];
				Normals[idx] = vtx.RotateToAxis(Model3D.TargetUpAxis);
			}

			foreach (VertexGroup vtxGroup in VertexGroups) {
				for (int idx = 0; idx < vtxGroup.Vertices.Count; idx++) {
					Vertex vtx = vtxGroup.Vertices[idx];
					vtx.Point = vtx.Point.RotateToAxis(Model3D.TargetUpAxis);
					vtxGroup.Vertices[idx] = vtx;
				}
			}
			
			HasAxialTransformed = true;
		}

		/// <summary>
		/// Clones this <see cref="MeshData"/>.<para/>
		/// This automatically registers the cloned mesh in <see cref="MeshDataBindings"/>.
		/// </summary>
		/// <exception cref="ObjectDisposedException">If this <see cref="MeshData"/> has been disposed.</exception>
		public MeshData Clone() {
			if (Disposed) throw new ObjectDisposedException("MeshData");

			MeshData data = new MeshData(Name + "-Clone") {
				Vertices = Vertices.ShallowClone().ToList(),
				Normals = Normals.ShallowClone().ToList(),
				UVs = UVs.ShallowClone().ToList(),
				Indices = Indices.ShallowClone().ToList(),
				VertexGroups = VertexGroups.ShallowClone().ToList(),
			};

			data.BoneNames = new string[BoneNames.Length];
			data.BoneIndices = new int[BoneIndices.GetLength(0), BoneIndices.GetLength(1)];
			data.BoneWeights = new float[BoneWeights.GetLength(0), BoneWeights.GetLength(1)];
			Array.Copy(BoneNames, data.BoneNames, BoneNames.Length);
			Array.Copy(BoneIndices, data.BoneIndices, BoneIndices.Length);
			Array.Copy(BoneWeights, data.BoneWeights, BoneWeights.Length);

			return data;
		}

		/// <summary>
		/// Calls <see cref="Dispose"/> if <see cref="Users"/> is empty.
		/// </summary>
		internal void DisposeIfNoUsersExist() {
			if (!Disposed && Users.Count == 0) {
				Debug.WriteLine($"MeshData [{Name}] has no users and will be destroyed (It was likely cloned, and this is the original).");
				Dispose();
			}
		}

		public void Dispose() {
			if (Disposed) throw new ObjectDisposedException("MeshData");
			if (_MeshDataBindings.ContainsValue(this)) _MeshDataBindings.Remove(this);
			_NonUniqueMeshDataInstances.Remove(this);
			Vertices = null;
			Normals = null;
			UVs = null;
			Indices = null;
			VertexGroups = null;
			BoneNames = null;
			BoneIndices = null;
			BoneWeights = null;
			Disposed = true;
		}
	}
}
