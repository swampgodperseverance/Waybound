using ReLogic.Content;
using Terraria;

namespace Waybound.Resources;

public static class Effects {
    public static Asset<Effect> OutLine { get; private set; } = null;
    public static Asset<Effect> IceShimer { get; private set; } = null;
    public static BasicEffect BasicEffect { get; private set; } = null;

    public static string FilePath(string name) => $"Assets/Effects/{name}";

    internal static void Load(AssetRepository asset) {
        OutLine = asset.Request<Effect>(FilePath("OutLine"), AssetRequestMode.AsyncLoad);
        IceShimer = asset.Request<Effect>(FilePath("IceShimer"), AssetRequestMode.AsyncLoad);
        Main.QueueMainThreadAction(() => BasicEffect = new BasicEffect(Main.instance.GraphicsDevice)
        {
            VertexColorEnabled = true,
            TextureEnabled = true,
            LightingEnabled = false,
            World = Matrix.Identity
        });
    }
    internal static void Unload() {
        OutLine = null;
        IceShimer = null;
        Main.QueueMainThreadAction(BasicEffect.Dispose);
        BasicEffect = null;
    }
};