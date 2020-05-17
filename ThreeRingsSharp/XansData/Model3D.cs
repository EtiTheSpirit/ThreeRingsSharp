using com.google.inject;
using com.threerings.math;
using java.nio.channels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.IO;
using ThreeRingsSharp.XansData.IO.GLTF;
using ThreeRingsSharp.XansData.Structs;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// A unified representation of a model. Since various implementations from Clyde may store data differently, this provides a common interface.
	/// </summary>
	public class Model3D : IDisposable {

		/// <summary>
		/// A list of bindings from <see cref="ModelFormat"/>s to a singleton of their applicable <see cref="AbstractModelExporter"/>.
		/// </summary>
		internal static readonly IReadOnlyDictionary<ModelFormat, dynamic> ExporterBindings = new Dictionary<ModelFormat, dynamic>() {
			[ModelFormat.FBX] = null,
			[ModelFormat.OBJ] = new ModelExporterFactory<OBJExporter>(),
			[ModelFormat.GLTF] = new ModelExporterFactory<GLTFExporter>()
		};

		/// <summary>
		/// The display name for this model, used in exporting (i.e. this is the name that will show up in Blender or any other modelling software.)
		/// </summary>
		public string Name { get; set; } = null;

		/// <summary>
		/// A reference to the file that the model here came from. This is used to reference textures and other path-dependent extra data.
		/// </summary>
		public FileInfo Source { get; set; }

		/// <summary>
		/// The transformation to apply to the model data. By default, this is the identity transformation (so no transform).
		/// </summary>
		public Transform3D Transform { get; set; } = new Transform3D();

		/// <summary>
		/// If true, then <see cref="Transform"/> has been applied to all <see cref="Vertices"/>.
		/// </summary>
		public bool HasDoneTransformation { get; protected internal set; } = false;

		/// <summary>
		/// The vertices that make up this 3D model. Generally speaking, this should be used if the model is not rigged.<para/>
		/// Consider using <see cref="VertexGroups"/> to access the geometry of rigged models, do mind some vertices may be duplicated.
		/// </summary>
		public readonly List<Vector3> Vertices = new List<Vector3>();

		/// <summary>
		/// The normals that make up this 3D model.
		/// </summary>
		public readonly List<Vector3> Normals = new List<Vector3>();

		/// <summary>
		/// The UV coordinates.
		/// </summary>
		public readonly List<Vector2> UVs = new List<Vector2>();

		/// <summary>
		/// The indices that define triangles.
		/// </summary>
		public readonly List<short> Indices = new List<short>();

		/// <summary>
		/// All of the vertex groups in this model, represented as a list of indices. These indices reference <see cref="Vertices"/>.
		/// </summary>
		public readonly List<VertexGroup> VertexGroups = new List<VertexGroup>();

		/// <summary>
		/// A list of every bone name in this model. Unlike the list provided in Clyde geometry, the first element (index 0) of this list is null.<para/>
		/// This allows easier bone indexing when observing vertex groups since all that needs to be done is testing if the name is null (0 denotes "not in a bone group")
		/// </summary>
		public string[] BoneNames = new string[0];

		/// <summary>
		/// The indices for bones. These correspond to an entry in the modified <see cref="BoneNames"/> list.<para/>
		/// If you need to find the bone for a given vertex, search <see cref="VertexGroups"/> instead, as <see cref="VertexGroup"/>s contain bindings to bones.
		/// </summary>
		public int[,] BoneIndices = new int[0,0];
		// NOTE TO SELF: Yes, these vanilla indices correspond to your modded name list.
		// You said it everywhere else, you say it here: OOO models dictate that a bone index of 0 = "no bone"
		// This means that every *other* index is actually subtracted by 1 to get the position in their vanilla bone name list.
		// Not here. You just have null at the start of the list then everything else corresponds linearly. No -1.

		/// <summary>
		/// The weights for bones.<para/>
		/// If you need to find the weight of a vertex for a given bone, search <see cref="VertexGroups"/> instead.
		/// </summary>
		public float[,] BoneWeights = new float[0,0];

		/// <summary>
		/// This should be <see langword="true"/> if this has bone data. If it is false, <see cref="ConstructGroups"/> will not do anything.<para/>
		/// Ensure this is only set to <see langword="true"/> if <see cref="BoneNames"/>, <see cref="BoneIndices"/>, and <see cref="BoneWeights"/> are all populated properly.
		/// </summary>
		public bool HasBoneData { get; set; } = false;

		/// <summary>
		/// Exports this model in a given format, writing the data to the target <see cref="FileInfo"/>
		/// </summary>
		/// <param name="targetFile">The file that will be written to.</param>
		/// <param name="targetFormat">The file format to use for the 3D model.</param>
		public void Export(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.FBX) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(new Model3D[] { this }, targetFile);
		}

		/// <summary>
		/// Takes <see cref="Transform"/> and applies it to <see cref="Vertices"/>, as well as every <see cref="Vertex"/> in each individual <see cref="VertexGroup"/>. This also normalizes the weights of vertices across all vertex groups.<para/>
		/// This can only be called once.
		/// </summary>
		public void ApplyTransformations() {
			if (HasDoneTransformation) return;

			// Transform the referenced geometry here.
			for (int idx = 0; idx < Vertices.Count; idx++) {
				Vertices[idx] = Transform.transformPoint(Vertices[idx]);
			}

			foreach (VertexGroup vtxGroup in VertexGroups) {
				for (int idx = 0; idx < vtxGroup.Vertices.Count; idx++) {
					Vertex vtx = vtxGroup.Vertices[idx];
					vtx.Point = Transform.transformPoint(vtx.Point);
					vtxGroup.Vertices[idx] = vtx;
				}
			}

			HasDoneTransformation = true;
		}

		/// <summary>
		/// Frees all information used by this <see cref="Model3D"/>.
		/// </summary>
		public void Dispose() {
			Vertices.Clear();
			Normals.Clear();
			UVs.Clear();
			Indices.Clear();
			VertexGroups.Clear();
			BoneNames = new string[0];
			BoneIndices = new int[0, 0];
			BoneWeights = new float[0, 0];
			Source = null;
			Name = null;
			Transform = null;
		}

		/// <summary>
		/// Iterates through <see cref="VertexGroups"/> and returns the first <see cref="VertexGroup"/> whose <see cref="VertexGroup.Name"/> is equal to <paramref name="name"/>, or <see langword="null"/> if one could not be found.
		/// </summary>
		/// <param name="name">The name to search for.</param>
		/// <returns></returns>
		public VertexGroup GetVertexGroupByName(string name) => VertexGroups.Where(vtxGroup => vtxGroup.Name == name).FirstOrDefault();

		/// <summary>
		/// Constructs all <see cref="VertexGroup"/> instances automatically.
		/// </summary>
		public void ConstructGroups() {
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
		/// Exports the given <see cref="Model3D"/> instances into a single file.
		/// </summary>
		/// <param name="targetFile"></param>
		/// <param name="targetFormat"></param>
		/// <param name="models"></param>
		public static void ExportIntoOne(FileInfo targetFile, ModelFormat targetFormat = ModelFormat.FBX, params Model3D[] models) {
			var factory = ExporterBindings[targetFormat];
			AbstractModelExporter exporter = factory.NewInstance();
			exporter.Export(models, targetFile);
		}
	}
}
