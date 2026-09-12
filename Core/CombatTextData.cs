namespace Waybound.Core;

public class CombatTextData(string id) {
    public int multMaxTime = 0;

    public string ID { get; private set; } = id;

    public bool hasAlpha = true;

    internal bool used = false;
}