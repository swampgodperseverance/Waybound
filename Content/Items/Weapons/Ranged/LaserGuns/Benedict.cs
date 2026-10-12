using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Common.GlobalPlayer;
using Waybound.Content.Items.Weapons.Ranged.LaserGuns.GemLaserGuns;
using static Terraria.ModLoader.ModContent;

namespace Waybound.Content.Items.Weapons.Ranged.LaserGuns
{
    public class Benedict : LaserGun
    {
        public override void SetDefaults()
        {
            base.SetDefaults(); 
            chargeAdd = 1;
            chargeRemove = 3;
            chargeMax = 360;

            Item.damage = 85;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 62;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.autoReuse = true;
            Item.knockBack = 1f;
            Item.UseSound = null;
            Item.rare = ItemRarityID.Red;
            Item.value = Item.sellPrice(0, 30, 0, 0);
            Item.shoot = ProjectileType<BenedictLaser>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            return player.ownedProjectileCounts[ProjectileType<BenedictLaser>()] < 1;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-6f, 0f);
    }

    public class BenedictLaser : ModProjectile
    {
        //I simply copied the ranged laser since it doesnt have bounces ability

        public const int MaxBounces = 15;      
        public const float MaxLength = 12000f;  // RayLength
        public const float MuzzleOffset = 90f; // playerOffset
        public const float WidthMul = 5.4f;    // Hitbox and appearance
        private const float Step = 4f;       
        private const float DrawStep = 6f;    
        private const float BaseWidth = 27f;   

        public float moveSpeed = 2f;
        public float rotateToDecrease = 22.5f;

        private static readonly Color ColorBG = new Color(70, 4, 22);
        private static readonly Color ColorAround = new Color(150, 14, 50);
        private static readonly Color ColorCore = new Color(225, 95, 125);

        private const int LaserDust = DustID.RedTorch;

        private static Texture2D beamTexture;

        private List<Vector2> points;
        private bool endsOnWall;
        private float textureTimer;

        public override string Texture => "Terraria/Images/Projectile_0";

        private float Charge01 => MathHelper.Clamp((Projectile.scale - 0.25f) / 0.75f, 0f, 1f);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 8;

            points = new List<Vector2>();
        }

        private static void EnsureTexture()
        {
            if (beamTexture == null || beamTexture.IsDisposed)
                beamTexture = Request<Texture2D>("Waybound/Content/Projectiles/LaserBeam", AssetRequestMode.ImmediateLoad).Value;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.active || player.dead || player.HeldItem.type != ItemType<Benedict>() || !player.controlUseItem)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            Projectile.Center = player.Center;

            UpdatePlayer(player);

            if (!Main.dedServ)
                EnsureTexture();

            if (beamTexture != null)
                textureTimer = (textureTimer + 0.5f + Charge01 * 0.8f) % beamTexture.Height;

            BuildPath(player);
            CastLights();
            SpawnEffects(player);

            if (Projectile.soundDelay <= 0)
            {
                SoundEngine.PlaySound(SoundID.Item15 with { Volume = 0.55f, Pitch = -0.5f + Charge01 * 0.5f }, player.Center);
                Projectile.soundDelay = 18;
            }

