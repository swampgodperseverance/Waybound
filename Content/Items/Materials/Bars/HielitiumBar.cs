using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Items.Placeable.Ores;
using Waybound.Content.Tiles.Bars;

namespace Waybound.Content.Items.Materials.Bars
{
	public class HielitiumBar : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
			// DisplayName.SetDefault("Hielitium Bar");
			// Tooltip.SetDefault("Too cold as you need better furnace");
		}

		public override void SetDefaults()
		{
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.maxStack = 999;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<HielitiumBarTile>();
			Item.width = 12;
			Item.height = 12;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = 2;
			Item.placeStyle = 0;
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient<HielitiumOreItem>(4)
				.AddTile(77) //hellforge
				.Register();
		}
	}
}