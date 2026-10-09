using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;

namespace Waybound.Common.Hooks;

internal class IL_MainHook {
    public static void Load() {
        IL_Main.HoverOverNPCs += HoverNPC; // Added Point if mouse in NPC
        IL_Main.DrawInterface_14_EntityHealthBars += DrawBar; // Active draw if hp == maxHp
        IL_Main.CraftItem += DisableCraft;
        IL_Main.DrawMap += IL_Main_DrawMap;
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
            Main.instance.MouseText("[" + Waybound.ModName + "]: new text pos" + text);
            modPlayer.npcIndex = num;
        });
    }
    static void DrawBar(ILContext il) {
        ILCursor c = new(il) { Index = 86 };
        c.RemoveRange(8);
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
    static void IL_Main_DrawMap(ILContext il) {
        ILCursor c = new(il);
        int GetScale() => 30 + Main.LocalPlayer.GetModPlayer<StaminaPlayer>().mapScaleY;
        c.GotoNext(i => i.MatchCallvirt("Terraria.GameContent.UI.Minimap.MinimapFrameManager", "DrawTo"));
        c.Index -= 9;
        c.RemoveRange(10);
        c.Emit(OpCodes.Ldloc, 56);
        c.Emit(OpCodes.Ldloc, 57);
        c.EmitDelegate((float num33, float num34) => Main.MinimapFrameManagerInstance.DrawTo(Main.spriteBatch, new Vector2(num33 + 10f, num34 + GetScale() + 10f)));
        c.GotoNext(i => i.MatchLdloc(73));
        c.Index -= 9;
        c.RemoveRange(24);
        c.Emit(OpCodes.Ldloc, 5);
        c.Emit(OpCodes.Ldloc, 6);
        c.Emit(OpCodes.Ldloc, 61);
        c.Emit(OpCodes.Ldloc, 62);
        c.Emit(OpCodes.Ldloc, 63);
        c.Emit(OpCodes.Ldloc, 64);
        c.Emit(OpCodes.Ldloc, 73);
        c.EmitDelegate((float num5, byte b, int k, int l, float num43, float num44, Rectangle value) => Main.spriteBatch.Draw(Main.instance.mapTarget[k, l], new Vector2(num43, num44 + (Main.mapStyle == 1 && !Main.mapFullscreen ? GetScale() : 0)), value, new Color(b, b, b, b), 0f, default, num5, SpriteEffects.None, 0f));
        c.GotoNext(i => i.MatchLdloc(95));
        c.Index -= 2;
        c.Emit(OpCodes.Ldloc, 13);
        c.EmitDelegate((float num12) => num12 + GetScale());
        c.Emit(OpCodes.Stloc, 13);
    }

    public static void Unload() {
        IL_Main.HoverOverNPCs -= HoverNPC; // Added Point if mouse in NPC
        IL_Main.DrawInterface_14_EntityHealthBars -= DrawBar; // Active draw if hp == maxHp
        IL_Main.CraftItem -= DisableCraft; //
    }
};