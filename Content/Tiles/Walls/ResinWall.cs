using Terraria;
using Waybound.Content.Dusts.DeepDusts;

namespace Waybound.Content.Tiles.Walls
{
	public class ResinWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			AddMapEntry(new Color(25, 36, 42));
			// DustType = ModContent.DustType<DeepTreeDust>();
		}
	}
}