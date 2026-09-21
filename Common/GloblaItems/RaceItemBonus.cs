using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Waybound.Common.WUtils;
using Waybound.Content.Race;

namespace Waybound.Common.GloblaItems;

public class RaceItemBonus : GlobalItem {
    public override bool InstancePerEntity => true;

    public RaceItemInfo? info = null;

    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips) {
        Player player = Main.LocalPlayer;
        if (info == null) {
            if (player.armor[0].type == ItemID.None) { return; }
            else if (player.armor[0].GetGlobalItem<RaceItemBonus>().info == null) { return; }
            else {
                RaceItemInfo raceItem = player.armor[0].GetGlobalItem<RaceItemBonus>().info.Value;
                if (raceItem.Items.Contains(item.type)) {
                    int index = tooltips.FindIndex(i => i.Name == "SetBonus");
                    if (index != -1) { AddText(index, tooltips, raceItem); };
                };
            };
        } else {
            if (info.Value.IsSetBonus) {
                int index = tooltips.FindIndex(i => i.Name == "SetBonus");
                if (index != -1) { AddText(index, tooltips, info.Value); };
            } else if (info.Value.IsAcc) {
                int index = tooltips.FindIndex(i => i.Name == "Equipable");
                if (index != -1) { AddText(index + 1, tooltips, info.Value); };
            } else {
                int index = tooltips.FindIndex(i => i.Name == "Tooltip0");
                if (index != -1) { AddText(index + 1, tooltips, info.Value); }
                else {
                    RaceInfo race = Race.GetRace(info.Value.ID);
                    Color color = !Race.CheckRace(Main.LocalPlayer, info.Value.ID) ? Colors.RarityTrash : race.Colors[0];
                    tooltips.Add(new(Mod, $"{Waybound.ModName}:RaceName", string.Format(Loc.GetTips("GlobalItem.RaceItemBonus.RaceText"), race.RaceName)) { OverrideColor = color });
                    tooltips.Add(new(Mod, $"{Waybound.ModName}:RaceBonus", info.Value.Text) { OverrideColor = color });
                };
            };
        };
    }
    void AddText(int index, List<TooltipLine> tooltips, RaceItemInfo info) {
        RaceInfo race = Race.GetRace(info.ID);
        Color color = !Race.CheckRace(Main.LocalPlayer, info.ID) ? Colors.RarityTrash : race.Colors[0];
        tooltips.Insert(index, new(Mod, $"{Waybound.ModName}:RaceName", string.Format(Loc.GetTips("GlobalItem.RaceItemBonus.RaceText"), race.RaceName)) { OverrideColor = color });
        tooltips.Insert(index + 1, new(Mod, $"{Waybound.ModName}:RaceBonus", info.Text) { OverrideColor = color });
    }
};
public struct RaceItemInfo() {
    public static RaceItemInfo SetArmor(int raceID, string text, bool isSetBonus = true, params int[] items) => new() { Items = items, ID = raceID, Text = text, IsSetBonus = isSetBonus };
    public static RaceItemInfo SetAcc(int item, int raceID, string text) => new() { Item = item, ID = raceID, Text = text, IsAcc = true };
    public static RaceItemInfo SetWeapons(int item, int raceID, string text) => new() { Item = item, ID = raceID, Text = text };

    public int[] Items { get; private set; } = [];
    public int Item { get; private set; } = 0;
    public int ID { get; private set; } = -1;

    public string Text { get; private set; } = "";

    public bool IsSetBonus { get; private set; } = false;
    public bool IsAcc { get; private set; } = false;
};