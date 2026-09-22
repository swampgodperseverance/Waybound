namespace Waybound;

internal static class Loader {
    internal static void Load(Mod mod) {
        Resources.Textures.Load(mod);
        Resources.Effects.Load(mod);
        Resources.Audio.Load();
        Tables.Set.Load();
        Core.CustomClassData.Load();

        Common.Hooks.Ons.Load();
        Common.Hooks.ILs.Load();

        Common.TagHandlers.TagLoader.Load();
    }
    internal static void Unload() {
        Resources.Textures.Unload();
        Resources.Effects.Unload();
        Resources.Audio.Unload();
        Tables.Set.Unload();

        Common.Hooks.Ons.Unload();
        Common.Hooks.ILs.Unload();
    }
};