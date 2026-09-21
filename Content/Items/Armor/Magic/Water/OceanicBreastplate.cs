using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Magic.Water;

[AutoloadEquip(EquipType.Body)]
public class OceanicBreastplate : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        Item.width = Item.height = 24;
        Item.rare = ItemRarityID.Red;
        Item.value = Item.buyPrice(gold: 30);
        Item.defense = 24;
    }
    public override void UpdateEquip(Player player) {
        player.GetDamage(DamageClass.Magic) += .9f;
        player.GetCritChance(DamageClass.Magic) += .9f;
    }
};