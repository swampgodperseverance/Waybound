namespace Waybound.Content.Race;

public class Dwarf : RaceInfo {
    protected override Stat SetStat => new(20, 20);
    public override Color[] Colors => [new(88, 83, 76), new(57, 56, 52)];
}
