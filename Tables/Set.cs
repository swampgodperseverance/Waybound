namespace Waybound.Tables;

public class Set {
    public static LootTabel IceLoot { get; private set; } = null;

    public static void Load() {
        IceLoot = new(nameof(IceLoot));
        IceLoot.SetLoot(new Loot(1, index: -21));
        IceLoot.SetLoot(new RandomLoot(new Loot(1, index: 42)));
    }
    public static void Unload() {
        IceLoot = null;
    }
};