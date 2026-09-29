namespace Waybound.Tables;

public class Loot(int id, int stack = 1, int chance = 1, int index = -1) : ILootTable {
    public int ItemID { get; private set; } = id;
    public int Stack { get; private set; } = stack;
    public int Сhance { get; private set; } = chance;
    public int SlotInChes { get; private set; } = index;
    internal bool ValidIndex => SlotInChes >= 0 && SlotInChes < 40;
    public Loot[] GetLoot() => [this];
}