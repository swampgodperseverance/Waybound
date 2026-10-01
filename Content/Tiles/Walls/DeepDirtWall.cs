using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Waybound.Content.Tiles.Walls
{
	public class DeepDirtWall : ModWall
	{
		public override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = false;
			AddMapEntry(new Color(28, 41, 55));
		}
	}
}