            if (Projectile.owner == Main.myPlayer && Charge01 > 0.9f && Main.GameUpdateCount % 20 == 0)
                player.GetModPlayer<ScreenShakePlayer>().TriggerShake(8, 0.7f);
        }

        private void UpdatePlayer(Player player)
        {
            if (Projectile.owner == Main.myPlayer)
            {
                float oldScaleSync = Projectile.ai[0];
                Vector2 oldVel = Projectile.velocity;

                Vector2 diff = Main.MouseWorld - player.Center;
                float rotation = MathHelper.ToDegrees(Projectile.velocity.ToRotation());
                float rotate = MathHelper.ToDegrees(diff.ToRotation());

                if (Math.Abs(rotation - rotate) >= 180f)
                {
                    if (rotation < 0) rotation = 360f - Math.Abs(rotation);
                    if (rotate < 0) rotate = 360f - Math.Abs(rotate);
                }

                float scaleNow = player.GetModPlayer<PlayerLaserGun>().laserScale;
                float lerpFactor = 1f / Math.Max(1f, Math.Abs(rotation - rotate)) * moveSpeed * (1.25f - scaleNow)
                    * (Math.Min(Math.Abs(rotation - rotate), rotateToDecrease) / rotateToDecrease);

                Projectile.velocity = new Vector2(1f, 0f).RotatedBy(MathHelper.ToRadians(MathHelper.Lerp(rotation, rotate, lerpFactor)));
                Projectile.direction = Projectile.velocity.X > 0f ? 1 : -1;
                player.direction = Projectile.direction;

                Projectile.ai[0] = scaleNow;

                if (Vector2.Dot(oldVel.SafeNormalize(Vector2.UnitX), Projectile.velocity) < 0.9995f || Math.Abs(oldScaleSync - scaleNow) > 0.02f)
                    Projectile.netUpdate = true;
            }

            Projectile.scale = Math.Max(Projectile.ai[0], 0.25f);

            int dir = Projectile.direction;
            player.ChangeDir(dir);
            player.itemRotation = (float)Math.Atan2(Projectile.velocity.Y * dir, Projectile.velocity.X * dir);
            player.itemAnimation = player.itemAnimationMax;
            player.itemTime = player.itemTimeMax;

            player.HeldItem.color = Color.Lerp(Color.White, new Color(200, 70, 90), Projectile.scale);
            player.lastVisualizedSelectedItem = player.HeldItem;

            Projectile.damage = (int)(player.GetTotalDamage(DamageClass.Ranged).ApplyTo(player.HeldItem.damage) * Projectile.scale);
        }
