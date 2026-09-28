using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Relic;

namespace Waybound.Content.Items.Placeable.Bosses
{
	public class ThemisRelicI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Themis Relic");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<ThemisRelic>());
			Item.width = 30;
			Item.height = 40;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Master;
			Item.master = true;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}
	}
}