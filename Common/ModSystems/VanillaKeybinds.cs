using Terraria.ModLoader;

namespace Waybound.Common.ModSystems
{
    public class VanillaKeybinds : ModSystem
    {
        public static ModKeybind ToggleAuraModeKeybind { get; private set; }
        public static ModKeybind ArmorSetBonusActivation { get; private set; }

        public override void Load()
        {
            ToggleAuraModeKeybind = KeybindLoader.RegisterKeybind(Mod, "ToggleAuraMode", "J");
            ArmorSetBonusActivation = KeybindLoader.RegisterKeybind(Mod, "ArmorSetBonusActivate", "K");
        }

        public override void Unload()
        {
            ToggleAuraModeKeybind = null;
            ArmorSetBonusActivation = null;
        }
    }
}