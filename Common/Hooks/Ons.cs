using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.UI;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;

namespace Waybound.Common.Hooks;

internal static class Ons {
    internal static void Load() {
        On_Main.DrawHealthBar += FixNPCHPBar; // if(HP != MaxHP) { Draw(); };
        On_Main.MouseTextHackZoom_string_int_byte_string += EditTextPos; // fix hover NPC text
        On_Main.MouseText_string_string_int_byte_int_int_int_int_int_bool += On_Main_MouseText_string_string_int_byte_int_int_int_int_int_bool;
        On_Main.DrawInterface_36_Cursor += DrawBar; // Draw bar for acc ThunderSigil

        On_PlayerDrawLayers.DrawPlayer_28_ArmOverItem += On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem;

        //FancyClassicPlayerResourcesDisplaySet
        //Main

        //On_UICharacterCreation.MakeCategoriesBar += On_UICharacterCreation_MakeCategoriesBar;
        //UICharacterCreation.MakeBackAndCreatebuttons += On_UICharacterCreation_MakeBackAndCreatebuttons;
        //On_UICharacterCreation.Draw += On_UICharacterCreation_Draw; // Draw custom race;
        //UICharacterCreation
    }

    static void On_Main_MouseText_string_string_int_byte_int_int_int_int_int_bool(On_Main.orig_MouseText_string_string_int_byte_int_int_int_int_int_bool orig, Main self, string cursorText, string buffTooltip, int rare, byte diff, int hackedMouseX, int hackedMouseY, int hackedScreenWidth, int hackedScreenHeight, int pushWidthX, bool noOverride) {
        string newPosText = "[" + Waybound.ModName + "]: new text pos";
        bool flag = false;

        if (!Main.gameMenu) { flag = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>().BarAlpha > 0; };
        if (cursorText.StartsWith(newPosText) || flag) {
            orig(self, cursorText, buffTooltip, rare, diff, Main.mouseX + 4, Main.mouseY - 22, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
            return;
        }
        orig(self, cursorText, buffTooltip, rare, diff, hackedMouseX, hackedMouseY, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
    }

    private static void On_PlayerDrawLayers_DrawPlayer_28_ArmOverItem(On_PlayerDrawLayers.orig_DrawPlayer_28_ArmOverItem orig, ref PlayerDrawSet drawinfo) {
        orig(ref drawinfo);
        if (drawinfo.drawPlayer.GetModPlayer<BloodyNecklacePlayer>().equipped) {
            DrawData item = new DrawData(Resources.Textures.Test.Value, new Vector2((int)(drawinfo.Position.X - Main.screenPosition.X - (float)(drawinfo.drawPlayer.bodyFrame.Width / 2) + (float)(drawinfo.drawPlayer.width / 2)), (int)(drawinfo.Position.Y - Main.screenPosition.Y + (float)drawinfo.drawPlayer.height - (float)drawinfo.drawPlayer.bodyFrame.Height + 4f)) + drawinfo.drawPlayer.bodyPosition + new Vector2(drawinfo.drawPlayer.bodyFrame.Width / 2, drawinfo.drawPlayer.bodyFrame.Height / 2), drawinfo.drawPlayer.bodyFrame, drawinfo.colorArmorBody, drawinfo.drawPlayer.bodyRotation, drawinfo.bodyVect, 1f, drawinfo.playerEffect);
            item.shader = drawinfo.cNeck;
            drawinfo.DrawDataCache.Add(item);
        }
    }

    private static void On_UICharacterCreation_MakeCategoriesBar(On_UICharacterCreation.orig_MakeCategoriesBar orig, UICharacterCreation self, UIElement categoryContainer) {
        //orig(self, categoryContainer);
    }

    private static void On_UICharacterCreation_MakeBackAndCreatebuttons(On_UICharacterCreation.orig_MakeBackAndCreatebuttons orig, UICharacterCreation self, Terraria.UI.UIElement outerContainer) {

    }


    //private static void On_UICharacterCreation_Draw(On_UICharacterCreation.orig_Draw orig, UICharacterCreation self, SpriteBatch spriteBatch) {
    //    orig(self, spriteBatch);
    //    float x = FontAssets.MouseText.Value.MeasureString("Swamp Lox").X;
    //    Vector2 vector = new Vector2(Main.mouseX, Main.mouseY) + new Vector2(16f);
    //    if (vector.Y > (float)(Main.screenHeight - 30))
    //        vector.Y = Main.screenHeight - 30;

    //    if (vector.X > (float)Main.screenWidth - x)
    //        vector.X = Main.screenWidth - 460;

    //    Terraria.Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, "Swamp Lox", vector.X, vector.Y, new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor), Color.Black, Vector2.Zero);
    //}

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
        On_Main.DrawInterface_36_Cursor -= DrawBar;
    }
};