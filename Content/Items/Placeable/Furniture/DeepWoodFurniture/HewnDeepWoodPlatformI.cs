using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Furniture.DeepWoodFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture
{
	public class HewnDeepWoodPlatformI : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
		}

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 15;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.createTile = TileType<HewnDeepWoodPlatform>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 2, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(2)
				.AddIngredient(ItemType<HewnDeepWoodItem>(), 1)
				.Register();
		}
	}
}