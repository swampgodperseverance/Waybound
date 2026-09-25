using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Materials.Misc;
using Waybound.Content.Tiles.Blocks;
using Waybound.Content.Items.Placeable.Walls;
using Waybound.Content.Tiles.Furniture.DesertHunterFurniture;

namespace Waybound.Content.Items.Placeable.Blocks
{
	public class DesertHunterTileI : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			// DisplayName.SetDefault("Desert Hunter Tile");
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
			Item.createTile = ModContent.TileType<DesertHunterTile>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 1, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(10)
				.AddIngredient(ModContent.ItemType<DesertWreckage>(), 1)
				.AddTile(ModContent.TileType<Tiles.Furniture.DesertHunterFurniture.DesertHunterForge>())
				.Register();
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<DesertHunterTileWallI>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}