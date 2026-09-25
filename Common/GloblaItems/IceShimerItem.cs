using System;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Waybound.Common.ModSystems.WorldGens;
using Waybound.Helpers;

namespace Waybound.Common.GloblaItems {
    public class IceShimmerItem : GlobalItem {
        public override bool InstancePerEntity => true;

        int _iceShimmerTimer;
        int _tryTime;

        bool _hasBeenIceShimmered;
        bool _hasRisenAboveWater;

        int _flashTimer = 0;
        int FlashDuration = 25;

        public override void Update(Item item, ref float gravity, ref float maxFallSpeed) {
            if (item == null || !item.active || item.IsAir) { return; };
            if (!Tables.Items.IceShime.TryGetValue(item.type, out int _) && _iceShimmerTimer > 0) {
                if (_tryTime >= 30) {
                    item.velocity = Vector2.Zero;
                    gravity = 0f;
                    maxFallSpeed = 0f;
                    return;
                };
                gravity = -0.35f;
                maxFallSpeed = 4f;
                item.velocity *= 0.92f;
                _tryTime++;
                return;
            };
            if (_hasBeenIceShimmered && _hasRisenAboveWater) {
                gravity = 0f;
                maxFallSpeed = 0f;
                item.velocity *= 0.92f;
                if (item.velocity.Length() < 0.15f) { item.velocity = Vector2.Zero; };
                return;
            };
            int tileX = (int)(item.Center.X / 16f);
            int tileY = (int)(item.position.Y / 16f);
            if (!WorldGen.InWorld(tileX, tileY) || Main.tile[tileX, tileY] == null) { return; }
            bool inWater = Main.tile[tileX, tileY].LiquidAmount > 0 && Main.tile[tileX, tileY].LiquidType == LiquidID.Water;
            bool inCryoSpring = false;
            Point itemTilePos = item.Center.ToTileCoordinates();
            for (int i = 0; i < WayboundGenVars.CryoSpringPos.Count; i++) {
                GenVector gen = WayboundGenVars.CryoSpringPos[i];
                float width = gen.End.X - gen.Start.X;
                float height = gen.End.Y - gen.Start.Y;

                if (WorldHelper.CheckBiomeTile(itemTilePos.X, itemTilePos.Y, (int)width, (int)height, (int)gen.Start.X, (int)gen.Start.Y)) {
                    inCryoSpring = true;
                    break;
                };
            };
            if (!inCryoSpring) {
                _iceShimmerTimer = 0;
                return;
            };
            if (_hasBeenIceShimmered) {
                gravity = -0.35f;
                maxFallSpeed = 4f;
                if (!inWater) {
                    _hasRisenAboveWater = true;
                    item.velocity.Y = 0f;
                    gravity = 0f;
                };
                return;
            };
            if (!inWater && _iceShimmerTimer==0) {
                _iceShimmerTimer = 0;
                return;
            };
            if (inWater) {
                gravity = -0.18f;
                maxFallSpeed = 1.6f;
            }
            else if (_iceShimmerTimer > 0) {
                gravity = 0f;
                maxFallSpeed = 0f;
                item.velocity.Y -= Utils.Clamp(item.velocity.Y - 1.8f, 0, item.velocity.Y);
            };
            item.wet = true;
            _iceShimmerTimer++;
            if (_iceShimmerTimer % 3 == 0) {
                Dust d = Dust.NewDustPerfect(item.Center + Main.rand.NextVector2Circular(item.width * 0.55f, item.height * 0.55f), DustID.IceTorch, Main.rand.NextVector2Circular(0.9f, 0.9f), 80, new Color(170, 230, 255), Main.rand.NextFloat(0.95f, 1.45f));
                d.noGravity = true;
                d.fadeIn = 1.15f;
                if (Main.rand.NextBool(4)) { Dust.NewDustPerfect(item.Center, DustID.SnowflakeIce, Vector2.Zero, 120, new Color(200, 240, 255), Main.rand.NextFloat(0.8f, 1.2f)).noGravity = true; };
            };
            if (_iceShimmerTimer < 75) { return; };
            if (Tables.Items.IceShime.TryGetValue(item.type, out int value)) {
                TryIceShimmerTransform(item, value);
                return;
            };
        }
        void TryIceShimmerTransform(Item item, int itemType) {
            if (_hasBeenIceShimmered) { return; };
            _hasBeenIceShimmered = true;
            _flashTimer = FlashDuration;
            if (Main.netMode == NetmodeID.MultiplayerClient) { return; };
            int oldStack = item.stack;
            item.SetDefaults(itemType);
            item.stack = oldStack;
            if (item.stack > item.maxStack) { item.stack = item.maxStack; }
            int remaining = oldStack - item.stack;
            while (remaining > 0) {
                int spawnStack = System.Math.Min(remaining, item.maxStack);
                int newIndex = Item.NewItem(item.GetSource_Misc("IceShimmer"), (int)item.position.X, (int)item.position.Y, item.width, item.height, itemType, spawnStack);
                Item shimerItem = Main.item[newIndex];
                shimerItem.velocity = item.velocity * 0.3f + Main.rand.NextVector2Circular(1.2f, 1.2f);
                shimerItem.playerIndexTheItemIsReservedFor = item.playerIndexTheItemIsReservedFor;
                shimerItem.wet = true;
                if (shimerItem.TryGetGlobalItem(out IceShimmerItem global)) { global._hasBeenIceShimmered = true; }
                if (Main.netMode == NetmodeID.Server) { NetMessage.SendData(MessageID.SyncItem, -1, -1, null, newIndex, 1f); };
                remaining -= spawnStack;
            };
            for (int i = 0; i < 20; i++) {
                Vector2 vel = Main.rand.NextVector2Circular(3.8f, 3.8f);
                Dust.NewDustPerfect(item.Center, DustID.IceTorch, vel, 100, new Color(180, 240, 255), 1.7f).noGravity = true;
                Dust.NewDustPerfect(item.Center, DustID.SnowflakeIce, vel * 0.55f, 150, Color.White, 1.4f).noGravity = true;
            };
            SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.75f, Pitch = 0.25f }, item.Center);
            SoundEngine.PlaySound(SoundID.Item30 with { Volume = 0.5f, Pitch = 0.55f }, item.Center);
            item.velocity.Y = -4.8f;
            item.velocity.X *= 0.35f;
            if (Main.netMode == NetmodeID.Server) { NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item.whoAmI, 1f); };
        }
        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            if (!_hasBeenIceShimmered && _iceShimmerTimer <= 0 && _flashTimer <= 0)
                return true;

            Texture2D tex = TextureAssets.Item[item.type].Value;
            Rectangle frame = Main.itemAnimations[item.type] != null
                ? Main.itemAnimations[item.type].GetFrame(tex)
                : tex.Frame();

            Vector2 pos = item.Center - Main.screenPosition;
            Vector2 origin = frame.Size() * 0.5f;

            float time = Main.GlobalTimeWrappedHourly * 2.5f;
            float pulse = 0.85f + MathF.Sin(time) * 0.15f;

            if (_hasBeenIceShimmered || _flashTimer > 0)
            {
                Texture2D megaspark = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
                Vector2 megaOrigin = megaspark.Size() * 0.5f;
                float aspect = megaspark.Width / (float)megaspark.Height;
                float baseScale = scale * 0.09f * pulse;
                Vector2 scaleVec = new Vector2(baseScale / aspect, baseScale);

                Vector2 glowPos = pos + new Vector2(0f, -6f);

                Main.EntitySpriteDraw(megaspark, glowPos, null,
                    new Color(60, 130, 235) * 0.55f,
                    time * 0.15f, megaOrigin, scaleVec, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(megaspark, glowPos, null,
                    new Color(110, 185, 255) * 0.7f,
                    -time * 0.25f, megaOrigin, scaleVec * 0.78f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(megaspark, glowPos, null,
                    new Color(175, 220, 255) * 0.85f,
                    time * 0.4f, megaOrigin, scaleVec * 0.52f, SpriteEffects.None, 0f);
                Main.EntitySpriteDraw(megaspark, glowPos, null,
                    new Color(235, 248, 255) * 0.95f,
                    -time * 0.6f, megaOrigin, scaleVec * 0.28f, SpriteEffects.None, 0f);

                if (_flashTimer > 0)
                {
                    float flashProgress = 1f - _flashTimer / (float)FlashDuration;
                    float flashAlpha = 1f - flashProgress;
                    flashAlpha *= flashAlpha;

                    Texture2D flashTex = ModContent.Request<Texture2D>("Waybound/Particles/Megaspark", AssetRequestMode.ImmediateLoad).Value;
                    Vector2 flashOrigin = flashTex.Size() * 0.5f;
                    float flashScale = scale * (1.5f + flashProgress * 8f);

                    Main.graphics.GraphicsDevice.BlendState = BlendState.Additive;

                    Main.EntitySpriteDraw(flashTex, pos, null,
                        new Color(180, 230, 255) * flashAlpha,
                        0f, flashOrigin, flashScale * 0.35f, SpriteEffects.None, 0f);

                    Main.EntitySpriteDraw(flashTex, pos, null,
                        new Color(235, 250, 255) * flashAlpha * 0.9f,
                        MathHelper.PiOver2, flashOrigin, flashScale * 0.22f, SpriteEffects.None, 0f);

                    for (int i = 0; i < 6; i++)
                    {
                        float rot = MathHelper.TwoPi * i / 6f + time;
                        Vector2 dir = rot.ToRotationVector2();
                        Main.EntitySpriteDraw(flashTex, pos + dir * (flashProgress * 40f), null,
                            new Color(200, 240, 255) * flashAlpha * 0.7f,
                            rot, flashOrigin, flashScale * 0.08f, SpriteEffects.None, 0f);
                    }

                    Main.graphics.GraphicsDevice.BlendState = BlendState.AlphaBlend;
                }

                Main.EntitySpriteDraw(tex, pos, frame, lightColor, rotation, origin, scale, SpriteEffects.None, 0f);
                return false;
            }

            Color color = lightColor;
            color.R = (byte)MathHelper.Clamp(color.R + 50, 0, 255);
            color.G = (byte)MathHelper.Clamp(color.G + 65, 0, 255);
            color.B = (byte)MathHelper.Clamp(color.B + 95, 0, 255);
            color.A = (byte)(color.A * 0.9f);

            scale *= 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + item.whoAmI) * 0.045f;

            spriteBatch.Draw(tex, pos, frame, color, rotation, origin, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, pos, frame, new Color(130, 205, 255, 55), rotation, origin, scale * 1.2f, SpriteEffects.None, 0f);

            Lighting.AddLight(item.Center, new Vector3(0.35f, 0.65f, 1.0f) * (0.55f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + item.whoAmI) * 0.15f));

            return false;
        }
        public override void UpdateInventory(Item item, Player player) {
            if (item.GetGlobalItem<IceShimmerItem>()._tryTime > 0 || item.GetGlobalItem<IceShimmerItem>()._iceShimmerTimer > 0) {
                _tryTime = 0;
                _iceShimmerTimer = 0;
                _hasBeenIceShimmered = false;
                _hasRisenAboveWater = false;
                if (_flashTimer > 0)
                    _flashTimer--;
            }
            ;
        }
    }
}