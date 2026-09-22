using Terraria;
using Terraria.Audio;
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
        public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) {
            if (!_hasBeenIceShimmered && _iceShimmerTimer <= 0) { return true; }
            Color color = lightColor;
            color.R = (byte)MathHelper.Clamp(color.R + 50, 0, 255);
            color.G = (byte)MathHelper.Clamp(color.G + 65, 0, 255);
            color.B = (byte)MathHelper.Clamp(color.B + 95, 0, 255);
            color.A = (byte)(color.A * 0.9f);
            scale *= 1f + (float)System.Math.Sin(Main.GameUpdateCount * 0.12f + item.whoAmI) * 0.045f;
            Main.GetItemDrawFrame(item.type, out Texture2D tex, out Rectangle frame);
            Vector2 drawPos = item.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, frame, color, rotation, frame.Size() / 2f, scale, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, frame, new(130, 205, 255, 55), rotation, frame.Size() / 2f, scale * 1.2f, SpriteEffects.None, 0f);
            Lighting.AddLight(item.Center, new Vector3(0.35f, 0.65f, 1.0f) * (0.55f + (float)System.Math.Sin(Main.GameUpdateCount * 0.1f + item.whoAmI) * 0.15f));
            return false;
        }
        public override void UpdateInventory(Item item, Player player) {
            if (item.GetGlobalItem<IceShimmerItem>()._tryTime > 0 || item.GetGlobalItem<IceShimmerItem>()._iceShimmerTimer > 0) {
                _tryTime = 0;
                _iceShimmerTimer = 0;
                _hasBeenIceShimmered = false;
                _hasRisenAboveWater = false;
            };
        }
    }
}