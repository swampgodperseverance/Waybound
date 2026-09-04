using ReLogic.Content;
using Terraria;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.Utils;

namespace Waybound.Common.Hooks;

internal static class Ons {
    internal static void Load(Mod mod) {
        On_Main.DrawHealthBar += FixNPCHPBar; // if(HP != MaxHP) { Draw(); };
        On_Main.MouseTextHackZoom_string_int_byte_string += EditTextPos; // fix hover NPC text
        On_Main.DrawInterface_36_Cursor += DrawBar; // Draw bar for acc ThunderSigil
        //On_UICharacterCreation.Draw += On_UICharacterCreation_Draw; // Draw custom race;
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
        if (text.Contains(newPosText)) { self.MouseText(text[newPosText.Length..], buffTooltip, itemRarity, diff, Main.mouseX + 4, Main.mouseY - 22); } 
        else { orig(self, text, itemRarity, diff, buffTooltip); };
    }
    static void DrawBar(On_Main.orig_DrawInterface_36_Cursor orig) {
        orig();
        ThunderSigilPlayer modPlayer = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>();
        if (modPlayer.BarAlpha > 0) {
            ref SpriteBatch sB = ref Main.spriteBatch;  
            Vector2 mousePos = new(Main.mouseX + 17, Main.mouseY + 21);
            if (modPlayer.visualOnly) { mousePos += Main.rand.NextVector2Circular(2, 2); };
            Asset<Texture2D>[] asset = Resources.Textures.Extaras;
            int barProgress = (int)(asset[1].Value.Width * UI.GetProgress(modPlayer.WorkTime, modPlayer.NEEDTIME));
            UI.DrawTexture(sB, asset[2].Value, mousePos, null, Color.White * modPlayer.BarAlpha, origin: Vector2.Zero);
            UI.DrawTexture(sB, asset[1].Value, mousePos.X(4).Y(4), new Rectangle(0, 0, barProgress, asset[1].Value.Height), Color.White * modPlayer.BarAlpha, origin: Vector2.Zero);
            if (barProgress > 2 && barProgress <= 26) { UI.DrawTexture(sB, asset[3].Value, mousePos.X(4 + barProgress).Y(4), color: Color.White * modPlayer.BarAlpha, origin: Vector2.Zero); };
            if (modPlayer.OutLineAlpha > 0) {
                Effect effect = Resources.Effects.OutLine.Value;

                effect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly);
                effect.Parameters["alpha"].SetValue(modPlayer.OutLineAlpha);

                sB.End();
                sB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, effect, Main.UIScaleMatrix);
 
                UI.DrawTexture(sB, asset[4].Value, mousePos.X(-2).Y(-2), null, Color.White, origin: Vector2.Zero);

                sB.End();
                sB.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.SamplerStateForCursor, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.UIScaleMatrix);

                //Main.NewText(modPlayer.OutLineAlpha);
                //Lighting.AddLight(new(mousePos.X + asset[1].Value.Width / 2, mousePos.Y - asset[1].Value.Height / 2), new Vector3(1.0f, 0.75f, 0.2f));
            };
        };
    }
    internal static void Unload() {
        On_Main.DrawHealthBar -= FixNPCHPBar;
        On_Main.MouseTextHackZoom_string_int_byte_string -= EditTextPos;
        On_Main.DrawInterface_36_Cursor -= DrawBar;
    }
};