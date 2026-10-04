using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Furniture
{
	public class DeepBrickFireplaceI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Brick Fireplace");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepBrickFireplace>());
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
				.AddIngredient(ModContent.ItemType<DeepTreeItem>(), 4)
				.AddIngredient(ItemID.Torch, 2)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}