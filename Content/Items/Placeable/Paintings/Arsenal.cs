using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Paintings
{
    // Token: 0x02000CA4 RID: 3236
    public class Arsenal : ModItem
    {
        // Token: 0x060047C7 RID: 18375 RVA: 0x001E00B0 File Offset: 0x001DE2B0
        public override void SetDefaults()
        {
            base.Item.DefaultToPlaceableTile(ModContent.TileType<PaintTile4x3>(), 0);
            base.Item.width = 30;
            base.Item.height = 30;
            base.Item.value = Item.sellPrice(0, 0, 10, 0);
        }

        // Token: 0x060047C8 RID: 18376 RVA: 0x001E00FD File Offset: 0x001DE2FD
        public override void AddRecipes()
        {

        }
    }
}