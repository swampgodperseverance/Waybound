namespace Waybound;

public class Waybound : Mod {
    Waybound() => _instance = this;

    public static Waybound Instance => _instance;
    static Waybound _instance = null;

    public static string ModName => Instance?.DisplayName ?? "Waybound";

    public Vector2 CameraOffset = Vector2.Zero;

    public override void Load() => Loader.Load(this);
    public override void Unload() => Loader.Unload(ref _instance);

    public override void HandlePacket(System.IO.BinaryReader reader, int whoAmI) => Common.NetCodeUtil.MultiplayerSystem.HandlePacket(reader, whoAmI);
};