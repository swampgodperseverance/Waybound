using System.Reflection;
using Terraria;

namespace Waybound.ModCompat; 
public class DragonLens {
    public static bool GetGoodMode(Player player) {
        if (ModLoader.TryGetMod("DragonLens", out Mod value)) {
            FieldInfo goodMode = value.GetType().Assembly.GetType("DragonLens.Content.Tools.Gameplay.Godmode").GetField("godMode", BindingFlags.Public | BindingFlags.Static);
            MethodInfo toolInfo = value.GetType().Assembly.GetType("DragonLens.Core.Systems.PermissionHandler").GetMethod("CanUseTools", BindingFlags.Public | BindingFlags.Static, [typeof(Player)]);
            return (bool)goodMode.GetValue(null) && (bool)toolInfo.Invoke(null, [player]);
        } else { return false; }
    }
}