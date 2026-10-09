using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Banners.PreHM;

namespace Waybound.Content.Items.Placeable.Banners
{
	public class CrystalHermitBannerI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Hermit Banner");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<CrystalHermitBanner>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 1;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
	}
}