using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Magic.Water;

[AutoloadEquip(EquipType.Legs)]
public class OceanLeggings : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Red;
        Item.value = Item.buyPrice(gold: 25);
        Item.defense = 18;
    }
    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Magic) += .17f;
        player.moveSpeed += .15f;
    }
};