using Waybound.Common.Rarities;
using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Generic.Viscula
{
	[AutoloadEquip(EquipType.Legs)]
	public class AorticBreeches : ModItem {
		public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 3, 90, 0);
			Item.rare = ModContent.RarityType<CoreburnedRarity>();
			Item.defense = 6;
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