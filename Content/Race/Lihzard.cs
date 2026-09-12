namespace Waybound.Content.Race;

public class Lihzard : RaceInfo {
    protected override Stat SetStat => new(MPRegen: 2, DamageResist: -.3f);
};
