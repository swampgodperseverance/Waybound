using ReLogic.Content;
using System;
using System.Collections.Generic;

namespace Waybound.Resources;

public static class Textures {
    const string Patch = "Assets/Textures/";

    public static Asset<Texture2D>[] Extaras => _extaras;
    public static Asset<Texture2D>[] RaceElements => _raceElements;
    public static Dictionary<string, Asset<Texture2D>[]> HeartAsset => _heartAsset;
    public static Asset<Texture2D>[] Tiles => _tiles;

    public static Asset<Texture2D> MegicPixel => _megicPixel;
    public static Asset<Texture2D> Test = null;

    readonly static Asset<Texture2D>[] _extaras = new Asset<Texture2D>[8];
    readonly static Asset<Texture2D>[] _raceElements = new Asset<Texture2D>[10];
    readonly static Dictionary<string, Asset<Texture2D>[]> _heartAsset = [];
    readonly static Asset<Texture2D>[] _tiles = new Asset<Texture2D>[2];

    static Asset<Texture2D> _megicPixel = null;

    internal static void Load(Mod mod) {
        _megicPixel = LoadTextures(Patch + "MagicPixel2X2");

        _extaras[0] = LoadTextures(Patch + "Extras/ThunderSigil_Bar");
        _extaras[1] = LoadTextures(Patch + "Extras/ThunderSigil_BG");
        _extaras[2] = LoadTextures(Patch + "Extras/ThunderSigil_Charg");
        _extaras[3] = LoadTextures(Patch + "Extras/ThunderSigil_BG_Glow");
        _extaras[4] = LoadTextures(Patch + "Extras/ThunderSigil_Barrier_Sklet");
        _extaras[5] = LoadTextures(Patch + "Extras/ThunderSigil_Barrier_Sklet_Bg");

        _raceElements[0] = LoadTextures(Patch + "UIs/Race/BgFullIcon");
        _raceElements[1] = LoadTextures(Patch + "UIs/Race/BgFullIcon_Hover");
        _raceElements[2] = LoadTextures(Patch + "UIs/Race/BgIcon");
        _raceElements[3] = LoadTextures(Patch + "UIs/Race/BgIcon_Hover");
        _raceElements[4] = LoadTextures(Patch + "UIs/Race/Stat_HP");
        _raceElements[5] = LoadTextures(Patch + "UIs/Race/Stat_HP_Regen");
        _raceElements[6] = LoadTextures(Patch + "UIs/Race/Stat_MP");
        _raceElements[7] = LoadTextures(Patch + "UIs/Race/Stat_MP_Regen");
        _raceElements[8] = LoadTextures(Patch + "UIs/Race/Stat_DamageResist");
        _raceElements[9] = LoadTextures(Patch + "UIs/Race/Stat_MiningSpeed");

        _tiles[0] = LoadTextures(Patch + "NoticeBoardTile");
        Test = LoadTextures(Patch + "Acc/top");

        Asset<Texture2D> LoadTextures(string name) => mod.Assets.Request<Texture2D>(name, AssetRequestMode.AsyncLoad);
    }
    internal static void RegisterHeart(Mod mod, string name) {
        string patch = Patch + "UIs/Hearts/" + name;
        _heartAsset.Add(name, [LoadTextures(patch), LoadTextures(patch + "_Fancy"), LoadTextures(patch + "_Bar")]);
        Asset<Texture2D> LoadTextures(string name) => mod.Assets.Request<Texture2D>(name, AssetRequestMode.AsyncLoad);
    }
    internal static void Unload() {
        _megicPixel = null;

        Array.Clear(_tiles);
        Array.Clear(_extaras);
    }
};