using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.ScorchedWoodFurniture
{
	public class ScorchedWoodWorkbenchI : ModItem
	{
		public override void SetStaticDefaults()
		{
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ScorchedWoodFurniture.ScorchedWoodWorkbench>(), 0);
			Item.width = 32;
			Item.height = 18;
			Item.maxStack = 99;
			Item.value = 150;
		}

	}
}