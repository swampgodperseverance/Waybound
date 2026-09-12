using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;
using Waybound.Common.GlobalPlayer;
using Waybound.Core;

namespace Waybound.Common.Hooks;

internal static class IL_CombatTextHook {
    internal static void Load() {
        IL_CombatText.NewText_Rectangle_Color_string_bool_bool += DrawText;
        IL_CombatText.Update += FixAlpha;
    }

    static void DrawText(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(MoveType.Before, i => i.MatchLdcI4(60));
        c.Index += 2;
        c.Emit(OpCodes.Ldarg, 2); 
        c.Emit(OpCodes.Ldloc, 0);
        c.EmitDelegate((string text, int index) => {
            int time = 0;
            int t = Main.combatText[index].lifeTime;
            if (!Main.gameMenu) {
                List<CombatTextData> data = Main.LocalPlayer.GetModPlayer<GlobalPlayerData>().ActiveTextData;
                for (int i = 0; i < data.Count; i++) {
                    if (data[i].ID == text && !data[i].used) {
                        time = data[i].multMaxTime;
                        data[i].used = true;
                    };
                };
            };
            Main.combatText[index].lifeTime += time;
        });
    }
    static void FixAlpha(ILContext il) {
        ILCursor c = new(il);
        c.Index += 6;
        c.RemoveRange(28);
        c.Emit(OpCodes.Ldarg, 0);
        c.EmitDelegate((CombatText text) => {
            if (!Main.gameMenu) {
                List<CombatTextData> data = Main.LocalPlayer.GetModPlayer<GlobalPlayerData>().ActiveTextData;
                List<string> data2 = Main.LocalPlayer.GetModPlayer<GlobalPlayerData>().ActiveTextDataID;
                for (int i = 0; i < data.Count; i++) {
                    if (data[i].ID == text.text && data[i].used && data[i].hasAlpha) { UpdadeAlpha(); }
                    else if(data[i].ID != text.text) { continue; };
                };
                for (int i = 0; i < data2.Count; i++) {
                    if (text.text != data2[i]) { UpdadeAlpha(); };
                };
            } else { UpdadeAlpha(); };

            void UpdadeAlpha() {
                text.alpha += (float)text.alphaDir * 0.05f;
                if ((double)text.alpha <= 0.6) { text.alphaDir = 1; };
                if (text.alpha >= 1f) {
                    text.alpha = 1f;
                    text.alphaDir = -1;
                };
            }
        });
        c.GotoNext(i => i.MatchStfld(typeof(CombatText).GetField("active", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)));
        c.Emit(OpCodes.Ldarg, 0);
        c.EmitDelegate((CombatText text) => {});
    }

    internal static void Unloadd() {
        IL_CombatText.NewText_Rectangle_Color_string_bool_bool -= DrawText;
        IL_CombatText.Update -= FixAlpha;
    }
};