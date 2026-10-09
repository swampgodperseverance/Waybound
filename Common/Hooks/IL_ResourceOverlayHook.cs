using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.ResourceSets;
using Waybound.Common.GlobalPlayer;
using Waybound.Common.HeartStyles;
using Waybound.Common.WUtils;

namespace Waybound.Common.Hooks;

internal class IL_ResourceOverlayHook {
    static readonly Dictionary<string, Asset<Texture2D>> _vanillaAssetCache = [];
    static readonly HashSet<Vector2> _posHeart = [];

    static string _text = null;

    internal static void Load() {
        IL_ClassicPlayerResourcesDisplaySet.DrawLife += DrawLifeInOldStyle; // Draw new heard style in style 1.3;
        IL_ClassicPlayerResourcesDisplaySet.DrawMana += DrawManaInOldStyle; // New color for mana text;
        IL_ClassicPlayerResourcesDisplaySet.TryToHover += AddCustomTextForBar; // Custom heart text if active style
        IL_ResourceDrawSettings.Draw += DrawLifeInNewStyle; // Draw new heard style in style 1.4..
        IL_HorizontalBarsPlayerResourcesDisplaySet.TryToHover += FixHover; // Fix hover bar text
        IL_HorizontalBarsPlayerResourcesDisplaySet.DrawLifeBarText += IL_HorizontalBarsPlayerResourcesDisplaySet_DrawLifeBarText;
        IL_HorizontalBarsPlayerResourcesDisplaySet.DrawManaText += FixManaText; // New pos and color
        IL_FancyClassicPlayerResourcesDisplaySet.DrawManaBar += AddManaText; // visibility and new color
        IL_FancyClassicPlayerResourcesDisplaySet.DrawLifeBarText += IL_FancyClassicPlayerResourcesDisplaySet_DrawLifeBarText;
        IL_CommonResourceBarMethods.DrawLifeMouseOver += IL_CommonResourceBarMethods_DrawLifeMouseOver;
        IL_CommonResourceBarMethods.DrawManaMouseOver += IL_CommonResourceBarMethods_DrawManaMouseOver;
    }

