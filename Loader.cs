using System;
using Waybound.Common.WUtils;
using Waybound.Core;

namespace Waybound;

internal static class Loader {
    internal static void Load(Mod mod) {
        if (Terraria.Main.dedServ) { return; }
        DebugLoc.Load();
        Particles.ParticleSystem.Load();
        UI.WindowChatColor(ConsoleColor.Green, DebugLoc.GetLoc("Start"));
        UI.WindowChatColor(ConsoleColor.Cyan, DebugLoc.GetLoc("Asset")); 
        Resources.Textures.Load(mod);
        UI.WindowChatColor(ConsoleColor.Cyan, DebugLoc.GetLoc("Shader"));
        Resources.Effects.Load(mod.Assets);
        UI.WindowChatColor(ConsoleColor.Cyan, DebugLoc.GetLoc("Song"));
        Resources.Audio.Load();
        UI.WindowChatColor(ConsoleColor.DarkCyan, DebugLoc.GetLoc("Tables"));
        Tables.Set.Load();
        UI.WindowChatColor(ConsoleColor.Magenta, DebugLoc.GetLoc("Data"));
        CustomClassData.Load();
        UI.WindowChatColor(ConsoleColor.Magenta, DebugLoc.GetLoc("Partical"));
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Waybound.Instance.Logger.Info(DebugLoc.GetLoc("Hook"));
        Common.Hooks.Ons.Load();
        Common.Hooks.ILs.Load();
        Common.TagHandlers.TagLoader.Load();
        Console.ResetColor();
        UI.WindowChatColor(ConsoleColor.Green, DebugLoc.GetLoc("End"));
    }
    internal static void Unload(ref Waybound instance) {
        Resources.Textures.Unload();
        Resources.Effects.Unload();
        Resources.Audio.Unload();
        Tables.Set.Unload();
        Particles.ParticleSystem.Unload();

        Common.Hooks.Ons.Unload();
        Common.Hooks.ILs.Unload();
        instance = null;
    }
};