using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture
{
	public class DesertHunterToiletI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Hunter Toilet");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 16;
			Item.height = 34;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = 150;
			Item.createTile = ModContent.TileType<Tiles.Furniture.DesertHunterFurniture.DesertHunterToilet>();
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup("Waybound:DesertHunterBlockI", 6)
				.AddTile(ModContent.TileType<DesertHunterForge>())
				.Register();
		}
	}
}