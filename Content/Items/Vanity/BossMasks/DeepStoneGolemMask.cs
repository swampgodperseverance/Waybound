using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace Waybound.Content.Items.Vanity.BossMasks
{
	[AutoloadEquip(EquipType.Head)]
	public class DeepStoneGolemMask : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("");
			// DisplayName.SetDefault("Kronos Mask");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 32;
			Item.height = 24;
			Item.value = Item.sellPrice(0, 0, 75, 0);
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
	}
}