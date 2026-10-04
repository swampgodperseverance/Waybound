using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture;

namespace Waybound.Content.Items.Placeable.Furniture
{
	public class DeepCrystalShardItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 99;
			// DisplayName.SetDefault("Deep Crystal Shard");
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
			Item.createTile = ModContent.TileType<DeepCrystalShard>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 2, 50);
			Item.rare = 0;
		}

		public override void AddRecipes()
		{
			CreateRecipe(4)
				.AddIngredient<DeepCrystalItem>(1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}