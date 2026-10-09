using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.Rarities;

namespace Waybound.Content.Items.Materials.Misc
{
	public class CrumblePenny : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert Wreckage");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 50;
		}

		public override void SetDefaults()
		{
			Item.width = 26;
			Item.height = 30;
			Item.maxStack = 999;
			Item.value = Item.sellPrice(0, 0, 2, 0);
			Item.rare = ModContent.RarityType<IceShimer>();
		}
	}
}