namespace Waybound.Content.Race;

public class Desfo : RaceInfo {
    protected override Stat SetStat => new(-40, 20);
    public override Color[] Colors => [Color.Gold, Color.Goldenrod];
}
