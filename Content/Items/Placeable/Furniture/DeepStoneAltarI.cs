using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Blocks;
using Waybound.Content.Items.Placeable.Furniture;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Furniture
{
	public class DeepStoneAltarI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Altar");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<DeepStoneAltar>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 20, 0);
			Item.consumable = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<DeepCrystalShardItem>(), 10)
				.AddIngredient(ModContent.ItemType<DeepStoneItem>(), 20)
				.AddTile(TileID.DemonAltar)
				.Register();
		}
	}
}