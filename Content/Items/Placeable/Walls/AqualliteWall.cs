using Terraria;
using Terraria.ID;
using Waybound.Content.Tiles.Walls;

namespace Waybound.Content.Items.Placeable.Walls
{
    public class AqualliteWall : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.StoneWall);
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Blue;
            Item.createWall = ModContent.WallType<AqualliteWallTile>();
        }
    }
}
