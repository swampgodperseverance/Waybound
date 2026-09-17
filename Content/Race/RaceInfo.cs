using Waybound.Common.WUtils;

namespace Waybound.Content.Race;

public abstract class RaceInfo {
    public Stat GetStat => SetStat;
    string Name => GetType().Name;
    public string RaceName => Loc.Get("Races." + Name + ".Name");
    public string Info => Loc.Get("Races." + Name + ".Info");
    public string Pozitiv => Loc.Get("Races." + Name + ".Pozitiv");
    public string Negativ => Loc.Get("Races." + Name + ".Negativ");
    public string ShortNegativ => Loc.Get("Races." + Name + ".ShortNegativ");

    protected abstract Stat SetStat { get; }
    public virtual Color[] Colors { get; set; } = [Color.White, Color.Black];
}
