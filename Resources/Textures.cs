using ReLogic.Content;
using System;

namespace Waybound.Resources;

public static class Textures {
    const string Patch = "Assets/Textures/";

    public static Asset<Texture2D>[] Extaras => _extaras;
    public static Asset<Texture2D>[] Tiles => _tiles;

    public static Asset<Texture2D> MegicPixel => _megicPixel;

    readonly static Asset<Texture2D>[] _extaras = new Asset<Texture2D>[5];
    readonly static Asset<Texture2D>[] _tiles = new Asset<Texture2D>[2];

    static Asset<Texture2D> _megicPixel = null;

    internal static void Load(Mod mod) {
        _megicPixel = LoadTextures(Patch + "MagicPixel2X2");

        _extaras[0] = LoadTextures(Patch + "Bolvanka");
        _extaras[1] = LoadTextures(Patch + "Extras/ThunderSigil_Bar");
        _extaras[2] = LoadTextures(Patch + "Extras/ThunderSigil_BG");
        _extaras[3] = LoadTextures(Patch + "Extras/ThunderSigil_Charg");
        _extaras[4] = LoadTextures(Patch + "Extras/ThunderSigil_BG_Glow");

        _tiles[0] = LoadTextures(Patch + "NoticeBoardTile");

        Asset<Texture2D> LoadTextures(string name) => mod.Assets.Request<Texture2D>(name, AssetRequestMode.ImmediateLoad);
    }
    internal static void Unload() {
        _megicPixel = null;

        Array.Clear(_tiles);
        Array.Clear(_extaras);
    }
};