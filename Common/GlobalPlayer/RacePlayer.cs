using Terraria.ID;
using Terraria.ModLoader.IO;
using Waybound.Content.Items.Accessories.Misc;
using Waybound.Content.Race;

namespace Waybound.Common.GlobalPlayer;

public class RacePlayer : ModPlayer {
    public RaceInfo Race => race ?? new Human();
    internal RaceInfo race = null;

    public override void SaveData(TagCompound tag) => tag[$"{Waybound.ModName}: Active Race"] = WUtils.Race.GetID(race);
    public override void LoadData(TagCompound tag) => race = WUtils.Race.GetRace(tag.GetAsInt($"{Waybound.ModName}: Active Race"));
    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
        int healthBonus = 0;
        int manaBonus = 0;

        Stat stat = Race.GetStat;
        healthBonus += stat.BonusHP;
        manaBonus += stat.BonusMP;

        health = new StatModifier(1f, 1f, healthBonus, 0);
        mana = new StatModifier(1f, 1f, manaBonus, 0);
    }
    public void StartItem() {
        if (WUtils.Race.GetID(Race) == WUtils.Race.ID.Desfo) {
            for (int i = 0; i < Player.inventory.Length; i++) {
                if (Player.inventory[i].type == ItemID.None) {
                    Player.inventory[i] = new(ItemType<DesfosBag>());
                    break;
                }
            }
        }
    }
};