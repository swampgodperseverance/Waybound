namespace Waybound.Tables;

public class Set {
    public static LootTabel IceLoot { get; private set; } = null;

    public static void Load() {
        IceLoot = new();
    }
    public static void Unload() {
        IceLoot = null;
    }
};
