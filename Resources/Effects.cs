using ReLogic.Content;
using Terraria;

namespace Waybound.Resources;

public static class Effects {
    public static Asset<Effect> OutLine { get; private set; }
    public static Asset<Effect> IceShimer { get; private set; }
    public static BasicEffect BasicEffect { get; private set; }

    public static string FilePath(string name) => $"Assets/Effects/{name}";

    internal static void Load(AssetRepository asset) {
        OutLine = asset.Request<Effect>(FilePath("OutLine"));
        IceShimer = asset.Request<Effect>(FilePath("IceShimer"));
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