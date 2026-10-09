using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Tiles.Furniture.ScorchedWoodFurniture;

namespace Waybound.Content.Items.Placeable.Furniture.ScorchedWoodFurniture
{
	public class ScorchedWoodChandelierI : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("ScorchedWood Chandelier");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.ScorchedWoodFurniture.ScorchedWoodChandelier>());
			Item.width = 32;
			Item.height = 32;
			Item.maxStack = 99;
			Item.rare = 0;
			Item.value = Item.sellPrice(0, 0, 10, 0);
		}


	}
}