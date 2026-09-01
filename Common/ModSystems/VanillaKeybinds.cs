using Terraria.ModLoader;

namespace Waybound.Common.ModSystems
{
    public class VanillaKeybinds : ModSystem
    {
        public static ModKeybind DesfosBagActivation { get; private set; }
        public static ModKeybind ArmorSetBonusActivation { get; private set; }

        public override void Load()
        {
            DesfosBagActivation = KeybindLoader.RegisterKeybind(Mod, "DesfosBagActivate", "B");
            ArmorSetBonusActivation = KeybindLoader.RegisterKeybind(Mod, "ArmorSetBonusActivate", "K");
        }

        public override void Unload()
        {
            DesfosBagActivation = null;
            ArmorSetBonusActivation = null;
        }
    }
}