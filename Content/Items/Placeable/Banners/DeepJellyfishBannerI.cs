//using Terraria;
//using Terraria.GameContent.Creative;
//using Terraria.ModLoader;

//namespace Waybound.Content.Items.Placeable.Banners
//{
//	public class DeepJellyfishBannerI : ModItem
//	{
//		public override void SetStaticDefaults()
//		{
//			// DisplayName.SetDefault("Deep Jellyfish Banner");
//			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
//		}

//		public override void SetDefaults()
//		{
//			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Banners.DeepJellyfishBanner>());
//			Item.width = 32;
//			Item.height = 32;
//			Item.maxStack = 99;
//			Item.rare = 1;
//			Item.value = Item.sellPrice(0, 0, 10, 0);
//		}
//	}
//}