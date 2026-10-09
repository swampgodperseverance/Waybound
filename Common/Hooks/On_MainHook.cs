using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Waybound.Common.Biome.Water;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.WUtils;

namespace Waybound.Common.Hooks;

internal static class On_MainHook {
    internal static void Load() {
        On_Main.DrawHealthBar += FixNPCHPBar; // if(HP != MaxHP) { Draw(); };
        On_Main.MouseText_string_string_int_byte_int_int_int_int_int_bool += EditTextPos; // Edit pos
        On_Main.DrawInterface_36_Cursor += DrawBar; // Draw bar for acc ThunderSigil
        On_Main.DrawMenu += DrawMenu;
        On_Main.CalculateWaterStyle += On_Main_CalculateWaterStyle;
        On_Main.GetInfoAccIconPosition += On_Main_GetInfoAccIconPosition;
    }

    static void FixNPCHPBar(On_Main.orig_DrawHealthBar orig, Main self, float X, float Y, int Health, int MaxHealth, float alpha, float scale, bool noFlip) {
        if (Health != MaxHealth) { orig(self, X, Y, Health, MaxHealth, alpha, scale, noFlip); };
    }
    static void EditTextPos(On_Main.orig_MouseText_string_string_int_byte_int_int_int_int_int_bool orig, Main self, string cursorText, string buffTooltip, int rare, byte diff, int hackedMouseX, int hackedMouseY, int hackedScreenWidth, int hackedScreenHeight, int pushWidthX, bool noOverride) {
        string newPosText = "[" + Waybound.ModName + "]: new text pos";
        bool flag = false;

        if (!Main.gameMenu) { flag = Main.LocalPlayer.GetModPlayer<ThunderSigilPlayer>().BarAlpha > 0; };
        if (cursorText.StartsWith(newPosText) && flag) {
            orig(self, cursorText[newPosText.Length..], buffTooltip, rare, diff, Main.mouseX + 4, Main.mouseY - 22, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
            return;
        }
        orig(self, cursorText, buffTooltip, rare, diff, hackedMouseX, hackedMouseY, hackedScreenWidth, hackedScreenHeight, pushWidthX, noOverride);
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
    static void DrawMenu(On_Main.orig_DrawMenu orig, Main self, GameTime gameTime) {
        orig(self, gameTime);
        if (Main.menuMode == 0) {
            IL_UICharacterCreationHook.saveData = null;
        }
    }
    static int On_Main_CalculateWaterStyle(On_Main.orig_CalculateWaterStyle orig, bool ignoreFountains) {
        if (Main.LocalPlayer.GetModPlayer<IceShimerPlayer>().Alpha != 0) {
            return GetInstance<CryoFluid>().Slot;
        }
        else {
            return orig(ignoreFountains);
        }
    }
    static void On_Main_GetInfoAccIconPosition(On_Main.orig_GetInfoAccIconPosition orig, int drawnCount, int StartX, out int X, out int Y) {
        orig(drawnCount, StartX, out X, out Y);
        Y += 40 + Main.LocalPlayer.GetModPlayer<StaminaPlayer>().mapScaleY;
        //orig.Invoke(drawnCount, StartX, out X, out Y);
    }


    internal static void Unload() {
        On_Main.DrawHealthBar -= FixNPCHPBar;
        On_Main.MouseText_string_string_int_byte_int_int_int_int_int_bool -= EditTextPos;
        On_Main.DrawInterface_36_Cursor -= DrawBar; 
        On_Main.DrawMenu -= DrawMenu;
        On_Main.CalculateWaterStyle -= On_Main_CalculateWaterStyle;
    }
}