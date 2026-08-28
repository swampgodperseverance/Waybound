using Microsoft.Xna.Framework;
using Waybound.Common.Rarities;
using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Summon.Guts;

[AutoloadEquip(EquipType.Body)]
public class Gutsplate : ModItem {
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults() {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);
        Item.rare = ModContent.RarityType<CoreburnedRarity>();
        Item.defense = 10;
        Item.value = Item.sellPrice(0, 6, 7, 90);
    }
    public override void UpdateEquip(Player player) {
        player.maxTurrets += 2;
        player.lifeRegen += 3;
    }
}