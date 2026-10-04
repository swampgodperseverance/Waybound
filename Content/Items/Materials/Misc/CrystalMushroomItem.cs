using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Materials.Misc
{
	public class CrystalMushroomItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Crystal Mushroom");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 25;
		}

		public override void SetDefaults()
		{
			Item.width = 34;
			Item.height = 32;
			Item.maxStack = 99;
			Item.value = Item.sellPrice(0, 0, 8, 0);
			Item.rare = 2;
		}
	}
}