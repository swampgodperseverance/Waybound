using Terraria.Audio;

namespace Waybound.Resources;

public class Audio {
    static readonly System.Collections.Generic.Dictionary<string, Terraria.Audio.SoundStyle> registerSounds = [];

    public static Terraria.Audio.SoundStyle Get(string name) => registerSounds.TryGetValue(name, out var value) == true ? value : throw new System.Exception("No item in dictionary");
    static void Set(string name, bool music = false) => registerSounds.Add(name, new("Waybound/Assets/Sounds/" + (music ? "Music" : "Effect") + "/" + name));

    internal static void Load() {
        Set("ThunderSigil_Restoration");
        Set("ThunderSigil_Destruction");
        Set("BloodyNecklace");

        SoundStyle heavyWeaponSwing = new SoundStyle("Waybound/Assets/Sounds/Effect/HeavyWeapon_Swing");
        heavyWeaponSwing.PitchRange = (-0.3f, -0.1f);
        registerSounds.Add("HeavyWeapon_Swing", heavyWeaponSwing);
        
        Set("BaseballBat_BallHit");
    }
    internal static void Unload() { }
};