using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Content.Tiles.Walls
{
    public class HellmutWallTile : ModWall
    {
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustID.Tungsten;
            //ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<NaturePlatingWall>();
            AddMapEntry(new Color(26, 26, 55));
        }
    }
}
