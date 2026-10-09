using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Relic;
using Waybound.Content.Tiles.Trophy;

namespace Waybound.Content.Items.Placeable.Bosses
{
    public class KorochunTrophyItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Themis Trophy");
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<KorochunTrophy>());
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.rare = ItemRarityID.Blue;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }
    }
}