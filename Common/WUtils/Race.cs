using Waybound.Content.Race;

namespace Waybound.Common.WUtils;

internal class Race {
    public static int GetID(RaceInfo race) {
        return race switch {
            Human => 0,
            Dwarf => 1,
            Lihzard => 2,
            Viking => 3,
            Desfo => 4,
            _ => 0,
        };
    }
    public static RaceInfo GetRace(int ID) {
        return ID switch {
            0 => new Human(),
            1 => new Dwarf(),
            2 => new Lihzard(),
            3 => new Viking(),
            4 => new Desfo(),
            _ => new Human(),
        };
    }
};