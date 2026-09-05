using ReLogic.Content;

namespace Waybound.Resources;

public static class Effects {
    public static Asset<Effect> OutLine { get; private set; } = null;

    public static string FilePath(string name) => $"Assets/Effects/{name}";

    internal static void Load(Mod mod) {
        if (!Terraria.Main.dedServ) {
            AssetRepository asset = mod.Assets;
            OutLine = asset.Request<Effect>(FilePath("OutLine"), AssetRequestMode.ImmediateLoad);
        };
    }
    internal static void Unload() {
        OutLine = null;
    }
};