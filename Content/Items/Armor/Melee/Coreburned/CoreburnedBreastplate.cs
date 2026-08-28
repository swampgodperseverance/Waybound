using Microsoft.Xna.Framework;
using Waybound.Common.Rarities;
using Terraria;
using Terraria.ID;

namespace Waybound.Content.Items.Armor.Melee.Coreburned;

[AutoloadEquip(EquipType.Body)]
public class CoreburnedBreastplate : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override void SetDefaults()
    {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);
        Item.rare = ModContent.RarityType<CoreburnedRarity>();
        Item.defense = 19;
        Item.value = Item.sellPrice(0, 4, 8, 50);
    }
    public override void UpdateEquip(Player player)
    {
        player.GetCritChance(DamageClass.Melee) += 15;
        player.endurance += 0.10f;
    }
}