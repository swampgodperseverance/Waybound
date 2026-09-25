using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.DesertHunterFurniture
{
	public class DesertHunterTableI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Hunter Table");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.DesertHunterFurniture.DesertHunterTable>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}
		public override void AddRecipes()
		{
			CreateRecipe()
				.AddRecipeGroup("Waybound:DesertHunterBlockI", 8)
				.AddTile(ModContent.TileType<DesertHunterForge>())
				.Register();
		}
	}
}