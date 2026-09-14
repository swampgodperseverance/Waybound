using ReLogic.Content;
using System;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;
using Waybound.Content.Race;
using Waybound.Core;
using Waybound.UIs;

namespace Waybound.Common.Hooks;

internal static class Ons {
    internal static void Load() {
        On_Main.DrawHealthBar += FixNPCHPBar; // if(HP != MaxHP) { Draw(); };
        On_Main.MouseTextHackZoom_string_int_byte_string += EditTextPos; // fix hover NPC text
        On_Main.MouseText_string_string_int_byte_int_int_int_int_int_bool += EditTextPos2;
        On_Main.DrawInterface_36_Cursor += DrawBar; // Draw bar for acc ThunderSigil

        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += FixNeck;
        On_UICharacterListItem.DrawSelf += DrawRaceName;
        On_UICharacterCreation.Click_NamingAndCreating += NeedRace; // For continue player need race
    }

    private static void DrawRaceName(On_UICharacterListItem.orig_DrawSelf orig, UICharacterListItem self, SpriteBatch spriteBatch) {
        orig(self, spriteBatch);
        Vector2 textPos = self.GetDimensions().Position().X(460).Y(4);
        Player player = self.Data.Player;
        RaceInfo race = player.GetModPlayer<RacePlayer>().Race;
        float pulse = (MathF.Sin(Main.GlobalTimeWrappedHourly * 3f) + 1f) / 2f;
        Color color = Color.Lerp(race.Colors[0], Color.White, pulse);

        DrawText(textPos, Loc.GetUI("PlayerRaceMenu.Race") + " ");
        DrawText(textPos.X(FontAssets.MouseText.Value.MeasureString(Loc.GetUI("PlayerRaceMenu.Race") + " ").X), race.RaceName, [color, race.Colors[1]], pulse);
        static Vector2 DrawText(Vector2 pos, string name, Color[] color = null, float alpha = 1) {
            color ??= [Color.White, Color.Black];
            return ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value, name, pos, color[0], color[1] * alpha, 0f, Vector2.Zero, Vector2.One);
        }
    }

    static void NeedRace(On_UICharacterCreation.orig_Click_NamingAndCreating orig, UICharacterCreation self, UIMouseEvent evt, UIElement listeningElement) {
        PlayerCreationData saveData = IL_UICharacterCreationHook.saveData;
        if (saveData.race != null) {
            orig(self, evt, listeningElement);
            saveData.element?.Remove();
            saveData.element = null;
            saveData.raceConfirmUI?.Remove();
            saveData.raceConfirmUI = null;
            saveData.openRaceUI = false;
            if (saveData.parent != null) {
                saveData.parent.Append(saveData.topContainer);
                saveData.parent.Append(saveData.middleContainer);
            };
            return;
        } else {
            if (saveData.openRaceUI) {
                SoundEngine.PlaySound(SoundID.MenuClose);
                saveData.element?.Remove();
                saveData.element = null;
                if (saveData.parent != null) {
                    saveData.parent.Append(saveData.topContainer);
                    saveData.parent.Append(saveData.middleContainer);
                };
                saveData.openRaceUI = false;
                return;
            };

            System.Type type = typeof(UICharacterCreation);
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            UIElement middleContainer = (UIElement)type.GetField("_middleContainer", flags).GetValue(self);
            UIElement topContainer = (UIElement)type.GetField("_topContainer", flags).GetValue(self);
            UIColoredImageButton clothingStyles = (UIColoredImageButton)type.GetField("_clothingStylesCategoryButton", flags).GetValue(self);
            UIColoredImageButton hairStyles = (UIColoredImageButton)type.GetField("_hairStylesCategoryButton", flags).GetValue(self);
            UIColoredImageButton charInfo = (UIColoredImageButton)type.GetField("_charInfoCategoryButton", flags).GetValue(self);

            if (saveData.raceConfirmUI == null) {
                SoundEngine.PlaySound(SoundID.MenuOpen);
                Race race = new(saveData) {
                    posScaleX = -347,
                    posScaleY = -301
                };
                saveData.raceConfirmUI = race;
                saveData.raceConfirmUI.OnInitialize();
                saveData.raceConfirmUI.Append(new UIText(Loc.GetUI("PlayerRaceMenu.NeerRace"), 0.55f, true) {
                    Left = StyleDimension.FromPixels(-240f),
                    Top = StyleDimension.FromPixels(-240f)
                });
                listeningElement.Append(saveData.raceConfirmUI);
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
            } else {
                SoundEngine.PlaySound(SoundID.MenuClose);
                saveData.raceConfirmUI?.Remove();
                saveData.raceConfirmUI = null;
                if (saveData.parent != null) {
                    saveData.parent.Append(saveData.topContainer);
                    saveData.parent.Append(saveData.middleContainer);
                };
            };
        };
    }
    static void EditTextPos2(On_Main.orig_MouseText_string_string_int_byte_int_int_int_int_int_bool orig, Main self, string cursorText, string buffTooltip, int rare, byte diff, int hackedMouseX, int hackedMouseY, int hackedScreenWidth, int hackedScreenHeight, int pushWidthX, bool noOverride) {
        string newPosText = "[" + Waybound.ModName + "]: new text pos";
        bool flag = false;

        if (!Main.gameMenu) { flag = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>().BarAlpha > 0; };
        if (cursorText.StartsWith(newPosText) || flag) {
            orig(self, cursorText, buffTooltip, rare, diff, Main.mouseX + 4, Main.mouseY - 22, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
            return;
        }
        orig(self, cursorText, buffTooltip, rare, diff, hackedMouseX, hackedMouseY, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
    }
    static void FixNeck(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItem orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);
        if (drawinfo.drawPlayer.GetModPlayer<BloodyNecklacePlayer>().equipped) {
            DrawData item = new(Resources.Textures.Test.Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame, drawinfo.colorArmorBody, drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect) {
                shader = drawinfo.cNeck
            };
            drawinfo.DrawDataCache.Add(item);
        }
    }
    static void FixNPCHPBar(On_Main.orig_DrawHealthBar orig, Main self, float X, float Y, int Health, int MaxHealth, float alpha, float scale, bool noFlip) {
        if (Health != MaxHealth) { orig(self, X, Y, Health, MaxHealth, alpha, scale, noFlip); };
    }
    static void EditTextPos(On_Main.orig_MouseTextHackZoom_string_int_byte_string orig, Main self, string text, int itemRarity, byte diff, string buffTooltip) {
        string newPosText = "[" + Waybound.ModName + "]: new text pos";
        if (text.StartsWith(newPosText)) {
            self.MouseText(text[newPosText.Length..], buffTooltip, itemRarity, diff, Main.mouseX + 4, Main.mouseY - 22);
            return;
        }

        orig(self, text, itemRarity, diff, buffTooltip);
    }
    static void DrawBar(On_Main.orig_DrawInterface_36_Cursor orig) {
        orig();
        if (Main.gameMenu) { return; }
        ThunderSigilPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>();
        if (modPlayer.BarAlpha > 0 && Main.mouseItem.type == ItemID.None && !Main.mouseText) {
            ref SpriteBatch sB = ref Main.spriteBatch;  
            Vector2 mousePos = new(Main.mouseX + 17, Main.mouseY + 21);
            if (modPlayer.visualOnly && !modPlayer.activeEffect) { mousePos += Main.rand.NextVector2Circular(2, 2); };
            Asset<Texture2D>[] asset = Resources.Textures.Extaras;
            int barProgress = (int)(asset[0].Value.Width * UI.GetProgress(modPlayer.WorkTime, modPlayer.NEEDTIME));
            UI.DrawTexture(sB, asset[1].Value, mousePos, null, Color.White * modPlayer.BarAlpha, origin: Vector2.Zero);
            UI.DrawTexture(sB, asset[0].Value, mousePos.X(4).Y(4), new Rectangle(0, 0, barProgress, asset[0].Value.Height), Color.White * modPlayer.BarAlpha, origin: Vector2.Zero);
            if (barProgress > 2 && barProgress <= 26) { UI.DrawTexture(sB, asset[2].Value, mousePos.X(4 + barProgress).Y(4), color: Color.White * modPlayer.BarAlpha, origin: Vector2.Zero); };
            if (modPlayer.OutLineAlpha > 0) {
                Effect effect = Resources.Effects.OutLine.Value;
                effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                effect.Parameters["alpha"].SetValue(modPlayer.OutLineAlpha);
                sB.End();
                sB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Main.UIScaleMatrix);
                UI.DrawTexture(sB, asset[3].Value, mousePos.X(-2).Y(-2), null, Color.White, origin: Vector2.Zero);
                sB.End();
                sB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);
            };
        };
    }
    internal static void Unload() {
        On_Main.DrawHealthBar -= FixNPCHPBar;
        On_Main.MouseTextHackZoom_string_int_byte_string -= EditTextPos;
        On_Main.MouseText_string_string_int_byte_int_int_int_int_int_bool -= EditTextPos2;
        On_Main.DrawInterface_36_Cursor -= DrawBar; 

        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem -= FixNeck;
        On_UICharacterListItem.DrawSelf -= DrawRaceName;
        On_UICharacterCreation.Click_NamingAndCreating -= NeedRace;
    }
};