using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;
using Waybound.Content.Tiles.Furniture;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;
using Waybound.Content.Items.Placeable.Blocks;

namespace Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture
{
	public class DeepWoodChairI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Wood Chair");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepWoodChair>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DeepTreeItem>(), 4)
				.AddTile(ModContent.TileType<DeepStoneAltar>())
				.Register();
		}
	}
}