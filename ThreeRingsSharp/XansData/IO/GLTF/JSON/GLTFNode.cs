using com.threerings.math;
using java.nio.channels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeRingsSharp.Utility;
using ThreeRingsSharp.XansData.Structs;
using ThreeRingsSharp.XansData.Extensions;

namespace ThreeRingsSharp.XansData.IO.GLTF.JSON {
	public class GLTFNode : GLTFObject {

		/// <summary>
		/// The name of this object as it appears in the 3D editor.
		/// </summary>
		[JsonProperty("name")] public string Name = null;

		/// <summary>
		/// The index of the mesh reference that this node points to.
		/// </summary>
		[JsonProperty("mesh")] public int Mesh = -1;

		/// <summary>
		/// The index of the skin referenced by this node.
		/// </summary>
		[JsonProperty("skin")] public int Skin = -1;

		/// <summary>
		/// The position of this object expressed as a 3D point <c>[x, y, z]</c>
		/// </summary>
		[JsonProperty("translation")] public float[] Translation = new float[3] { 0, 0, 0 };

		/// <summary>
		/// The rotation of this object expressed as a Quaternion: <c>[x, y, z, w]</c>
		/// </summary>
		[JsonProperty("rotation")] public float[] Rotation = new float[4] { 0, 0, 0, 1 };

		/// <summary>
		/// The scale of this object expressed as a 3D point <c>[x, y, z]</c>
		/// </summary>
		[JsonProperty("scale")] public float[] Scale = new float[3] { 1, 1, 1 };

		/// <summary>
		/// A transformation matrix representing the position, size, and scale of this <see cref="GLTFNode"/>.
		/// </summary>
		[JsonProperty("matrix")] public float[] Matrix = new float[16] {
			1, 0, 0, 0,
			0, 1, 0, 0,
			0, 0, 1, 0,
			0, 0, 0, 1
		};

		/// <summary>
		/// The child nodes of this node.
		/// </summary>
		[JsonProperty("children")] public List<int> Children = new List<int>();

		/// <summary>
		/// Set this to true to export <see cref="Matrix"/>, and false to export <see cref="Translation"/>, <see cref="Rotation"/>, and <see cref="Scale"/>.
		/// </summary>
		[JsonIgnore] public bool ExportMatrix = false;

		/// <summary>
		/// Sets <see cref="Translation"/> to the given <see cref="Vector3"/>.
		/// </summary>
		/// <param name="translation">The <see cref="Vector3"/> to set <see cref="Translation"/> to.</param>
		public void SetPosition(Vector3 translation) {
			Translation = new float[3];
			Translation[0] = translation.X;
			Translation[1] = translation.Y;
			Translation[2] = translation.Z;
		}

		/// <summary>
		/// Sets <see cref="Rotation"/> to the given <see cref="Quaternion"/>.
		/// </summary>
		/// <param name="rotation">The <see cref="Quaternion"/> to set <see cref="Rotation"/> to.</param>
		public void SetRotation(Quaternion rotation) {
			Rotation = new float[4];
			Rotation[0] = rotation.x;
			Rotation[1] = rotation.y;
			Rotation[2] = rotation.z;
			Rotation[3] = rotation.w;
			
		}

		/// <summary>
		/// Sets <see cref="Scale"/> to the given <see cref="Vector3"/>.
		/// </summary>
		/// <param name="scale">The <see cref="Vector3"/> to set <see cref="Scale"/> to.</param>
		public void SetScale(Vector3 scale) {
			Scale = new float[3];
			Scale[0] = scale.X;
			Scale[1] = scale.Y;
			Scale[2] = scale.Z;
		}

		/// <summary>
		/// Sets <see cref="Scale"/> so that all three components are equal to the given <see cref="float"/>.
		/// </summary>
		/// <param name="scale">The <see cref="float"/> to set all components of <see cref="Scale"/> to.</param>
		public void SetScale(float scale) => SetScale(new Vector3(scale, scale, scale));

		/// <summary>
		/// Sets all applicable transformation-related properties from the given <see cref="Transform3D"/>.
		/// </summary>
		/// <param name="transform"></param>
		public void SetTransform(Transform3D transform) {
			(Vector3f Translation, Quaternion Rotation, Vector3f Scale) transformData = transform.GetAllTransforms();

			SetPosition(transformData.Translation);
			SetRotation(transformData.Rotation);
			SetScale(transformData.Scale);
		}

		#region Newtonsoft Field Write Conditions
		// These are referenced by newtonsoft during runtime.
		// Format: ShouldSerialize...
		// Replace ... with the name of the field.

		public bool ShouldSerializeChildren() => Children.Count > 0;
		public bool ShouldSerializeName() => Name != null;
		public bool ShouldSerializeMesh() => Mesh >= 0;
		public bool ShouldSerializeSkin() => Skin >= 0;
		public bool ShouldSerializeTranslation() => !ExportMatrix;
		public bool ShouldSerializeRotation() => !ExportMatrix;
		public bool ShouldSerializeScale() => !ExportMatrix;
		public bool ShouldSerializeMatrix() => ExportMatrix;
		#endregion

	}
}
