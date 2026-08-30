using Terraria.Localization;

namespace Waybound.Common.WUtils;

// Localization Util
public static class LocUtil {
    public const string ACC = "Accessories";
    public const string ARM = "Armors";
    public const string WEP = "Weapons";
    public const string CHATMSG = "ChatsMsg";

    public enum CategoryName {
        NPC, Quest
    }
    public static string Category(CategoryName name) {
        return name switch {
            CategoryName.NPC => "NPCs",
            CategoryName.Quest => "Quests",
            _ => "Quest",
        };
    }
    public static string LocKey(CategoryName nameType, string nameKey) {
        return nameType switch {
            CategoryName.NPC => Language.GetTextValue($"Mods.Waybound.NPCs.{nameKey}"),
            CategoryName.Quest => Language.GetTextValue($"Mods.Waybound.Quests.{nameKey}"),
            _ => "Quest",
        };
    }
    public static string LocQuestKey(string npcName, string nameKey) => Language.GetTextValue($"Mods.Waybound.Quests.{npcName}.{nameKey}");
    public static string LocUIKey(string nameCategory, string nameKey) => Language.GetTextValue($"Mods.Waybound.UI.{nameCategory}.{nameKey}");
    public static string LocNPCKey(string npcName, string locKey) => Language.GetTextValue($"Mods.Waybound.NPCs.{npcName}.{locKey}");
    public static string ItemTooltip(string category, string tooltipKey) => Language.GetTextValue($"Mods.Waybound.Tooltips.{category}.{tooltipKey}");
    public static string EventLocKey(string eventName) => Language.GetTextValue($"Mods.Waybound.Events.{eventName}");
    public static string SynergiaLocKey(string name) => Language.GetTextValue($"Mods.Waybound.{name}");
    public static string AddBaseTooltips(string name) => Language.GetTextValue($"Mods.Waybound.BaseLoc.{name}");
    public static string DamageClassName(string className) => $"Mods.Waybound.BaseLoc.DamageClass.{className}";
    public static string AddAttackSpeed(string damageTypeKey, int speed) {
        string damageType = Language.GetTextValue(damageTypeKey);
        string attackSpead = Language.GetTextValue("Mods.Waybound.BaseLoc.AttackSpead");
        return string.Format(attackSpead, damageType, speed);
    }
}