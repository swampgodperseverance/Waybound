namespace Waybound;

internal static class Loader {
    internal static void Load(Mod mod) {
        Resources.Textures.Load(mod);
        Resources.Effects.Load(mod);
        Resources.Audio.Load();

        Common.Hooks.Ons.Load(mod);
        Common.Hooks.ILs.Load(mod);

        Terraria.UI.Chat.ChatManager.Register<Common.TagHandlers.Bar>("TBPreview"); // TagHendler
    }
    internal static void Unload() {
        Resources.Textures.Unload();
        Resources.Effects.Unload();
        Resources.Audio.Unload();

        Common.Hooks.Ons.Unload();
        Common.Hooks.ILs.Unload();
    }
};