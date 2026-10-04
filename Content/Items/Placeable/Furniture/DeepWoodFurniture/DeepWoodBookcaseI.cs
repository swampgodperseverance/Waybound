using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Furniture;

using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture
{
	public class DeepWoodBookcaseI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Wood Bookcase");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepWoodBookcase>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DeepTreeItem>(), 20)
				.AddIngredient(ItemID.Book, 10)
				.AddTile(ModContent.TileType<DeepStoneAltar>())
				.Register();
		}
	}
}