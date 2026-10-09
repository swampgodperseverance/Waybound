using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;
using static Terraria.GameContent.Animations.IL_Actions.Sprites;

namespace Waybound.Common.OverlayResources;

public class StaminResource : ModResourceOverlay {
    public override void PostDrawResourceDisplay(PlayerStatsSnapshot snapshot, IPlayerResourcesDisplaySet displaySet, bool drawingLife, Color textColor, bool drawText) {
        StaminaPlayer staminaPlayer = Main.LocalPlayer.GetModPlayer<StaminaPlayer>();
        SpriteBatch spriteBatch = Main.spriteBatch;
        Asset<Texture2D>[] asset = Resources.Textures.ThrowerUI;
        Vector2 pos = new(Main.screenWidth - 94, 82);
        int count = staminaPlayer.MaxStatminaCount / 10;
        int style = 0;
        int scaleX = 0;
        string activeStyleName = displaySet.NameKey;
        string staminaCount = staminaPlayer.StaminaCount + "/" + staminaPlayer.MaxStatminaCount;
        bool drawStaminaText = false;
        bool hover = false;

        if (activeStyleName == "HorizontalBars") {
            pos.Y -= 5;
            staminaPlayer.mapScaleY = -17;
            style = 2;
        }
        else if(activeStyleName == "HorizontalBarsWithText") {
            drawStaminaText = true;
            pos.Y -= 1;
            staminaPlayer.mapScaleY = -13;
            style = 2;
        }
        else if (activeStyleName == "HorizontalBarsWithFullText") {
            staminaPlayer.mapScaleY = -15;
            pos.Y -= 3;
            pos.X -= 0;
            drawStaminaText = true;
            style = 2;
        }
        else if (activeStyleName == "New") {
            if (staminaPlayer.Player.ConsumedLifeCrystals < 6) {
                pos.Y -= 23;
                staminaPlayer.mapScaleY = -29; // -29
            }
            else {
                pos.Y += 6;
                staminaPlayer.mapScaleY = -4;
            }
            style = 1;
        }
        else if (activeStyleName == "NewWithText") {
            if (staminaPlayer.Player.ConsumedLifeCrystals < 6) {
                pos.Y += 13;
                staminaPlayer.mapScaleY = 7; // -23
            }
            else { 
                pos.Y += 42;
                staminaPlayer.mapScaleY = 32;
            }
            drawStaminaText = true;
            style = 1;
        }
        else {
            if (staminaPlayer.Player.ConsumedLifeCrystals < 6) {
                pos.Y += 26;
                staminaPlayer.mapScaleY = 8; // -22
            }
            else { 
                pos.Y += 42;
                staminaPlayer.mapScaleY = 28;
            }
            drawStaminaText = true;
        }
        //Main.NewText((float)(Math.Sin(Main.GlobalTimeWrappedHourly * staminaPlayer.Intensity)));
        if (style == 2) {
            pos.Y += 1;
            pos.X += 12;
            for (int i = 0; i < count; i++) {
                if (UI.DrawAndHover(spriteBatch, asset[6].Value, pos.X(scaleX))) { hover = true; }
                DrawGlow(14, pos.X(scaleX));
                int stamina = Utils.Clamp(staminaPlayer.StaminaCount - i * 10, 0, 10);
                if (stamina > 0) {
                    int width = (int)(12 * (stamina / 10f));
                    if (UI.DrawAndHover(spriteBatch, asset[8].Value, pos.X(scaleX - 6 + (12 - width)).Y(-6), new(12 - width, 0, width, 12), origin: Vector2.Zero)) { hover = true; }
                }
                if (i == count - 1) { 
                    if (UI.DrawAndHover(spriteBatch, asset[5].Value, pos.X(scaleX - 9))) { hover = true; }
                    DrawGlow(13, pos.X(scaleX - 9));
                }
                else {
                }
                scaleX -= 12;
            }
            pos.X -= 12;
            if (UI.DrawAndHover(spriteBatch, asset[7].Value, pos.X(12 + 8).Y(4))) {
                UI.DrawMouseText(spriteBatch, Loc.GetUI("StaminResource.Name"), Color.DarkOrange, Color.SandyBrown, 68, true);
                hover = false;
            }
            DrawGlow(15, pos.X(12 + 8).Y(4));
        }
        else if (style == 1) {
            Vector2 basePos = pos;
            basePos.X -= 186;
            basePos.Y += 2;
            int stamina = staminaPlayer.StaminaCount;

            for (int i = 0; i < count; i++) {
                if (i == 0) { 
                    UI.DrawAndHover(spriteBatch, asset[1].Value, basePos.X(scaleX));
                    DrawGlow(10, basePos.X(scaleX));
                }
                else if (i == count - 1) { 
                    UI.DrawAndHover(spriteBatch, asset[3].Value, basePos.X(scaleX + 2).Y(2));
                    DrawGlow(12, basePos.X(scaleX + 2).Y(2));
                }
                else {
                    UI.DrawAndHover(spriteBatch, asset[2].Value, basePos.X(scaleX));
                    DrawGlow(11, basePos.X(scaleX));
                };
                if (UI.DrawAndHover(spriteBatch, asset[4].Value, basePos.X(scaleX + 1), scale: MathHelper.Clamp((stamina - i * 10) / 10f, 0f, 1f))) { hover = true; };
                scaleX += 26;
            }
        }
        else {
            Vector2 basePos = pos;
            basePos.X -= 193;
            basePos.Y += 3;
            for (int i = 1; i <= count; i++) {
                float scale = 1f;
                int brightness = 255;
                bool pulse = false;

                if (staminaPlayer.StaminaCount >= i * 10) { if (staminaPlayer.StaminaCount == i * 10) { pulse = true; } }
                else {
                    float fill = (staminaPlayer.StaminaCount - (i - 1) * 10) / (float)10;
                    fill = MathHelper.Clamp(fill, 0f, 1f);
                    brightness = Math.Max(30, (int)(30f + 225f * fill));
                    scale = Math.Max(0.75f, fill / 4f + 0.75f);
                    if (fill > 0f) { pulse = true; }
                }
                if (pulse) { scale += Main.cursorScale - 1f; }
                if (UI.DrawAndHover(spriteBatch, asset[0].Value, basePos.X(scaleX), color: new Color(brightness, brightness, brightness, (int)(brightness * 0.9)), scale: scale)) { hover = true; }
                DrawGlow(9, basePos.X(scaleX), scale);
                scaleX += 29;
            }
        }
        if (drawStaminaText) {
            string text = $"{Loc.GetUI("StaminResource.Name")}:";
            Vector2 vector = pos.X(-Math.Abs(scaleX - 58)).Y(-8);
            if (style == 0 || style == 1) { vector = pos.X(-62).Y(-40); };
            UI.DrawResourceText(spriteBatch, text, vector + new Vector2((0f - FontAssets.MouseText.Value.MeasureString(text + " " + staminaCount).X) * 0.5f, 0f), Color.DarkOrange, Color.SandyBrown, Vector2.Zero);
            UI.DrawResourceText(spriteBatch, staminaCount, vector + new Vector2(FontAssets.MouseText.Value.MeasureString(text + " " + staminaCount).X * 0.5f, 0f), Color.DarkOrange, Color.SandyBrown, new Vector2(FontAssets.MouseText.Value.MeasureString(staminaCount).X, 0f));
        }
        if (hover) { UI.DrawMouseText(spriteBatch, staminaCount, Color.DarkOrange, Color.SandyBrown, 60, true); }
        void DrawGlow(int index, Vector2 pos, float scale = 1f) {
            if (staminaPlayer.BarAlpha > 0) {
                Color color = WColors.OutLineColor(Color.DarkGreen, Color.LightGreen, staminaPlayer.BarGlowTime, staminaPlayer.BarAlpha);
                UI.DrawTexture(spriteBatch, asset[index].Value, pos, color: color, scale: scale); 
            };
        }
    }
};
