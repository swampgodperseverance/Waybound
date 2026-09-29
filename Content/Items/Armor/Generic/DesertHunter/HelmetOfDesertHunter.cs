using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Armor.Generic.DesertHunter
{
	[AutoloadEquip(EquipType.Head)]
	public class HelmetOfDesertHunter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("2% increased all damage");
			// DisplayName.SetDefault("Helmet Of Desert Hunter");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = 1;
			Item.defense = 2;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<BreastplateOfDesertHunter>() && legs.type == ItemType<LeggingsOfDesertHunter>();
		}

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = "Desert drone lights your way";

            player.GetModPlayer<ArmorOfDesertHunter>().armorOfDesertHunter = 1;
            player.buffImmune[BuffID.Darkness] = true;
        }

        public override void AddRecipes()
		{

		}
	}
	public class ArmorOfDesertHunter : ModPlayer
	{
		//desert hunter
		public int armorOfDesertHunter;
		public int armorOfDesertConqueror;

		public override void ResetEffects()
		{
			armorOfDesertHunter = 0;
			armorOfDesertConqueror = 0;

		}



		public override void PreUpdate()
		{
			if (armorOfDesertHunter == 1)
				Player.AddBuff(ModContent.BuffType<DesertHunterArmorDroneB>(), 2);
			else
				Player.ClearBuff(ModContent.BuffType<DesertHunterArmorDroneB>());

		}
	}
}