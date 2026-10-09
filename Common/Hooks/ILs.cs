using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Waybound.Common.GloblaItems;
using Waybound.Common.ItemDropRules;

namespace Waybound.Common.Hooks;

// I LOVE ILCode
internal static class ILs {
    readonly static Dictionary<string, IColorDropInfo> tooltips = [];

    internal static void Load() {
        IL_MainHook.Load();
        IL_CombatTextHook.Load();

        IL_UIBestiaryInfoItemLine.SetBestiaryNotesOnItemCache += InitColorLootTooltips;
        IL_UIBestiaryInfoItemLine.DrawMouseOver += SetColorLootTooltips;

        IL_ResourceOverlayHook.Load();
        IL_UICharacterCreationHook.Load();
    }

    static void InitColorLootTooltips(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchCallvirt<List<string>>(nameof(List<string>.Add)));
        c.Emit(OpCodes.Ldloc, 2);
        c.EmitDelegate((IItemDropRuleCondition condition) => {
            if (condition is IColorDropInfo color) { tooltips.Add(condition.GetConditionDescription(), color); };
        });
    }
    static void SetColorLootTooltips(ILContext il) {
        ILCursor c = new(il);
        c.Index += 3;
        c.EmitDelegate(() => {
            if (tooltips.Count <= 0) { return; }
            Main.HoverItem.GetGlobalItem<OthenItems>().lootTooltips = new(tooltips.Select(i => (i.Key, i.Value.DropColor)).ToDictionary());
        });
    }

    internal static void Unload() {
        IL_MainHook.Unload();

        IL_CombatTextHook.Unloadd();

        IL_UIBestiaryInfoItemLine.SetBestiaryNotesOnItemCache -= InitColorLootTooltips;
        IL_UIBestiaryInfoItemLine.DrawMouseOver -= SetColorLootTooltips;

        IL_ResourceOverlayHook.Unload();
        IL_UICharacterCreationHook.Unload();
    }
};