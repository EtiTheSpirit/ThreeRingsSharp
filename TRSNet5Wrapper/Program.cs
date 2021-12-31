using OOOReader.Reader;
using System;
using System.Diagnostics;
using System.IO;
using ThreeRingsSharp.Utilities;
using ThreeRingsSharp.XansData;

namespace ThreeRingsSharp {
	class Program {
		private const string RSRC = @"E:\Steam Games\steamapps\common\Spiral Knights\rsrc\";

		static void Main(string[] args) {
			FileInfo target = new FileInfo(RSRC + @"character\npc\monster\gremlin\null\model.dat");
			ShadowClass grem = (ShadowClass)MasterDataExtractor.Open(target, null);
			ReadFileContext ctx = new ReadFileContext(target);
			ConfigHandlers.ModelConfigs.ArticulatedConfig.ReadData(ctx, grem);
			foreach (Model3D model in ctx.AllModels) {
				model.Export(new FileInfo(@"F:\Users\Xan\Desktop\3D\ROFL.glb"));
			}
		}
	}
}
