using Terraria.Localization;

namespace Waybound.Common.Utils;

// Localization Util
public static class Loc {
    public const string LocPatch = "Mods.Waybound.";
    public static string Get(string name) => Language.GetTextValue(LocPatch + name);
    public static string GetTips(string name) => Language.GetTextValue(LocPatch + "Tooltips." + name);
    public static string GetNPCChat(string key) => Language.GetTextValue(LocPatch + "NPCsChat." + key);
    public static string GetChat(string key) => Language.GetTextValue(LocPatch + "ChatMsg." + key);
};