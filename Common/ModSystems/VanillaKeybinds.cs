namespace Waybound.Common.ModSystems
{
    public class VanillaKeybinds : ModSystem
    {
        public static ModKeybind DesfosBagActivation { get; private set; }
        public static ModKeybind ArmorSetBonusActivation { get; private set; }
        public static ModKeybind AccBonusActivation { get; private set; } = null;

        public override void Load()
        {
            DesfosBagActivation = KeybindLoader.RegisterKeybind(Mod, "DesfosBagActivate", "B");
            ArmorSetBonusActivation = KeybindLoader.RegisterKeybind(Mod, "ArmorSetBonusActivate", "K");
            AccBonusActivation = KeybindLoader.RegisterKeybind(Mod, "ActiveAcc", Microsoft.Xna.Framework.Input.Keys.Q);
        }

        public override void Unload()
        {
            DesfosBagActivation = null;
            ArmorSetBonusActivation = null;
            AccBonusActivation = null;
        }
    }
}