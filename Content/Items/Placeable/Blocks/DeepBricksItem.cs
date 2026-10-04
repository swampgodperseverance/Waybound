using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Content.Items.Placeable.Blocks
{
	public class DeepBricksItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			// DisplayName.SetDefault("Deep Brick");
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
			Item.createTile = ModContent.TileType<DeepBricks>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 2, 50);
			Item.rare = 0;
		}
		public override void AddRecipes()
		{
			CreateRecipe(4)
				.AddIngredient(ModContent.ItemType<DeepStoneItem>(), 3)
				.AddTile(TileID.Furnaces)
				.Register();
			CreateRecipe(1)
				.AddIngredient(ModContent.ItemType<Items.Placeable.Walls.DeepBricksWallItem>(), 4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}