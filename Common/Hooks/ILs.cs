using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;
using Waybound.Core;

namespace Waybound.Common.Hooks;

// I LOVE ILCode
internal static class ILs {
    static PlayerCreationData saveData = null;

    internal static void Load() {
        IL_Main.HoverOverNPCs += HoverNPC; // Added Point if mouse in NPC
        IL_Main.DrawInterface_14_EntityHealthBars += DrawBar; // Active draw if hp == maxHp
        IL_Main.CraftItem += IL_Main_CraftItem;

        IL_CombatTextHook.Load();
        IL_ResourceOverlayHook.Load();

        IL_UICharacterCreation.BuildPage += IL_UICharacterCreation_BuildPage;
        IL_UICharacterCreation.MakeBackAndCreatebuttons += DrawButton; // Draw Race Button
        IL_UICharacterCreation.Draw += IL_UICharacterCreation_Draw;
    }
    static void IL_Main_CraftItem(ILContext il) {
        ILCursor c = new(il);
        c.Index += 25;
        c.RemoveRange(15);
        c.Emit(OpCodes.Ldloc, 0);
        c.EmitDelegate((Item item) => {
            bool flag = true;
            if (flag) {
                int stack = item.stack;
                item = new Item(2) { stack = stack };
            }
            if (Main.mouseItem.stack > 0) { ItemLoader.StackItems(Main.mouseItem, item, out _); }
            else { Main.mouseItem = item; }
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
                    if (modPlayer.activeEffect) {
                        //modPlayer.UpdateOutLineAlpha(true);
                        modPlayer.npcIndex = -1;
                        //if (modPlayer.BarAlpha == 0) { modPlayer.npcIndex = -1; };
                    };
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
    static void IL_UICharacterCreation_BuildPage(ILContext il) {
        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg, 0);
        c.EmitDelegate((UICharacterCreation self) => {
            Player player = (Player)typeof(UICharacterCreation).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(self);
            saveData = new(player);
        });
    }
    static void DrawButton(ILContext il) {
        ILCursor c = new(il);   
        c.GotoNext(i => i.MatchLdcR4(0.5f));
        c.Remove();
        c.Emit(OpCodes.Ldc_R4, 0.35f);
        c.GotoNext(i => i.MatchLdcR4(0.5f));
        c.Remove();
        c.Emit(OpCodes.Ldc_R4, 0.35f);
        c.Index += 45;
        c.Emit(OpCodes.Ldarg, 0);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((UICharacterCreation self, UIElement outerContainer) => {
            UITextPanel<LocalizedText> raceButton = new(Language.GetText(Loc.GetUI("PlayerRaceMenu.Create")), 0.7f, large: true) {
                Width = StyleDimension.FromPixelsAndPercent(-10f, 0.35f),
                Height = StyleDimension.FromPixels(50f),
                VAlign = 1f,
                HAlign = 0.5f,
                Top = StyleDimension.FromPixels(-45f)
            };

            raceButton.OnMouseOver += (evt, listeningElement) => {
                SoundEngine.PlaySound(SoundID.MenuTick);
                ((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
                ((UIPanel)evt.Target).BorderColor = Colors.FancyUIFatButtonMouseOver;
            };
            raceButton.OnMouseOut += (evt, listeningElement) => {
                ((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.8f;
                ((UIPanel)evt.Target).BorderColor = Color.Black;
            };

            raceButton.OnLeftClick += (evt, listeningElement) => {
                saveData.openRaceUI = !saveData.openRaceUI;

                Type type = typeof(UICharacterCreation);
                BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

                UIElement middleContainer = (UIElement)type.GetField("_middleContainer", flags).GetValue(self);
                UIElement topContainer = (UIElement)type.GetField("_topContainer", flags).GetValue(self);
                UIColoredImageButton clothingStyles = (UIColoredImageButton)type.GetField("_clothingStylesCategoryButton", flags).GetValue(self);
                UIColoredImageButton hairStyles = (UIColoredImageButton)type.GetField("_hairStylesCategoryButton", flags).GetValue(self);
                UIColoredImageButton charInfo = (UIColoredImageButton)type.GetField("_charInfoCategoryButton", flags).GetValue(self);

                if (saveData.openRaceUI) {
                    saveData.element = new UIs.Race();
                    outerContainer.Append(saveData.element);
                    if (saveData.middleContainer == null) {
                        saveData.middleContainer = middleContainer;
                        saveData.topContainer = topContainer;
                        saveData.parent = middleContainer.Parent;
                    };
                    clothingStyles.SetSelected(false);
                    hairStyles.SetSelected(false);
                    charInfo.SetSelected(false);
                    middleContainer.Remove();
                    topContainer.Remove();
                } 
                else {
                    saveData.element?.Remove();
                    saveData.element = null;
                    if (saveData.parent != null) {
                        saveData.parent.Append(saveData.topContainer);
                        saveData.parent.Append(saveData.middleContainer);
                    };
                };
            };

            raceButton.SetSnapPoint("Race", 0);
            outerContainer.Append(raceButton);
        });
    }
    static void IL_UICharacterCreation_Draw(ILContext il) {
        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((SpriteBatch sB) => {
            if (saveData.openRaceUI) {

            }
        });
    }

    internal static void Unload() {
        IL_Main.HoverOverNPCs -= HoverNPC;
        IL_Main.DrawInterface_14_EntityHealthBars -= DrawBar;

        IL_CombatTextHook.Unloadd();
        IL_ResourceOverlayHook.Unload();

        IL_UICharacterCreation.BuildPage -= IL_UICharacterCreation_BuildPage;
        IL_UICharacterCreation.MakeBackAndCreatebuttons -= DrawButton; // Draw Race Button
        IL_UICharacterCreation.Draw -= IL_UICharacterCreation_Draw;
    }
};