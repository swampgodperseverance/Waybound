using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;
using Waybound.Content.Tiles.Furniture;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;
using Waybound.Content.Items.Placeable.Blocks;

namespace Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture
{
	public class DeepWoodSofaI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Wood Sofa");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepWoodSofa>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DeepTreeItem>(), 5)
				.AddIngredient(ItemID.Silk, 2)
				.AddTile(ModContent.TileType<DeepStoneAltar>())
				.Register();
		}
	}
}