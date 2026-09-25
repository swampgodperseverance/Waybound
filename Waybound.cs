using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace Waybound;

public class Waybound : Mod
{
    public static Waybound Instance { get; private set; }

    public static string ModName => Instance?.DisplayName ?? "Waybound";

    public Vector2 CameraOffset;

    public override void Load()
    {
        Instance = this;
        Loader.Load(this);
    }

    public override void Unload()
    {
        Loader.Unload();
        Instance = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
        => Common.NetCodeUtil.MultiplayerSystem.HandlePacket(reader, whoAmI);
}