using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Walls;

namespace Waybound.Content.Items.Placeable.Walls

{
    public class DesertHunterBlockWallI : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			// DisplayName.SetDefault("Desert Hunter Block Wall");
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
			Item.createWall = ModContent.WallType<DesertHunterBlockWall>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 2, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(4)
				.AddIngredient(ModContent.ItemType<DesertHunterBlockI>(), 1)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}