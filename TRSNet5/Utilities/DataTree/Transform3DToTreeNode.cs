using OOOReader.Utility.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDataTree.Data;
using XDataTree.TreeElements;

namespace ThreeRingsSharp.Utilities.DataTree {
	public static class Transform3DToTreeNode {

		public static KeyValueContainerElement ToKeyValueContainer(this Transform3D transform) {
			KeyValueContainerElement ctr = new KeyValueContainerElement("Transform", SilkImage.Matrix);
			KeyValueElement trs = new KeyValueElement("Translation", transform.Translation.ToString(), false, SilkImage.Vector3);
			KeyValueElement rot = new KeyValueElement("Rotation", transform.Rotation.ToString(), false, SilkImage.Vector4);
			KeyValueElement sca = new KeyValueElement("Scale", transform.Scale.ToString(), false, SilkImage.Vector3);
			ctr.Add(trs);
			ctr.Add(rot);
			ctr.Add(sca);
			return ctr;
		}

	}
}
