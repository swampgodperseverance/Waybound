using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Waybound.Content.Tiles.Relic
{
    public abstract class BaseRelic : ModTile {
        public const int FrameWidth = 18 * 3;
        public const int FrameHeight = 18 * 4;
        public const int HorizontalFrames = 1;
        public const int VerticalFrames = 1;

        public Asset<Texture2D> RelicTexture;
        string RelicTextureName => $"Waybound/Assets/Textures/Relic/{ExtraTextureName}";

        public abstract string ExtraTextureName { get; }
        public override string Texture => "Waybound/Assets/Textures/Relic/RelicPedestal";
        public override void Load() {
            RelicTexture = Request<Texture2D>(RelicTextureName);
        }
        public override void SetStaticDefaults() {
            Main.tileShine[Type] = 400;
            Main.tileFrameImportant[Type] = true;
            TileID.Sets.InteractibleByNPCs[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.newTile.StyleHorizontal = false;
            TileObjectData.newTile.StyleWrapLimitVisualOverride = 2;
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.StyleWrapLimit = 2;
            TileObjectData.newTile.styleLineSkipVisualOverride = 0;
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(233, 207, 94), Language.GetText("MapObject.Relic"));
        }
        public override bool CreateDust(int i, int j, ref int type) {
            return false;
        }
        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) {
            tileFrameX %= FrameWidth;
            tileFrameY %= FrameHeight * 2;
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData) {
            if (drawData.tileFrameX % FrameWidth == 0 && drawData.tileFrameY % FrameHeight == 0) {
                Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
            }
        }
        public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch) {
            Point p = new(i, j);
            Tile tile = Main.tile[p.X, p.Y];
            if (!tile.HasTile) {
                return;
            }

            Texture2D texture = RelicTexture.Value;

            int frameY = tile.TileFrameX / FrameWidth;
            Rectangle frame = texture.Frame(HorizontalFrames, VerticalFrames, 0, frameY);

            Vector2 origin = frame.Size() / 2f;
            Vector2 worldPos = p.ToWorldCoordinates(24f, 64f);

            Color color = Lighting.GetColor(p.X, p.Y);

            bool direction = tile.TileFrameY / FrameHeight != 0;
            SpriteEffects effects = direction ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            const float TwoPi = (float)Math.PI * 2f;
            float offset = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 5f);
            Vector2 drawPos = worldPos - Main.screenPosition + new Vector2(0f, -40f) + new Vector2(0f, offset * 4f);

            spriteBatch.Draw(texture, drawPos, frame, color, 0f, origin, 1f, effects, 0f);

            float scale = (float)Math.Sin(Main.GlobalTimeWrappedHourly * TwoPi / 2f) * 0.3f + 0.7f;
            Color effectColor = color;
            effectColor.A = 0;
            effectColor = effectColor * 0.1f * scale;
            for (float num5 = 0f; num5 < 1f; num5 += 355f / (678f * (float)Math.PI)) {
                spriteBatch.Draw(texture, drawPos + (TwoPi * num5).ToRotationVector2() * (6f + offset * 2f), frame, effectColor, 0f, origin, 1f, effects, 0f);
            }
        }
        public override void PostDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, Rectangle frame, Vector2 position, Color color, bool validPlacement, SpriteEffects spriteEffects) {
            bool facingRight = frame.Y / FrameHeight != 0;
            spriteEffects = facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            frame.Y %= FrameHeight;

            frame.X = frame.X / 18 * 16;
            frame.Y = frame.Y / 18 * 16;
            if (facingRight && frame.X != 16) {
                frame.X = frame.X == 0 ? 32 : 0;
            }

            spriteBatch.Draw(RelicTexture.Value, position, frame, color, 0f, Vector2.Zero, 1f, spriteEffects, 0f);
        }
    }
}