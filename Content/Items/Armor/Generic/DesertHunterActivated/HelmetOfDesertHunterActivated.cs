using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.ModSystems;
using Waybound.Content.Items.Armor.Generic.DesertHunter;
using Waybound.Content.Items.Materials.Misc;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Armor.Generic.DesertHunterActivated
{
	[AutoloadEquip(EquipType.Head)]
	public class HelmetOfDesertHunterActivated : ModItem
	{
		public override void SetStaticDefaults()
		{
			// Tooltip.SetDefault("3% increased all damage");
			// DisplayName.SetDefault("Helmet of Desert Conqueror");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 22;
			Item.value = Item.sellPrice(0, 0, 30, 0);
			Item.rare = 2;
			Item.defense = 3;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.05f;
        }

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<BreastplateOfDesertHunterActivated>() && legs.type == ItemType<LeggingsOfDesertHunterActivated>();
		}

		public override void UpdateArmorSet(Player player)
		{
            player.setBonus = "Desert drone lights your way" +
                "\nSelect keybind for [Armor Key] in Controls";
            foreach (string key in VanillaKeybinds.ArmorSetBonusActivation.GetAssignedKeys())
            {
                player.setBonus = "Desert drone lights your way" +
                    "\nPress " + key + " to switch drone's mode to fight mode";
            }

			player.GetModPlayer<ArmorOfDesertHunter>().armorOfDesertHunter = 1;
			player.GetModPlayer<ArmorOfDesertHunter>().armorOfDesertConqueror = 1;
            player.buffImmune[BuffID.Darkness] = true;
            player.AddBuff(BuffID.Dangersense, 2);
        }

		public override void AddRecipes()
		{
			CreateRecipe(1)
				.AddIngredient<HelmetOfDesertHunter>(1)
				.AddIngredient<DesertCore>(1)
				.AddTile(TileID.Anvils)
				.Register();
		}
	}
}