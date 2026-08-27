using Waybound.Content.Tiles.Trophy;
using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Placeable.Bosses
{
	public class CogwormTrophy : ModItem
	{
		public override void SetDefaults() {
			// Vanilla has many useful methods like these, use them! This substitutes setting Item.createTile and Item.placeStyle as well as setting a few values that are common across all placeable items
			Item.DefaultToPlaceableTile(ModContent.TileType<CogwormTrophyTile>());

			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.buyPrice(0, 1);
		}
	}
}