using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Materials.Misc
{
	public class DeepGolemCore : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Deep Golem Core");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 50;
		}

		public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 28;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(0, 0, 2, 0);
			Item.rare = 1;
		}
	}
}