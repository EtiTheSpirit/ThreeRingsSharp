using OOOReader.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData.Extensions {
	public static class BinaryWriterExtensions {

		public static void Write(this BinaryWriter writer, Matrix4f matrix) {
			writer.Write(matrix.M00);
			writer.Write(matrix.M01);
			writer.Write(matrix.M02);
			writer.Write(matrix.M03);

			writer.Write(matrix.M10);
			writer.Write(matrix.M11);
			writer.Write(matrix.M12);
			writer.Write(matrix.M13);

			writer.Write(matrix.M20);
			writer.Write(matrix.M21);
			writer.Write(matrix.M22);
			writer.Write(matrix.M23);

			writer.Write(matrix.M30);
			writer.Write(matrix.M31);
			writer.Write(matrix.M32);
			writer.Write(matrix.M33);
		}

		public static void Write(this BinaryWriter writer, Transform3D transform) => Write(writer, transform.Matrix);

	}
}
