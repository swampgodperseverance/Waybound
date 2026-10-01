using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Furniture
{
	public class DeepBrickChimneyI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Brick Chimney");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepBrickChimney>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DeepBricksItem>(), 10)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}