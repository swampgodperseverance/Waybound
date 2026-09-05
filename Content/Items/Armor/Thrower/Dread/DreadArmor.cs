
﻿using Waybound.Common.Rarities;
using Waybound.Common.Utils;
using Terraria;
using Terraria.ID;

﻿using Terraria;
using Waybound.Common.Rarities;

namespace Waybound.Content.Items.Armor.Thrower.Dread
{
	[AutoloadEquip(EquipType.Body)]
	public class DreadArmor : ModItem {
		public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 4, 20, 0);
			Item.rare = ModContent.RarityType<CoreburnedRarity>();
			Item.defense = 11;
		}
		public override void UpdateEquip(Player player) {
			player.GetDamage(DamageClass.Throwing) += 0.15f;
			player.moveSpeed += 0.15f;
		}
		//public override bool IsArmorSet(Item head, Item body, Item legs) {
		//	return body.type == ItemType<CorrodeBody>() && legs.type == ItemType<CorrodeLegs>() && head.type == Type;
		//}
		//public override void UpdateArmorSet(Player player) {

		//}
	}
}