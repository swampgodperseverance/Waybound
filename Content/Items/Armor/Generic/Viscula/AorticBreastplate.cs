using Terraria;
using Terraria.ID;
using Waybound.Common.Rarities;

namespace Waybound.Content.Items.Armor.Generic.Viscula
{
	[AutoloadEquip(EquipType.Body)]
	public class AorticBreastplate : ModItem {
		public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 4, 20, 0);
			Item.rare = ItemRarityID.Purple;
			Item.defense = 11;
		}
		public override void UpdateEquip(Player player) {

		}
		//public override bool IsArmorSet(Item head, Item body, Item legs) {
		//	return body.type == ItemType<CorrodeBody>() && legs.type == ItemType<CorrodeLegs>() && head.type == Type;
		//}
		//public override void UpdateArmorSet(Player player) {

		//}
	}
}