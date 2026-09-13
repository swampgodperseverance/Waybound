using Terraria.ModLoader.IO;
using Waybound.Content.Race;

namespace Waybound.Common.GlobalPlayer;

public class RacePlayer : ModPlayer {
    public RaceInfo Race => race ?? new Human();
    internal RaceInfo race = null;

    public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
        int healthBonus = 0;
        int manaBonus = 0;

        if (race == null) {
            health = new StatModifier(1f, 1f, 0, 0);
            mana = new StatModifier(1f, 1f, 0, 0);
            return;
        };
        Stat stat = race.GetStat;
        healthBonus += stat.BonusHP;
        manaBonus += stat.BonusMP;

        health = new StatModifier(1f, 1f, healthBonus, 0);
        mana = new StatModifier(1f, 1f, manaBonus, 0);
    }
    public override void SaveData(TagCompound tag) {
        tag[$"{Waybound.ModName}: Active Race"] = WUtils.Race.GetID(race);
    }
    public override void LoadData(TagCompound tag) {
        race = WUtils.Race.GetRace(tag.GetAsInt($"{Waybound.ModName}: Active Race"));
    }
    public override void PostUpdate() {
    }
};