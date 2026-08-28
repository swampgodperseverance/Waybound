using Microsoft.Xna.Framework;
using Waybound.Common.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Armor.Melee.Coreburned;

[AutoloadEquip(EquipType.Legs)]
public class CoreburnedLeggings : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public override void SetDefaults()
    {
        int width = 26; int height = 20;
        Item.Size = new Vector2(width, height);
        Item.rare = ModContent.RarityType<CoreburnedRarity>();
        Item.defense = 17;
        Item.value = Item.sellPrice(0, 4, 8, 50);
    }

    public override void UpdateEquip(Player player)
    {
        player.moveSpeed += 0.20f;
        player.endurance += 0.05f;
    }
}