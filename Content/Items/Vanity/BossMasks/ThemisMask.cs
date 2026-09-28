using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Vanity.BossMasks
{
	[AutoloadEquip(EquipType.Head)]
	public class ThemisMask : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("");
			// DisplayName.SetDefault("Themis Mask");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 22;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 0, 75, 0);
			Item.rare = 1;
			Item.vanity = true;
		}
	}
}