using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;
using Waybound.Content.Race;

namespace Waybound.Common.Hooks;

internal static class Ons {
    internal static void Load() {
        On_MainHook.Load();

        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += FixNeck;

        On_UIPanel.DrawPanel += On_UIPanel_DrawPanel;

        On_UICharacterListItem.DrawSelf += DrawRaceName;
        On_UICharacterCreation.Click_NamingAndCreating += NeedRace; // For continue player need race
        On_UICharacterCreation.Click_GoBack += On_UICharacterCreation_Click_GoBack;
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
    static void On_UICharacterCreation_Click_GoBack(On_UICharacterCreation.orig_Click_GoBack orig, UICharacterCreation self, UIMouseEvent evt, UIElement listeningElement) {
        IL_UICharacterCreationHook.saveData.openRaceUI = false;
        orig(self, evt, listeningElement);
    }
    static void On_UIPanel_DrawPanel(On_UIPanel.orig_DrawPanel orig, UIPanel self, SpriteBatch spriteBatch, Texture2D texture, Color color) {
        if (IL_UICharacterCreationHook.saveData == null) {
            orig(self, spriteBatch, texture, color);
            return;
        }
        if (IL_UICharacterCreationHook.saveData.openRaceUI || IL_UICharacterCreationHook.saveData.raceConfirmUI != null) {
            if (self is UITextPanel<LocalizedText> button) {
                if (button.Text == Language.GetText("UI.Back").Value || button.Text == Language.GetText("UI.Create").Value || button.Text == Loc.GetUI("PlayerRaceMenu.Create")) {
                    CalculatedStyle dimensions = button.GetDimensions();
                    Point point = new((int)dimensions.X, (int)dimensions.Y);
                    UIs.Race r = (UIs.Race)IL_UICharacterCreationHook.saveData.element;
                    float alpha = r == null ? 1f : r.alpha;
                    UI.DrawTexture(spriteBatch, Resources.Textures.RaceElements[17].Value, point.ToVector2().X(82).Y(24), color: Color.White * alpha);
                    if (button.IsMouseHovering) { UI.DrawTexture(spriteBatch, Resources.Textures.RaceElements[18].Value, point.ToVector2().X(82).Y(24), color: Color.Gold * alpha); };
                    return;
                }
                else {
                    orig(self, spriteBatch, texture, color);
                    return;
                }
            }
            else { return; }
        }
        else { orig(self, spriteBatch, texture, color); }
    }
    static void DrawRaceName(On_UICharacterListItem.orig_DrawSelf orig, UICharacterListItem self, SpriteBatch spriteBatch) {
        orig(self, spriteBatch);
        Vector2 textPos = self.GetDimensions().Position().X(476).Y(4);
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
        orig(self, evt, listeningElement);
        //PlayerCreationData saveData = IL_UICharacterCreationHook.saveData;
        //if (saveData.race != null) {
        //    orig(self, evt, listeningElement);
        //    saveData.element?.Remove();
        //    saveData.element = null;
        //    saveData.raceConfirmUI?.Remove();
        //    saveData.raceConfirmUI = null;
        //    saveData.openRaceUI = false;
        //    if (saveData.parent != null) {
        //        saveData.parent.Append(saveData.topContainer);
        //        saveData.parent.Append(saveData.middleContainer);
        //    };
        //    return;
        //} else {
        //    if (saveData.openRaceUI) {
        //        SoundEngine.PlaySound(SoundID.MenuClose);
        //        saveData.element?.Remove();
        //        saveData.element = null;
        //        if (saveData.parent != null) {
        //            saveData.parent.Append(saveData.topContainer);
        //            saveData.parent.Append(saveData.middleContainer);
        //        };
        //        saveData.openRaceUI = false;
        //        return;
        //    };

        //    Type type = typeof(UICharacterCreation);
        //    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

        //    UIElement middleContainer = (UIElement)type.GetField("_middleContainer", flags).GetValue(self);
        //    UIElement topContainer = (UIElement)type.GetField("_topContainer", flags).GetValue(self);
        //    UIColoredImageButton clothingStyles = (UIColoredImageButton)type.GetField("_clothingStylesCategoryButton", flags).GetValue(self);
        //    UIColoredImageButton hairStyles = (UIColoredImageButton)type.GetField("_hairStylesCategoryButton", flags).GetValue(self);
        //    UIColoredImageButton charInfo = (UIColoredImageButton)type.GetField("_charInfoCategoryButton", flags).GetValue(self);

        //    if (saveData.raceConfirmUI == null) {
        //        SoundEngine.PlaySound(SoundID.MenuOpen);
        //        UIs.Race race = new(saveData, Loc.GetUI("PlayerRaceMenu.NeedRace")) {
        //            posScaleX = -347,
        //            posScaleY = -301
        //        };
        //        saveData.raceConfirmUI = race;
        //        saveData.raceConfirmUI.OnInitialize();
        //        listeningElement.Append(saveData.raceConfirmUI);
        //        if (saveData.middleContainer == null) {
        //            saveData.middleContainer = middleContainer;
        //            saveData.topContainer = topContainer;
        //            saveData.parent = middleContainer.Parent;
        //        };
        //        clothingStyles.SetSelected(false);
        //        hairStyles.SetSelected(false);
        //        charInfo.SetSelected(false);
        //        middleContainer.Remove();
        //        topContainer.Remove();
        //    } else {
        //        SoundEngine.PlaySound(SoundID.MenuClose);
        //        saveData.raceConfirmUI?.Remove();
        //        saveData.raceConfirmUI = null;
        //        if (saveData.parent != null) {
        //            saveData.parent.Append(saveData.topContainer);
        //            saveData.parent.Append(saveData.middleContainer);
        //        };
        //    };
        //};
    } 
    internal static void Unload() {
        On_MainHook.Unload();

        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem -= FixNeck;

        On_UIPanel.DrawPanel -= On_UIPanel_DrawPanel;

        On_UICharacterListItem.DrawSelf -= DrawRaceName;
        On_UICharacterCreation.Click_NamingAndCreating -= NeedRace; // For continue player need race
        On_UICharacterCreation.Click_GoBack -= On_UICharacterCreation_Click_GoBack;
    }
};