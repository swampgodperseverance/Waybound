using System.Collections.Generic;
using Waybound.Common.HeartStyles;
using Waybound.Content.Race;

namespace Waybound.Core;

public class CustomClassData {
    public static List<HeartStyle> Heart { get; private set; } = [];
    public static RaceInfo[] RaceInfo { get; private set; } = new RaceInfo[5];

    internal static void Load() {
        RaceInfo[0] = new Human();
        RaceInfo[1] = new Dwarf();
        RaceInfo[2] = new Lihzard();
        RaceInfo[3] = new Viking();
        RaceInfo[4] = new Desfo();
    }
};