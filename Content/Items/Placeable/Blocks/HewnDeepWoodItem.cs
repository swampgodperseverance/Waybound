using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Waybound.Content.Items.Placeable.Furniture.DeepWoodFurniture;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Content.Items.Placeable.Blocks
{
	public class HewnDeepWoodItem : ModItem
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
			Item.createTile = TileType<HewnDeepWood>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 1, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient(ItemType<DeepTreeItem>(), 1)
				.AddTile(TileID.Sawmill)
				.Register();
			CreateRecipe(1)
				.AddIngredient(ItemType<HewnDeepWoodPlatformI>(), 2)
				.Register();
		}
	}
}