using Waybound.Content.Race;

namespace Waybound.Common.WUtils;

public class Race {
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
    public class ID {
        public const int Human = 0;
        public const int Dwarf = 1;
        public const int Lihzard = 2;
        public const int Viking = 3;
        public const int Desfo = 4;
    };
};