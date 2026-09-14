namespace Waybound.Content.Race;

public class Viking : RaceInfo {
    protected override Stat SetStat => new(40);
    public override Color[] Colors => [new(95, 205, 228), new(41, 109, 123)];
}
