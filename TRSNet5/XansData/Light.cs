using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThreeRingsSharp.XansData {

	/// <summary>
	/// Represents a light.
	/// </summary>
	[Obsolete("Lights are not ready")]
	public class Light {

		public Color AmbientColor { get; set; }
		public Color DiffuseColor { get; set; }
		public Color SpecularColor { get; set; }

		public float Range { get; set; }

		public float Intensity { get; set; }

	}
}
