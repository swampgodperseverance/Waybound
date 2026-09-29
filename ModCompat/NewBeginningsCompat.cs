//using Microsoft.Xna.Framework.Graphics;
//using ReLogic.Content;
//using Terraria.ID;
//using static Synergia.Helpers.ItemHelper;

//namespace Synergia.Common.ModSystems.ModSupports {
//    public class NewBeginningsCompat : ModSupportsSystem {
//        public static Asset<Texture2D> GetIcon(string name) => Request<Texture2D>("Synergia/Assets/Textures/" + name);
//        public override Mod TargetMod() { ModLoader.TryGetMod("NewBeginnings", out Mod mod); return mod; }
//        public override void Load(Mod beginnings) {
//            beginnings.Call("Delay", () => {
//                AddNatureBeast();
//            });
//            void AddNatureBeast() {
//                object equip = EquipData(beginnings, head: ItemID.GarlandHat, acc: [GetRoAItem("Herbarium")]);
//                object misc = MiscData(beginnings, 60, 20);
//                object result = AddOrigin(beginnings, GetIcon("NatureBeast"), "SynergiaNatureBeast", "Mods.Synergia.Origins.NatureBeast", [(GetRoAItem("SapStream"), 1)], equip, misc);
//            }
//        }
//        static object EquipData(Mod beginnings, int head = 0, int body = 0, int legs = 0, int[] acc = null) {
//            acc ??= [];
//            return beginnings.Call("EquipData", head, body, legs, acc);
//        }
//        static object MiscData(Mod beginnings, int hp = 100, int mp = 20, int spawn = -1) => beginnings.Call("MiscData", hp, mp, spawn);
//        static object AddOrigin(Mod beginnings, Asset<Texture2D> icon, string key, string translationKey, (int, int)[] inventory, object equip, object misc) => beginnings.Call("ShortAddOrigin", icon, key, translationKey, inventory, equip, misc);
//    }
//}
