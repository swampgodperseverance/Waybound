namespace Waybound.Resources;

public class Audio {
    static readonly System.Collections.Generic.Dictionary<string, Terraria.Audio.SoundStyle> registerSounds = [];

    public static Terraria.Audio.SoundStyle Get(string name) => registerSounds.TryGetValue(name, out var value) == true ? value : throw new System.Exception("No item in dictionary");
    static void Set(string name, bool music = false) => registerSounds.TryAdd(name, new("Waybound/Assets/Sounds" + (music ? "Music" : "Misc") + "/" + name));

    internal static void Load() { }
    internal static void Unload() { }
};