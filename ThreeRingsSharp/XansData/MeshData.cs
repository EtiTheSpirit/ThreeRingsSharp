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
using static com.threerings.opengl.model.config.ArticulatedConfig;
using com.threerings.opengl.model.config;

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
		internal readonly List<Model3D> _Users = new List<Model3D>();

		/// <summary>
		/// A list of <see cref="Model3D"/> instances that reference this mesh.
		/// </summary>
		public IReadOnlyList<Model3D> Users => _Users;

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
		public List<ushort> Indices { get; set; } = new List<ushort>();

		/// <summary>
		/// All of the vertex groups in this model, represented as a list of indices. These indices reference <see cref="Vertices"/>.
		/// </summary>
		public List<VertexGroup> VertexGroups { get; set; } = new List<VertexGroup>();

		/// <summary>
		/// A list of every bone name in this model that deforms the model (so NOT all of the bones)! Unlike the list provided in Clyde geometry, the first element (index 0) of this list is null.<para/>
		/// This allows easier bone indexing when observing vertex groups since all that needs to be done is testing if the name is null (0 denotes "not in a bone group")
		/// </summary>
		public string[] BoneNames { get; set; } = new string[0];

		/// <summary>
		/// A list of the bones within <see cref="AllBones"/> that are not listed in <see cref="BoneNames"/>
		/// </summary>
		public string[] ExtraBoneNames { get; set; } = new string[0];

		/// <summary>
		/// The indices for bones. These correspond to an entry in the modified <see cref="BoneNames"/> list.<para/>
		/// If you need to find the bone for a given vertex, search <see cref="VertexGroups"/> instead, as <see cref="VertexGroup"/>s contain bindings to bones.
		/// </summary>
		public ushort[,] BoneIndices { get; set; } = new ushort[0, 0];
		// NOTE TO SELF: Yes, these vanilla indices correspond to your modded name list.
		// You said it everywhere else, you say it here: OOO models dictate that a bone index of 0 = "no bone"
		// This means that every *other* index is actually subtracted by 1 to get the position in their vanilla bone name list.
		// Not here. You just have null at the start of the list then everything else corresponds linearly. No -1.

		// Thankfully, glTF 2.0 standard says that the limit is 4 bones per vertex.
		// Incidentally, that is exactly how OOO does it. That means the support is literally perfect and it's a 1:1 translation.

		/// <summary>
		/// The weights for bones.<para/>
		/// If you need to find the weight of a vertex for a given bone, search <see cref="VertexGroups"/> instead.
		/// </summary>
		public float[,] BoneWeights { get; set; } = new float[0, 0];

		/// <summary>
		/// The native ushort index array from the model that instantiated this mesh without any processing.
		/// </summary>
		public ushort[] BoneIndicesNative { get; set; } = new ushort[0];

		/// <summary>
		/// The native float weight array from the model that instantiated this mesh without any processing.
		/// </summary>
		public float[] BoneWeightsNative { get; set; } = new float[0];

		/// <summary>
		/// This should be <see langword="true"/> if this has bone data. If it is false, <see cref="ConstructGroups"/> will not do anything.<para/>
		/// Ensure this is only set to <see langword="true"/> if <see cref="BoneNames"/>, <see cref="BoneIndices"/>, and <see cref="BoneWeights"/> are all populated properly.<para/>
		/// As a temporary fix for a number of issues caused by armors, this property's { <see langword="get"/>; } will only be able to return <see langword="true"/> if all of the data associated with bones is correct (so setting it to <see langword="true"/> without correct data will cause this to still return <see langword="false"/>).
		/// </summary>
		public bool HasBoneData {
			get {
				if (!_HasBoneData) return false;

				if (AllBones.Count == 0) return false;
				foreach (string bname0 in BoneNames) if (bname0 != null && !AllBones.ContainsKey(bname0)) return false;
				foreach (string bname1 in ExtraBoneNames) if (bname1 != null && !AllBones.ContainsKey(bname1)) return false;
				
				return true;
			}
			set => _HasBoneData = value;
		}
		private bool _HasBoneData = false;

		/// <summary>
		/// If <see langword="true"/>, <see cref="ApplyTransform(Transform3D)"/> has already been called.
		/// </summary>
		public bool HasTransformed { get; private set; } = false;

		/// <summary>
		/// If <see langword="true"/>, <see cref="ApplyAxialTransformationMod"/> has already been called. Axial transformations change the up axis based on <see cref="Model3D.TargetUpAxis"/>.
		/// </summary>
		public bool HasAxialTransformed { get; private set; } = false;

		/// <summary>
		/// All vertices will be moved by this value when exporting.
		/// </summary>
		public Vector3 VertexOffset { get; set; } = new Vector3();

		/// <summary>
		/// The rig associated with this <see cref="MeshData"/>.
		/// </summary>
		public Armature Skeleton { get; private set; } = null;

		/// <summary>
		/// A list of every bone in <see cref="Skeleton"/> indexable by the names in <see cref="BoneNames"/>
		/// </summary>
		public Dictionary<string, Armature> AllBones { get; } = new Dictionary<string, Armature>();

		/// <summary>
		/// Creates a new blank <see cref="MeshData"/>.<para/>
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
		/// Since this is used by every tile when exporting scenes, it has its own method. This iterates through <see cref="Vertices"/> and then sets <see cref="VertexOffset"/> to the center of their axis-aligned bounding box.
		/// </summary>
		public void SetOffsetToAABBCenter() {
			float maxX = 0;
			float maxY = 0;
			float maxZ = 0;
			float minX = 0;
			float minY = 0;
			float minZ = 0;
			foreach (Vector3 vertex in Vertices) {
				float x = vertex.X;
				float y = vertex.Y;
				float z = vertex.Z;
				if (x > maxX) maxX = x;
				if (y > maxY) maxY = y;
				if (z > maxZ) maxZ = z;
				if (x < minX) minX = x;
				if (y < minY) minY = y;
				if (z < minZ) minZ = z;
			}

			VertexOffset = new Vector3((maxX - minX) / 2f, (maxY - minY) / 2f, (maxZ - minZ) / 2f);
		}

		/// <summary>
		/// Sets <see cref="Skeleton"/> and populates <see cref="AllBones"/>.
		/// </summary>
		/// <param name="root"></param>
		public void SetBones(Node root) => SetBones(Armature.ConstructHierarchyFromNode(root));

		public void SetBones(Armature root) {
			AllBones.Clear();
			Skeleton = root;
			AllBones[root.Name] = root;
			IterateChildren(root);

			int trueIndex = 0;
			// Start at 1 because 0 was made always null.
			for (int index = 1; index < BoneNames.Length; index++) {
				string boneName = BoneNames[index];
				AllBones[boneName].Index = trueIndex;
				trueIndex++;
			}
			// Now do note: BoneNames does NOT include all bones! It strictly includes bones that deform the model.
			// We've assigned the indices there, but we need to add indices to the extra stuff too for proper glTF exports.
			// Let's do that now:
			int extraNameIndex = 0;
			ExtraBoneNames = new string[AllBones.Count - BoneNames.Length + 1];
			foreach (KeyValuePair<string, Armature> boneInfo in AllBones) {
				if (!BoneNames.Contains(boneInfo.Key)) {
					boneInfo.Value.Index = trueIndex;
					ExtraBoneNames[extraNameIndex] = boneInfo.Value.Name;
					trueIndex++;
					extraNameIndex++;
				}
			}
		}

		private void IterateChildren(Armature parent) {
			foreach (Armature child in parent.Children) {
				AllBones[child.Name] = child;
				IterateChildren(child);
			}
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
			VertexGroups.Clear(); // Just in case its a second+ call
			// To reiterate this from GeometryConfigTranslater since this is where looking back at the program is going to get confusing...

			// Consider it literally: boneIndices and boneWeights for bones are vertex *attribute* arrays.
			// This means that we iterate through the indices of the model itself, then...
			// The vertex at vertices[index] is part of up to four bone groups (which are defined as a group of 4 values at boneIndices[index])
			// Why four? Bone indices are quadruplets. The returned indices point to a bone name in the name array.
			// A bone index of 0 means "no associated bone".
			// A note to self: SkinnedIndexedStored geometry contains a bone name list. You have altered this list so that [0] is null, then everything else starts at [1] and after.
			// You did this so that you could check if the returned bone name was null.

			// Apparently, this concept went way over my head in SK Animator Tools and it was a disaster. Part of why the code was so horrifying there.

			// Now as for how this data is organized, you automatically grab this data and split it into vertex groups.
			// A vertex group contains instances of vertices. 

			foreach (string boneName in BoneNames) {
				if (boneName != null) {
					VertexGroups.Add(new VertexGroup(boneName));
				}
			}

			foreach (ushort index in Indices) {
				ushort[] boneIndices = BoneIndices.GetSecondDimensionAt(index);
				float[] boneWeights = BoneWeights.GetSecondDimensionAt(index);
				//Vector3 point = Vertices[index];
				//Vector3 normal = Normals[index];
				//Vector2 uv = UVs[index];

				// Iterate 4x because again, quadruplets.
				for (int idx = 0; idx < 4; idx++) {
					ushort boneIndex = boneIndices[idx];
					float boneWeight = boneWeights[idx];
					string boneName = BoneNames[boneIndex];
					if (boneName == null) return;
					VertexGroup groupForBone = GetVertexGroupByName(boneName);
					groupForBone.IndexIndices.Add(index);

					// Populate the group.
					//groupForBone.Vertices.Add(new Vertex(point, boneWeight, normal, uv));
					//groupForBone.Indices.Add(index);
				}
			}
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
			data.BoneIndices = new ushort[BoneIndices.GetLength(0), BoneIndices.GetLength(1)];
			data.BoneWeights = new float[BoneWeights.GetLength(0), BoneWeights.GetLength(1)];
			data.BoneIndicesNative = BoneIndicesNative.ShallowClone().ToArray();
			data.BoneWeightsNative = BoneWeightsNative.ShallowClone().ToArray();
			Array.Copy(BoneNames, data.BoneNames, BoneNames.Length);
			Array.Copy(BoneIndices, data.BoneIndices, BoneIndices.Length);
			Array.Copy(BoneWeights, data.BoneWeights, BoneWeights.Length);

			return data;
		}

		/// <summary>
		/// Calls <see cref="Dispose"/> if <see cref="_Users"/> is empty.
		/// </summary>
		internal void DisposeIfNoUsersExist() {
			if (!Disposed && _Users.Count == 0) {
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
