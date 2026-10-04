using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Banners.PreHM;

namespace Waybound.Content.Items.Placeable.Banners
{
	public class CrystalDepthsBannerI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Depths Banner");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<CrystalDepthsBanner>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}