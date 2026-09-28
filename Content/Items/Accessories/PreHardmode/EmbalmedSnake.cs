using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
	public class EmbalmedSnake : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("8% increased magic damage\nGives immunity to poison");
			// DisplayName.SetDefault("Embalmed Snake");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = 2;
			Item.value = Item.sellPrice(0, 0, 50, 0);
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetDamage(DamageClass.Magic) += 0.08f;
			player.buffImmune[BuffID.Poisoned] = true;
		}
	}
}