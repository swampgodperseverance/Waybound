using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture
{
	public class DesertHunterPlatformI : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			// DisplayName.SetDefault("Desert Hunter Platform");
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
			Item.createTile = ModContent.TileType<DesertHunterPlatform>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 2, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(2)
				.AddRecipeGroup("Waybound:DesertHunterBlockI", 1)
				.Register();
		}
	}
}