    static void DrawLifeInOldStyle(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(MoveType.After, i => i.MatchLdfld(typeof(Player).GetField("ghost")));
        c.GotoNext(MoveType.After, i => i.MatchLdfld(typeof(Player).GetField("ghost")));
        c.Index += 1;
        c.RemoveRange(81);
        c.Emit(OpCodes.Ldloc, 0); // localPlayer
        c.Emit(OpCodes.Ldloc, 7); // num4
        c.Emit(OpCodes.Ldloc, 9); // vector
        c.EmitDelegate((Player localPlayer, int num4, Vector2 pos) => {
            int num = Main.screenWidth - 800;
            Color color = Color.Brown; //Color.Tomato //Color.Sienna; //Color.OrangeRed //Color.Maroon; //Color.Firebrick // Color.Brown; // Color.Crimson
            Color color2 = Color.Crimson;
            UI.DrawResourceText(Main.spriteBatch, Lang.inter[0].Value, new Vector2((float)(500 + 13 * num4) - pos.X * 0.5f + (float)num, 6f), color2, color, Vector2.Zero);
            UI.DrawResourceText(Main.spriteBatch, localPlayer.statLife + "/" + localPlayer.statLifeMax2, new Vector2((float)(500 + 13 * num4) + pos.X * 0.5f + (float)num, 6f), color2, color, new Vector2(FontAssets.MouseText.Value.MeasureString(localPlayer.statLife + "/" + localPlayer.statLifeMax2).X, 0f));
        });
        c.GotoNext(MoveType.After, i => i.MatchLdloca(21));
        if (!ModLoader.HasMod("ExxoAvalonOrigins")) {
            c.Index -= 1;
            c.RemoveRange(30);
        };
        c.Emit(OpCodes.Ldarg, 0); // this;
        c.Emit(OpCodes.Ldloc, 0); // player
        c.Emit(OpCodes.Ldloc, 1); // sb
        c.Emit(OpCodes.Ldloc, 3); // snapshot
        c.Emit(OpCodes.Ldloc, 11); // index
        c.Emit(OpCodes.Ldloc, 12); // num5
        c.Emit(OpCodes.Ldloc, 13); // num6
        c.Emit(OpCodes.Ldloc, 17); // a
        c.Emit(OpCodes.Ldloc, 19); // heartTexture
        c.Emit(OpCodes.Ldloc, 20); // position
        c.EmitDelegate((ClassicPlayerResourcesDisplaySet set, Player player, SpriteBatch sb, PlayerStatsSnapshot snapshot, int i, int num5, float num6, int a, Asset<Texture2D> heartTexture, Vector2 position) => {
            HeartStylesPlayer heartStylesPlayer = player.GetModPlayer<HeartStylesPlayer>();
            HeartStyle style = player.GetModPlayer<HeartStylesPlayer>().GetStyleForHeart(i - 1, set.NameKey);
            HeartStyle baseStyle = null;
            Color color = new(num5, num5, num5, a);
            Vector2 scale = new(num6);
            int elementIndex = i - 1;
            int scalaeY = heartStylesPlayer.CurrentHeart == elementIndex + 1 ? -3 : 0;
            int scalaeY2 = heartStylesPlayer.CurrentHeart != elementIndex + 1 ? -3 : 0;
            bool hover = false;

            if (style != null && !style.Reaplece) { baseStyle = heartStylesPlayer.GetStyleForHeart(elementIndex, set.NameKey, style); };
            if (style != null) {
                Asset<Texture2D>[] value = new Asset<Texture2D>[3];
                if (style.HasAsset) { Resources.Textures.HeartAsset.TryGetValue(style.Name, out value); };
                if (value[0] == null) { value[0] = GetVanillHeart(player, elementIndex)[0]; }
                if (style.Reaplece && !style.Flip) {
                    heartTexture = value[0];
                    _posHeart.Add(position);
                    hover = UI.Hover(position, new Rectangle(0, 0, heartTexture.Value.Width + 20, heartTexture.Value.Height + 20));
                } else if (!style.Flip) {
                    style.Draw(sb, ref heartTexture, position.Y(scalaeY), ref color, num6);
                    _posHeart.Add(position);
                    hover = UI.Hover(position, new Rectangle(0, 0, heartTexture.Value.Width + 20, heartTexture.Value.Height + 20));
                };
                if (style.Flip) {
                    if (style.Reaplece) { UI.DrawTexture(sb, value[0].Value, position.Y(scalaeY2)); }
                    else {
                        baseStyle = heartStylesPlayer.GetStyleForHeart(elementIndex, set.NameKey, style);
                        Asset<Texture2D> baseTexture = GetHeartTexture(player, baseStyle, elementIndex, set.NameKey);
                        if (baseTexture != null) { style.Draw(sb, ref baseTexture, position.Y(scalaeY2), ref color, num6); };
                    };
                    _posHeart.Add(position);
                    hover = UI.Hover(position, new Rectangle(0, 0, heartTexture.Value.Width + 20, heartTexture.Value.Height + 20));
                };
                if (hover) { _text = style.Text; };
            } else { ResourceOverlayLoader.DrawResource(new(snapshot, set, elementIndex, heartTexture) { position = position, color = color, origin = heartTexture.Size() / 2f, scale = scale }); };
        });
    }
    static void DrawManaInOldStyle(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdloc(1));
        c.RemoveRange(25);
        c.Index++;
        c.Emit(OpCodes.Ldloc, 6);
        c.EmitDelegate((int num) => {
            UI.DrawResourceText(Main.spriteBatch, Lang.inter[2].Value, new Vector2(800 - num + Main.screenWidth - 800, 6f), Color.CornflowerBlue, Color.RoyalBlue, Vector2.Zero);
        });
    }
    static void AddCustomTextForBar(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(MoveType.After, i => i.MatchCall(typeof(CommonResourceBarMethods).GetMethod("DrawLifeMouseOver", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, [], null)));
        c.Index -= 1;
        c.RemoveRange(1);
        c.EmitDelegate(() => {
            List<Vector2> pos = [.. _posHeart];
            bool hover = false;
            for (int i = 0; i < pos.Count; i++) {
                if (UI.Hover(pos[i], new Rectangle(0, 0, TextureAssets.Heart.Value.Width + 20, TextureAssets.Heart.Value.Height + 20))) { hover = true; break; };
            };
            if (hover && _text != null) {
                Main.instance.MouseTextHackZoom(_text);
                _posHeart.Clear();
                hover = false;
            } else { CommonResourceBarMethods.DrawLifeMouseOver(); };
        });
    }
    static void DrawLifeInNewStyle(ILContext il) {
        ILCursor c = new(il);
        c.Index += 87;
        c.RemoveRange(37);
        c.Emit(OpCodes.Ldarg, 0); // StatsSnapshot
        c.Emit(OpCodes.Ldarg, 1); // spriteBatch
        c.Emit(OpCodes.Ldarg, 2); // isHovered
        c.Emit(OpCodes.Ldloc, 2); // value
        c.Emit(OpCodes.Ldloc, 4); // elementIndex
        c.Emit(OpCodes.Ldloc, 5); // texture
        c.Emit(OpCodes.Ldloc, 7); // drawScale
        c.Emit(OpCodes.Ldloc, 9); // rectangle
        c.Emit(OpCodes.Ldloc, 10); // position
        c.Emit(OpCodes.Ldloc, 11); // origin
        c.Emit(OpCodes.Ldloc, 12); // Rectangle2    
        c.EmitDelegate((ref ResourceDrawSettings data, SpriteBatch sb, ref bool isHovered, Point mousePos, int elementIndex, Asset<Texture2D> texture, float drawScale, Rectangle rectangle, Vector2 position, Vector2 origin, Rectangle hoverRect) => {
            if (data.DisplaySet == null) { return; }
            Player player = Main.LocalPlayer;
            HeartStyle style = player.GetModPlayer<HeartStylesPlayer>().GetStyleForHeart(elementIndex, data.DisplaySet.NameKey);
            HeartStyle baseStyle = null;
            Asset<Texture2D> styleTexture = null;
            Color color = Color.White;

            string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
            string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

            bool hover = false;

            if (style != null && !style.Reaplece) { baseStyle = player.GetModPlayer<HeartStylesPlayer>().GetStyleForHeart(elementIndex, data.DisplaySet.NameKey, style); };
            if (!CompareAssets(texture, fancyFolder + "Heart_Fill") || !CompareAssets(texture, fancyFolder + "Heart_Fill_B") || !CompareAssets(texture, barsFolder + "HP_Fill") || !CompareAssets(texture, barsFolder + "HP_Fill_Honey")) { ResourceOverlayLoader.DrawResource(new(data.StatsSnapshot, data.DisplaySet, elementIndex + data.ResourceIndexOffset, texture) { position = position, source = rectangle, origin = origin, scale = new Vector2(drawScale), SpriteBatch = sb, color = color }); };
            if (style != null) {
                Asset<Texture2D>[] value = new Asset<Texture2D>[3];
                if (style.HasAsset) { Resources.Textures.HeartAsset.TryGetValue(style.Name, out value); };
                if (CompareAssets(texture, fancyFolder + "Heart_Fill") || CompareAssets(texture, fancyFolder + "Heart_Fill_B")) { styleTexture = value[1]; };
                if (CompareAssets(texture, barsFolder + "HP_Fill") || CompareAssets(texture, barsFolder + "HP_Fill_Honey")) { styleTexture = value[2]; };
                if (style.Reaplece && styleTexture != null) {
                    texture = styleTexture;
                    hover = UI.Hover(position, new Rectangle(0, 0, texture.Value.Width + 20, texture.Value.Height + 20));
                } else if (!style.Flip && styleTexture != null) {
                    style.Draw(sb, ref texture, position, ref color, 1f);
                    hover = UI.Hover(position, new Rectangle(0, 0, texture.Value.Width + 20, texture.Value.Height + 20));
                };
                if (value[2] == texture || value[1] == texture || CompareAssets(texture, fancyFolder + "Heart_Fill") || CompareAssets(texture, fancyFolder + "Heart_Fill_B") || CompareAssets(texture, barsFolder + "HP_Fill") || CompareAssets(texture, barsFolder + "HP_Fill_Honey")) { ResourceOverlayLoader.DrawResource(new(data.StatsSnapshot, data.DisplaySet, elementIndex + data.ResourceIndexOffset, texture) { position = position, source = rectangle, origin = origin, scale = new Vector2(drawScale), SpriteBatch = sb, color = color }); };
                if (style.Flip) { 
                    int scaleX = 0;
                    int scaleY = 0;

                    if (CompareAssets(texture, fancyFolder + "Heart_Left")) {
                        styleTexture = value[1];
                        scaleX = 15;
                        scaleY = 15;
                    };
                    if (CompareAssets(texture, fancyFolder + "Heart_Right")) {
                        styleTexture = value[1];
                        scaleX = 11;
                        scaleY = 15;
                    };
                    if (CompareAssets(texture, fancyFolder + "Heart_Middle")) {
                        styleTexture = value[1];
                        scaleX = 11;
                        scaleY = 15;
                    };
                    if (CompareAssets(texture, fancyFolder + "Heart_Right_Fancy")) {
                        styleTexture = value[1];
                        scaleX = 19;
                        scaleY = 19;
                    };
                    if (CompareAssets(texture, barsFolder + "HP_Panel_Middle")) {
                        styleTexture = value[2];
                        scaleX = 6;
                        scaleY = 12;
                    };
                    if (CompareAssets(texture, barsFolder + "Panel_Left")) {
                        styleTexture = value[2];
                        scaleX = 6;
                        scaleY = 12;
                    };
                    if (scaleX != 0 || scaleY != 0) {
                        if (style.Reaplece) { UI.DrawTexture<Vector2>(sb, styleTexture.Value, new(position.X + scaleX, position.Y + scaleY)); }
                        else {
                            baseStyle = player.GetModPlayer<HeartStylesPlayer>().GetStyleForHeart(elementIndex, data.DisplaySet.NameKey, style);
                            Asset<Texture2D> baseTexture = GetHeartTexture(player, baseStyle, elementIndex, data.DisplaySet.NameKey);
                            if (baseTexture != null) { style.Draw(sb, ref baseTexture, new(position.X + scaleX, position.Y + scaleY), ref color, 1f); };
                        };
                        hover = UI.Hover(position, new Rectangle(0, 0, texture.Value.Width + 20, texture.Value.Height + 20));
                    };
                };
                if (hoverRect.Contains(mousePos)) {
                    if (hover && style.DrawCustomText) {
                        Main.instance.MouseTextHackZoom(style.Text);
                        Main.mouseText = true;
                    } else { isHovered = true; };
                };
            } else {
                ResourceOverlayLoader.DrawResource(new(data.StatsSnapshot, data.DisplaySet, elementIndex + data.ResourceIndexOffset, texture) { position = position, source = rectangle, origin = origin, scale = new Vector2(drawScale), SpriteBatch = sb, color = color });
                if (hoverRect.Contains(mousePos)) { isHovered = true; };
            };
        });
    }
    static void FixHover(ILContext il) {
        ILCursor c = new(il);
        c.Index += 3;
        c.Remove();
        c.Emit(OpCodes.Ldarg, 0);
        c.EmitDelegate((HorizontalBarsPlayerResourcesDisplaySet set) => {
            if (ResourceOverlayLoader.DisplayHoverText(new PlayerStatsSnapshot(Main.LocalPlayer), set, true)) { CommonResourceBarMethods.DrawLifeMouseOver(); };
        });
    }
    static void IL_HorizontalBarsPlayerResourcesDisplaySet_DrawLifeBarText(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdarg(0));
        c.RemoveRange(65);
        c.Emit(OpCodes.Ldloc, 0);
        c.Emit(OpCodes.Ldloc, 4);
        c.EmitDelegate((Vector2 vector, Vector2 vector2) => {
            Color color = Color.Brown;
            Color color2 = Color.Crimson;
            UI.DrawResourceText(Main.spriteBatch, Lang.inter[0].Value, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), color2, color, Vector2.Zero);
            UI.DrawResourceText(Main.spriteBatch, Main.LocalPlayer.statLife + "/" + Main.LocalPlayer.statLifeMax2, vector + new Vector2(vector2.X * 0.5f, 0f), color2, color, new Vector2(FontAssets.MouseText.Value.MeasureString(Main.LocalPlayer.statLife + "/" + Main.LocalPlayer.statLifeMax2).X, 0f));
        });
    }
    static void FixManaText(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdarg(0));
        c.RemoveRange(48);
        c.Emit(OpCodes.Ldarg, 0); // SB;
        c.Emit(OpCodes.Ldloc, 0); // color
        c.Emit(OpCodes.Ldloc, 3); // text;
        c.Emit(OpCodes.Ldloc, 4); // text2;
        c.Emit(OpCodes.Ldloc, 5); // vector;
        c.Emit(OpCodes.Ldloc, 7); // vector2;
        c.EmitDelegate((SpriteBatch sB, Color color, string text, string text2, Vector2 pos, Vector2 pos2) => {
            int GetMpScale() {
                int scale = 0;
                int mp = Main.LocalPlayer.statManaMax;
                int count = (mp / 20);
                if (mp > 200) { for (int i = 1; i < mp / 40; i++) { if (mp % 20 * (2 + i) == 0) { scale += (mp < 800) ? 3 : 4; }; }; };
                return ((mp / 20) * 10) + scale;
            }
            pos = new(pos.X + ((Main.LocalPlayer.statManaMax <= 80) ? 36 : 16) - GetMpScale(), pos.Y - 20);
            //DarkMagenta DarkOrchid SteelBlue SlateBlue RoyalBlue DarkSlateBlue
            //Color.Brown Color.Chocolate DarkViolet DeepPink Fuchsia HotPink IndianRed Indigo LightPink Magenta Maroon MediumOrchid MediumPurple Violet
            // CornflowerBlue + RoyalBlue
            UI.DrawResourceText(sB, text, pos + new Vector2((0f - pos2.X) * 0.5f, 0f), Color.CornflowerBlue, Color.RoyalBlue, Vector2.Zero);
            UI.DrawResourceText(sB, text2, pos + new Vector2(pos2.X * 0.5f, 0f), Color.CornflowerBlue, Color.RoyalBlue, new Vector2(FontAssets.MouseText.Value.MeasureString(text2).X, 0f));
        });
    }
    static void AddManaText(ILContext il) {
        ILCursor c = new(il);
        c.Emit(OpCodes.Ldarg, 0);
        c.Emit(OpCodes.Ldarg, 1);
        c.EmitDelegate((FancyClassicPlayerResourcesDisplaySet self, SpriteBatch sB) => {
            if (!(bool)typeof(FancyClassicPlayerResourcesDisplaySet).GetField("_drawText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self)) { return; };
            Vector2 vector = FontAssets.MouseText.Value.MeasureString(Lang.inter[2].Value);
            int num = vector.X >= 45f ? (int)vector.X + 5 : 50;
            UI.DrawResourceText(sB, Lang.inter[2].Value, new(Main.screenWidth - num, 6f), Color.CornflowerBlue, Color.RoyalBlue, Vector2.Zero);
        });
    }
    static void IL_FancyClassicPlayerResourcesDisplaySet_DrawLifeBarText(ILContext il) {
        ILCursor c = new(il);
        c.GotoNext(i => i.MatchLdarg(0));
        c.RemoveRange(65);
        c.Emit(OpCodes.Ldloc, 0);
        c.Emit(OpCodes.Ldloc, 4);
        c.EmitDelegate((Vector2 vector, Vector2 vector2) => {
            Color color = Color.Brown;
            Color color2 = Color.Crimson;
            UI.DrawResourceText(Main.spriteBatch, Lang.inter[0].Value, vector + new Vector2((0f - vector2.X) * 0.5f, 0f), color2, color, Vector2.Zero);
            UI.DrawResourceText(Main.spriteBatch, Main.LocalPlayer.statLife + "/" + Main.LocalPlayer.statLifeMax2, vector + new Vector2(vector2.X * 0.5f, 0f), color2, color, new Vector2(FontAssets.MouseText.Value.MeasureString(Main.LocalPlayer.statLife + "/" + Main.LocalPlayer.statLifeMax2).X, 0f));
        });
    }
    static void IL_CommonResourceBarMethods_DrawLifeMouseOver(ILContext il) {
        ILCursor c = new(il);
        ILForDrawCommonResourceBarMethods(ref c);
        c.EmitDelegate((string text) => {
            UI.DrawMouseText(Main.spriteBatch, text, Color.Brown, Color.Crimson, 72, true, Main.UIScale);
        });
    }
    static void IL_CommonResourceBarMethods_DrawManaMouseOver(ILContext il) {
        ILCursor c = new(il);
        ILForDrawCommonResourceBarMethods(ref c);
        c.EmitDelegate((string text) => {
            UI.DrawMouseText(Main.spriteBatch, text, Color.CornflowerBlue, Color.RoyalBlue, 72, true, Main.UIScale);
        });
    }

    static void ILForDrawCommonResourceBarMethods(ref ILCursor c) {
        c.GotoNext(i => i.MatchLdloc(1));
        c.Index--;
        c.RemoveRange(4);
        c.Emit(OpCodes.Ldloc, 1);
    }

    static bool CompareAssets(Asset<Texture2D> currentAsset, string compareAssetPath) {
        if (!_vanillaAssetCache.TryGetValue(compareAssetPath, out var asset)) asset = _vanillaAssetCache[compareAssetPath] = Main.Assets.Request<Texture2D>(compareAssetPath);
        return currentAsset == asset;
    }
    static Asset<Texture2D>[] GetVanillHeart(Player player, int index) {
        string fancyFolder = "Images/UI/PlayerResourceSets/FancyClassic/";
        string barsFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";

        if (player.ConsumedLifeFruit >= index) { return [TextureAssets.Heart2, GetAsset(fancyFolder + "Heart_Fill_B"), GetAsset(barsFolder + "HP_Fill_Honey")]; }
        else { return [TextureAssets.Heart, GetAsset(fancyFolder + "Heart_Fill"), GetAsset(barsFolder + "HP_Fill")]; }
        static Asset<Texture2D> GetAsset(string name) => Main.Assets.Request<Texture2D>(name);
    }
    static Asset<Texture2D> GetCurrentHeart(Asset<Texture2D>[] hearts, string passName) {
        if (passName == "HorizontalBarsWithFullText" || passName == "HorizontalBarsWithText" || passName == "HorizontalBars") { return hearts[2]; }
        else if (passName == "New" || passName == "NewWithText") { return hearts[1]; }
        else { return hearts[0]; }
    }
    static Asset<Texture2D> GetHeartTexture(Player player, HeartStyle style, int elementIndex, string displaySet) {
        if (style == null || !style.HasAsset) {
            Asset<Texture2D>[] vanilla = GetVanillHeart(player, elementIndex);
            return GetCurrentHeart(vanilla, displaySet);
        };
        if (!Resources.Textures.HeartAsset.TryGetValue(style.Name, out Asset<Texture2D>[] assets)) { return null; };
        if (displaySet == "HorizontalBarsWithFullText" || displaySet == "HorizontalBarsWithText" || displaySet == "HorizontalBars") { return assets[2]; };
        if (displaySet == "New" || displaySet == "NewWithText") { return assets[1]; };
        return assets[0];
    }

    internal static void Unload() {
        IL_ClassicPlayerResourcesDisplaySet.DrawLife -= DrawLifeInOldStyle;
        IL_ClassicPlayerResourcesDisplaySet.TryToHover -= AddCustomTextForBar;
        IL_ResourceDrawSettings.Draw -= DrawLifeInNewStyle;
        IL_HorizontalBarsPlayerResourcesDisplaySet.TryToHover -= FixHover;
    }
};