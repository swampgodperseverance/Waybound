using Waybound.Common.Utils;

namespace Waybound.Content.Race;

public abstract class RaceInfo {
    public Stat GetStat => SetStat;
    string Name => GetType().Name;
    public string Info => Loc.Get("Races." + Name + ".Info");
    public string RaceName => Loc.Get("Races." + Name + ".Name");

    protected abstract Stat SetStat { get; }
}
