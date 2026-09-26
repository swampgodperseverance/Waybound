using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Accessories;

namespace Waybound.Content.Items.Accessories.Shields
{
	[AutoloadEquip(EquipType.Shield)]
	public class ShieldOfDesertHunter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("Every 20 seconds the shields rise around the player and give 35 defence for 5 seconds");
			// DisplayName.SetDefault("Minotaur Shield");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = -12;
			Item.value = Item.sellPrice(0, 0, 1, 50);
			Item.expert = true;
		}
		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<shieldOfDesertHunterUpdate>().shieldOfDesertHunter = true;
		}
	}
    public class shieldOfDesertHunterUpdate : ModPlayer
    {
        public bool shieldOfDesertHunter;
        public int shieldOfDesertHunterTimer;

        public override void ResetEffects()
        {
            shieldOfDesertHunter = false;
        }

        public override void PreUpdate()
        {
            if (shieldOfDesertHunter == true)
            {
                shieldOfDesertHunterTimer++;

                if (shieldOfDesertHunterTimer > 900)
                {
                    Player.AddBuff(ModContent.BuffType<ShieldOfDesertHunterB>(), 300);
                    shieldOfDesertHunterTimer = -300;
                }
            }
            else
            {
                shieldOfDesertHunterTimer = 0;
            }
        }
    }
}