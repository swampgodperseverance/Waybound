namespace Waybound.Content.Race;

public class Lihzard : RaceInfo {
    protected override Stat SetStat => new(BonusMP: 40, MPRegen: 2, DamageResist: -.3f);
    public override Color[] Colors => [new(16, 145, 92), new(22, 106, 71)];
};