//new wall bouncing here
        private void BuildPath(Player player)
        {
            points.Clear();
            endsOnWall = false;

            Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            Vector2 pos = player.Center + dir * MuzzleOffset;
            if (!Collision.CanHitLine(player.Center, 1, 1, pos, 1, 1))
                pos = player.Center;

            points.Add(pos);

            float remaining = MaxLength;
            int bounces = 0;

            while (remaining > 0f)
            {
                bool hit = false;
                Point prevTile = pos.ToTileCoordinates();
                Point hitTile = prevTile;

                while (remaining > 0f)
                {
                    Vector2 next = pos + dir * Step;
                    Point nt = next.ToTileCoordinates();

                    if (!WorldGen.InWorld(nt.X, nt.Y, 2) || WorldGen.SolidTile(nt.X, nt.Y))
                    {
                        hit = true;
                        hitTile = nt;
                        break;
                    }

                    pos = next;
                    prevTile = nt;
                    remaining -= Step;
                }

                points.Add(pos);

                if (!hit)
                    break;

                if (bounces >= MaxBounces)
                {
                    endsOnWall = true;
                    break;
                }

                bool changedX = hitTile.X != prevTile.X;
                bool changedY = hitTile.Y != prevTile.Y;
                bool flipX, flipY;

                if (changedX && !changedY)
                {
                    flipX = true;
                    flipY = false;
                }
                else if (changedY && !changedX)
                {
                    flipX = false;
                    flipY = true;
                }
                else
                {
                    bool solidSideX = WorldGen.InWorld(hitTile.X, prevTile.Y, 2) && WorldGen.SolidTile(hitTile.X, prevTile.Y);
                    bool solidSideY = WorldGen.InWorld(prevTile.X, hitTile.Y, 2) && WorldGen.SolidTile(prevTile.X, hitTile.Y);

                    if (solidSideX && !solidSideY) { flipX = true; flipY = false; }
                    else if (solidSideY && !solidSideX) { flipX = false; flipY = true; }
                    else { flipX = true; flipY = true; }
                }

                if (flipX) dir.X = -dir.X;
                if (flipY) dir.Y = -dir.Y;
                dir = dir.SafeNormalize(Vector2.UnitX);

                bounces++;
            }

            if (points.Count > 0 && endsOnWall == false && points.Count > 1 && remaining > 0f)
                endsOnWall = true;
        }

        private void CastLights()
        {
            if (points.Count < 2)
                return;

            DelegateMethods.v3_1 = ColorAround.ToVector3() * 0.9f;
            for (int i = 0; i < points.Count - 1; i++)
                Utils.PlotTileLine(points[i], points[i + 1], BaseWidth * Projectile.scale, DelegateMethods.CastLight);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (points == null || points.Count < 2)
                return false;

            float point = 0f;
            float width = BaseWidth * Projectile.scale * WidthMul * 0.8f;

            for (int i = 0; i < points.Count - 1; i++)
            {
                if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), points[i], points[i + 1], width, ref point))
                    return true;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center + Main.rand.NextVector2Circular(target.width / 2f, target.height / 2f),
                    DustID.RedTorch, Main.rand.NextVector2Circular(4f, 4f), 0, default, Main.rand.NextFloat(1f, 1.5f));
                d.noGravity = true;
            }
        }

        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            for (int i = 0; i < points.Count - 1; i++)
                Utils.PlotTileLine(points[i], points[i + 1], (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
        }
        private void SpawnEffects(Player player)
        {
            if (Main.dedServ || points.Count < 2)
                return;

            float charge = Charge01;
            float scale = Projectile.scale;
            Vector3 lc = ColorAround.ToVector3();
            Vector2 start = points[0];
            Vector2 unit0 = (points[1] - points[0]).SafeNormalize(Vector2.UnitX);
            if (Main.rand.NextBool(Math.Max(1, 3 - (int)(charge * 2f))))
            {
                Vector2 off = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(50f, 100f) * (0.6f + scale * 0.6f);
                Dust d = Dust.NewDustPerfect(start + off, LaserDust, -off * 0.07f + unit0 * 1.5f, 0, default, 0.9f + charge * 0.8f);
                d.noGravity = true;
            }

            // Дым у дула
            if (Main.rand.NextBool(30 - (int)(26f * scale)))
            {
                Gore gore = Gore.NewGorePerfect(Projectile.GetSource_FromThis(), start,
                    Vector2.Zero, Main.rand.NextFromList(GoreID.Smoke1, GoreID.Smoke2, GoreID.Smoke3), 1f);
                gore.position -= new Vector2(gore.Width, gore.Height) / 2 + unit0 * Main.rand.NextFloat(0f, MuzzleOffset - 5f);
                gore.scale = Main.rand.NextFloat(0.3f, 0.6f);
                gore.velocity = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-3f, -1f));
                gore.alpha = 120;
            }

            Lighting.AddLight(start, lc * (0.5f + charge * 0.5f));









            










            for (int i = 1; i < points.Count; i++)
            {
                Vector2 p = points[i];
                bool isEnd = i == points.Count - 1;
                if (isEnd && !endsOnWall)
                    continue;

                Vector2 dirIn = (points[i] - points[i - 1]).SafeNormalize(Vector2.UnitX);

                int amount = Main.rand.Next(2, 5 + (int)(charge * 3f));
                for (int j = 0; j < amount; j++)
                {
                    Dust d = Dust.NewDustPerfect(p, LaserDust,
                        dirIn.RotatedBy(MathHelper.ToRadians(180f + Main.rand.NextFloat(-70f, 70f))) * Main.rand.NextFloat(4f, 9f + charge * 5f),
                        0, default, Main.rand.NextFloat(1.0f, 1.4f) + charge * 0.4f);
                    d.noGravity = true;

                    Dust d2 = Dust.NewDustPerfect(p, LaserDust,
                        Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2f, 4f),
                        0, default, Main.rand.NextFloat(0.8f, 1.1f));
                    d2.noGravity = true;
                }

                Lighting.AddLight(p, lc * (0.8f + charge * 0.9f));
            }

            // Искры вдоль луча
            if (charge > 0.25f && Main.rand.NextBool(Math.Max(1, 4 - (int)(charge * 3f))))
            {
                int s = Main.rand.Next(points.Count - 1);
                Vector2 a = points[s];
                Vector2 b = points[s + 1];
                Vector2 unit = (b - a).SafeNormalize(Vector2.UnitX);
                Vector2 side = unit.RotatedBy(MathHelper.PiOver2 * (Main.rand.NextBool() ? 1 : -1));
                Vector2 pos = Vector2.Lerp(a, b, Main.rand.NextFloat()) + side * Main.rand.NextFloat(0f, 8f * scale);

                Dust d = Dust.NewDustPerfect(pos, LaserDust,
                    side * Main.rand.NextFloat(0.5f, 2.5f) + unit * Main.rand.NextFloat(-1f, 2f), 0, default, Main.rand.NextFloat(0.8f, 1.3f));
                d.noGravity = true;
            }
        }

        private static void BeginAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }

        private static void EndAdditive()
        {
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
        }


        private void DrawCap(Vector2 world, float rot, float capScale, float charge, float rowOffset)
        {
            int colW = (int)BaseWidth;
            Vector2 org = new Vector2(colW, 1f) / 2f;
            Vector2 p = world - Main.screenPosition;

            for (int i = 0; i < 180; i += 10)
            {
                float a = rot + MathHelper.ToRadians(i);

                Main.EntitySpriteDraw(beamTexture, p, new Rectangle(colW, 0, colW, 1),
                    ColorBG, a, org, capScale * 1.6f, SpriteEffects.None, 0);
                Main.EntitySpriteDraw(beamTexture, p, new Rectangle(0, ((int)(textureTimer + rowOffset)) % beamTexture.Height, colW, 1),
                    ColorCore, a, org, capScale * 1.2f, SpriteEffects.None, 0);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Main.dedServ)
                return false;

            EnsureTexture();
            if (beamTexture == null || points == null || points.Count < 2)
                return false;

            float scale = Math.Max(Projectile.scale, 0.05f) * WidthMul;
            float charge = Charge01;
            float time = Main.GlobalTimeWrappedHourly;
            int colW = (int)BaseWidth;
            Vector2 org = new Vector2(colW, 1f) / 2f;
            float capPulse = 1f + 0.12f * MathF.Sin(time * 16f);

            BeginAdditive();

            float travelled = 0f;
            for (int s = 0; s < points.Count - 1; s++)
            {
                Vector2 a = points[s];
                Vector2 b = points[s + 1];
                Vector2 seg = b - a;
                float len = seg.Length();
                if (len < 0.5f)
                    continue;

                Vector2 unit = seg / len;
                float r = unit.ToRotation() + MathHelper.PiOver2;

                float bounceFade = 1f - s * 0.07f;

                for (float i = 0f; i <= len; i += DrawStep)
                {
                    float d = travelled + i;
                    Vector2 p = a + unit * i - Main.screenPosition;

                    float wave = 1f
                        + (0.06f + 0.07f * charge) * MathF.Sin(d * 0.045f - time * 14f)
                        + 0.035f * MathF.Sin(d * 0.13f + time * 23f);

                    float taper = s == 0 ? MathHelper.Clamp(d / 40f, 0.35f, 1f) : 1f;
                    float w = scale * wave * taper * bounceFade;

                    Main.EntitySpriteDraw(beamTexture, p, new Rectangle(colW, 0, colW, 1),
                        ColorBG * ((0.28f + 0.22f * charge) * bounceFade), r, org, new Vector2(w * 2.3f, DrawStep), SpriteEffects.None, 0);

                    Main.EntitySpriteDraw(beamTexture, p, new Rectangle(colW, 0, colW, 1),
                        ColorBG * (0.9f * bounceFade), r, org, new Vector2(w, DrawStep), SpriteEffects.None, 0);

                    Main.EntitySpriteDraw(beamTexture, p, new Rectangle(colW * 2, (int)textureTimer * 4 % beamTexture.Height, colW, 1),
                        ColorAround * bounceFade, r, org, new Vector2(w * 1.05f, DrawStep), SpriteEffects.None, 0);


                    Main.EntitySpriteDraw(beamTexture, p, new Rectangle(0, ((int)textureTimer + (int)d / 12) % beamTexture.Height, colW, 1),
                        ColorCore * bounceFade, r, org, new Vector2(w * 0.55f, DrawStep), SpriteEffects.None, 0);

                    if (charge > 0.5f)
                    {
                        Main.EntitySpriteDraw(beamTexture, p, new Rectangle(0, ((int)textureTimer * 2 + (int)d / 6) % beamTexture.Height, colW, 1),
                            ColorCore * ((charge - 0.5f) * 1.6f * bounceFade), r, org, new Vector2(w * 0.25f, DrawStep), SpriteEffects.None, 0);
                    }
                }

                travelled += len;
            }

            DrawCap(points[0], points[0] == Vector2.Zero ? 0f : (points[1] - points[0]).ToRotation() + MathHelper.PiOver2, scale * 1.1f * capPulse, charge, 0f);

            for (int i = 1; i < points.Count; i++)
            {
                bool isEnd = i == points.Count - 1;
                if (isEnd && !endsOnWall)
                    continue;

                float k = 1f - (i - 1) * 0.07f;
                float rot = (points[i] - points[i - 1]).ToRotation() + MathHelper.PiOver2;

                DrawCap(points[i], rot, scale * (isEnd ? 1.5f : 1.3f) * capPulse * k, charge, i * 37f);

            }

            EndAdditive();
            return false;
        }
    }
}