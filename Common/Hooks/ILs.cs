using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.GloblaItems;
using Waybound.Common.ItemDropRules;
using Waybound.Common.WUtils;

namespace Waybound.Common.Hooks;

// I LOVE ILCode
internal static class ILs {
    readonly static Dictionary<string, IColorDropInfo> tooltips = [];

    internal static void Load() {
        IL_Main.HoverOverNPCs += HoverNPC; // Added Point if mouse in NPC
        IL_Main.DrawInterface_14_EntityHealthBars += DrawBar; // Active draw if hp == maxHp
        IL_Main.CraftItem += DisableCraft;

        IL_CombatTextHook.Load();

        IL_UIBestiaryInfoItemLine.SetBestiaryNotesOnItemCache += InitColorLootTooltips;
        IL_UIBestiaryInfoItemLine.DrawMouseOver += SetColorLootTooltips;

        IL_ResourceOverlayHook.Load();
        IL_UICharacterCreationHook.Load();
    }

    static void DisableCraft(ILContext il) {
        ILCursor c = new(il);
        c.Index += 25;
        c.RemoveRange(15);
        c.Emit(OpCodes.Ldloc, 0);
        c.EmitDelegate((Item item) => {
            if (Race.CheckRace(Main.LocalPlayer, Race.ID.Desfo)) {
                int stack = item.stack;
                item = new Item(2) { stack = stack };
            };
            if (Main.mouseItem.stack > 0) { ItemLoader.StackItems(Main.mouseItem, item, out _); }
            else { Main.mouseItem = item; };
        });
    }
    static void HoverNPC(ILContext il) {
        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((Rectangle rectangle) => {
            ThunderSigilPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>();
            if (!modPlayer.equipped) { return; };
            if (modPlayer.npcIndex != -1) {
                NPC npc = Main.npc[modPlayer.npcIndex];
                npc.position += npc.netOffset;
                Rectangle value = npc.type >= NPCID.WyvernHead && npc.type <= NPCID.WyvernTail ? new((int)((double)npc.position.X + (double)npc.width * 0.5 - 32.0), (int)((double)npc.position.Y + (double)npc.height * 0.5 - 32.0), 64, 64) : new((int)npc.Bottom.X - npc.frame.Width / 2, (int)npc.Bottom.Y - npc.frame.Height, npc.frame.Width, npc.frame.Height);
                NPCLoader.ModifyHoverBoundingBox(npc, ref value);
                if (!rectangle.Intersects(value)) {
                    modPlayer.visualOnly = true;
                    if (modPlayer.OutLineAlpha == 0) {
                        if (modPlayer.BarAlpha == 0) { modPlayer.npcIndex = -1; };
                        if (modPlayer.activeEffect) { modPlayer.UpdateAlpha(true); }
                        else { modPlayer.WorkTime -= 2; };
                        if (modPlayer.WorkTime == 0) { modPlayer.UpdateAlpha(true); };
                    };
                    if (modPlayer.activeEffect) { modPlayer.npcIndex = -1; };
                } else { 
                    modPlayer.UpdateAlpha(false);
                    modPlayer.visualOnly = false;
                };
            };
        });
        c.GotoNext(MoveType.After, i => i.MatchLdstr("/"));
        c.Index += 12;
        c.RemoveRange(4);
        c.Emit(OpCodes.Ldloc, 12);
        c.Emit(OpCodes.Ldloc, 13);
        c.EmitDelegate((string text, int num) => {
            ThunderSigilPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>();
            if (!modPlayer.equipped || modPlayer.Player.dead || Main.npc[num].friendly) {
                Main.instance.MouseTextHackZoom(text);
                return; 
            };
            Main.instance.MouseTextHackZoom("[" + Waybound.ModName + "]: new text pos" + text);
            modPlayer.npcIndex = num;
        });
    }
    static void DrawBar(ILContext il) {
        ILCursor c = new(il) { Index = 86 };
        c.RemoveRange(8);
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
        IL_Main.HoverOverNPCs -= HoverNPC;
        IL_Main.DrawInterface_14_EntityHealthBars -= DrawBar;
        IL_Main.CraftItem -= DisableCraft;

        IL_CombatTextHook.Unloadd();

        IL_UIBestiaryInfoItemLine.SetBestiaryNotesOnItemCache -= InitColorLootTooltips;
        IL_UIBestiaryInfoItemLine.DrawMouseOver -= SetColorLootTooltips;

        IL_ResourceOverlayHook.Unload();
        IL_UICharacterCreationHook.Unload();
    }
};