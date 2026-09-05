using Terraria;
using Terraria.ID;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Content.Items.Placeable.Blocks
{
    public class WickedStoneBlock : ModItem
    {
        // Token: 0x060013FC RID: 5116 RVA: 0x000A2BD8 File Offset: 0x000A0DD8
        public override void SetDefaults()
        {
            base.Item.width = 30;
            base.Item.height = 24;
            base.Item.maxStack = 9999;
            base.Item.value = Item.sellPrice(0, 0, 0, 0);
            base.Item.rare = 0;
            base.Item.createTile = ModContent.TileType<WickedStone>();
            base.Item.useTurn = true;
            base.Item.autoReuse = true;
            base.Item.useAnimation = 15;
            base.Item.useTime = 10;
            base.Item.useStyle = 1;
            base.Item.consumable = true;
        }

        // Token: 0x060013FD RID: 5117 RVA: 0x000A2C89 File Offset: 0x000A0E89
        public override void AddRecipes()
        {
           
        }
    }
}
