namespace Waybound;

public class Waybound : Mod {
    public Waybound() => _instance = this;

    public static Mod Instance => _instance;
    static Mod _instance = null;

    public static string ModName => Instance == null ? "Waybound" : Instance.DisplayName;

    public override void Load() => Loader.Load(this);
    public override void HandlePacket(System.IO.BinaryReader reader, int whoAmI) => Common.NetCodeUtil.MultiplayerSystem.HandlePacket(reader, whoAmI);
    public override void Unload() => Loader.Unload();
};