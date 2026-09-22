using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;
using Waybound.Particles;
using SysVector2 = System.Numerics.Vector2;

namespace Waybound.Content.Projectiles.Summon.PreHM
{
    public class IcebatProj : ModProjectile
    {
        private const int FlightFrames = 4;
        private const int HangFrame = 4;
        private const float OrbitRadius = 64f;
        private const float BaseChargeSpeed = 14f;
        private const float BaseReturnSpeed = 10f;
        private const float BaseIdleSpeed = 6f;
        private const float PerchSpeed = 8f;
        private const int ChargeCooldown = 40;
        private const float AttackRange = 950f;
        private const float ChargeDistance = 270f;

        private static readonly Color IceColor = new Color(140, 210, 255);

        private Vector2 lockedPerchPos = Vector2.Zero;
        private Vector2 oldPos = Vector2.Zero;

        public int Stacks
        {
            get => (int)Projectile.ai[2];
            set => Projectile.ai[2] = value;
        }

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 5;
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.scale = 1f;
            Projectile.netImportant = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void OnSpawn(IEntitySource source)
        {
            if (Stacks < 1) Stacks = 1;
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            foreach (var other in Main.ActiveProjectiles)
            {
                if (other.whoAmI != Projectile.whoAmI && other.owner == owner.whoAmI && other.type == Type)
                    other.Kill();
            }

            int stacks = Math.Max(1, Stacks);

            float power = stacks;
            float chargeSpeed = BaseChargeSpeed * (0.85f + power * 0.20f);
            float idleSpeed = BaseIdleSpeed * (0.90f + power * 0.13f);
            float returnSpeed = BaseReturnSpeed * (0.90f + power * 0.13f);
            Projectile.scale = 0.92f + power * 0.03f;
            Projectile.minionSlots = stacks;

            if (Projectile.ai[0] != 3 || Projectile.localAI[1] < 0.95f)
            {
                if (Math.Abs(Projectile.velocity.X) > 0.4f)
                    Projectile.spriteDirection = Projectile.velocity.X > 0f ? -1 : 1;
            }

            Lighting.AddLight(Projectile.Center, 0.18f * power, 0.42f * power, 0.80f * power);

            SpawnAmbientParticles(stacks);

            SearchForTargets(owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter);

            if (Projectile.localAI[0] > 0)
                Projectile.localAI[0]--;

            switch ((int)Projectile.ai[0])
            {
                case 0:
                    IdleOrbit(owner, foundTarget, distanceFromTarget, targetCenter, idleSpeed, stacks);
                    break;
                case 1:
                    ChargeAttack(owner, foundTarget, distanceFromTarget, targetCenter, chargeSpeed, stacks);
                    break;
                case 2:
                    ReturnToPlayer(owner, foundTarget, distanceFromTarget, targetCenter, returnSpeed);
                    break;
                case 3:
                    PerchBehavior(owner, foundTarget, distanceFromTarget, targetCenter);
                    break;
            }

            if (oldPos == Vector2.Zero)
                oldPos = Projectile.Center;
            else if (!Main.gamePaused)
                oldPos = Vector2.Lerp(oldPos, Projectile.Center, 0.20f);

            Animate();
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<IcebatBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<IcebatBuff>()))
                Projectile.timeLeft = 2;

            return true;
        }

        private void SearchForTargets(Player owner, out bool foundTarget, out float distanceFromTarget, out Vector2 targetCenter)
        {
            distanceFromTarget = AttackRange;
            targetCenter = Projectile.Center;
            foundTarget = false;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[owner.MinionAttackTargetNPC];
                float between = Vector2.Distance(npc.Center, Projectile.Center);
                if (between < 2200f)
                {
                    distanceFromTarget = between;
                    targetCenter = npc.Center;
                    foundTarget = true;
                }
            }

            if (!foundTarget)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy())
                    {
                        float between = Vector2.Distance(npc.Center, Projectile.Center);
                        bool closest = Vector2.Distance(Projectile.Center, targetCenter) > between;
                        bool inRange = between < distanceFromTarget;
                        bool lineOfSight = Collision.CanHitLine(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height);
                        bool closeThroughWall = between < 160f;

                        if (((closest && inRange) || !foundTarget) && (lineOfSight || closeThroughWall))
                        {
                            distanceFromTarget = between;
                            targetCenter = npc.Center;
                            foundTarget = true;
                        }
                    }
                }
            }

            Projectile.friendly = foundTarget && Projectile.ai[0] == 1;
        }

        private void IdleOrbit(Player owner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float idleSpeed, int stacks)
        {
            if (TryFindPerchSpot(owner, out Vector2 perchPos))
            {
                Projectile.ai[0] = 3;
                Projectile.ai[1] = 0;
                Projectile.localAI[1] = 0f;
                lockedPerchPos = perchPos;
                Projectile.netUpdate = true;
                return;
            }

            if (foundTarget && distanceFromTarget < ChargeDistance && Projectile.localAI[0] <= 0)
            {
                Projectile.ai[0] = 1;
                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
                return;
            }

            Projectile.ai[1] += 0.045f + stacks * 0.01f;
            float angle = Projectile.ai[1];

            Vector2 desiredPos = owner.Center + new Vector2(
                (float)Math.Cos(angle) * OrbitRadius,
                (float)Math.Sin(angle) * OrbitRadius * 0.65f - 20f
            );

            Vector2 toDesired = desiredPos - Projectile.Center;
            float dist = toDesired.Length();

            float speed = idleSpeed;
            float inertia = 16f;

            if (dist > 280f)
            {
                speed = 13f + stacks * 1.8f;
                inertia = 11f;
            }

            if (dist > 8f)
            {
                toDesired.Normalize();
                toDesired *= speed;
                Projectile.velocity = (Projectile.velocity * (inertia - 1f) + toDesired) / inertia;
            }
            else
            {
                Projectile.velocity *= 0.9f;
            }
        }

        private void ChargeAttack(Player owner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float chargeSpeed, int stacks)
        {
            if (!foundTarget)
            {
                Projectile.ai[0] = 2;
                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
                return;
            }

            Vector2 toTarget = targetCenter - Projectile.Center;
            float dist = toTarget.Length();

            if (dist < 26f || Projectile.ai[1] > 42)
            {
                Projectile.ai[0] = 2;
                Projectile.ai[1] = 0;
                Projectile.localAI[0] = ChargeCooldown;
                Projectile.netUpdate = true;
                return;
            }

            Projectile.ai[1]++;

            toTarget.Normalize();
            toTarget *= chargeSpeed;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget, 0.19f + stacks * 0.025f);
        }

        private void ReturnToPlayer(Player owner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter, float returnSpeed)
        {
            Vector2 idlePos = owner.Center + new Vector2(0, -40f);
            Vector2 toIdle = idlePos - Projectile.Center;
            float dist = toIdle.Length();

            if (dist < 48f)
            {
                Projectile.ai[0] = 0;
                Projectile.ai[1] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Projectile.netUpdate = true;
                return;
            }

            if (foundTarget && distanceFromTarget < ChargeDistance && Projectile.localAI[0] <= 0)
            {
                Projectile.ai[0] = 1;
                Projectile.ai[1] = 0;
                Projectile.netUpdate = true;
                return;
            }

            toIdle.Normalize();
            toIdle *= returnSpeed;
            Projectile.velocity = (Projectile.velocity * 11f + toIdle) / 12f;
        }

        private void PerchBehavior(Player owner, bool foundTarget, float distanceFromTarget, Vector2 targetCenter)
        {
            if (foundTarget && distanceFromTarget < ChargeDistance * 1.25f && Projectile.localAI[0] <= 0)
            {
                Projectile.ai[0] = 1;
                Projectile.ai[1] = 0;
                Projectile.localAI[1] = 0f;
                lockedPerchPos = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            float playerDist = Vector2.Distance(Projectile.Center, owner.Center);
            if (playerDist > 280f)
            {
                Projectile.ai[0] = 0;
                Projectile.ai[1] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Projectile.localAI[1] = 0f;
                lockedPerchPos = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            if (lockedPerchPos == Vector2.Zero)
            {
                if (!TryFindPerchSpot(owner, out Vector2 newPerch))
                {
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                    Projectile.localAI[1] = 0f;
                    Projectile.netUpdate = true;
                    return;
                }
                lockedPerchPos = newPerch;
            }

            Point tileCheck = lockedPerchPos.ToTileCoordinates();
            tileCheck.Y -= 1;

            bool stillValid = false;
            if (WorldGen.InWorld(tileCheck.X, tileCheck.Y))
            {
                Tile t = Main.tile[tileCheck.X, tileCheck.Y];
                if (t.HasUnactuatedTile && Main.tileSolid[t.TileType] && !Main.tileSolidTop[t.TileType])
                {
                    int belowY = tileCheck.Y + 1;
                    if (WorldGen.InWorld(tileCheck.X, belowY))
                    {
                        Tile below = Main.tile[tileCheck.X, belowY];
                        bool belowSolid = below.HasUnactuatedTile && Main.tileSolid[below.TileType] && !Main.tileSolidTop[below.TileType];
                        if (!belowSolid)
                            stillValid = true;
                    }
                }
            }

            if (!stillValid)
            {
                Projectile.ai[0] = 0;
                Projectile.ai[1] = Main.rand.NextFloat(0f, MathHelper.TwoPi);
                Projectile.localAI[1] = 0f;
                lockedPerchPos = Vector2.Zero;
                Projectile.netUpdate = true;
                return;
            }

            if (Projectile.localAI[1] >= 0.95f)
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = lockedPerchPos;
                Projectile.rotation = MathHelper.Pi;
                Projectile.frame = HangFrame;
                return;
            }

            Vector2 toPerch = lockedPerchPos - Projectile.Center;
            float dist = toPerch.Length();

            if (dist > 12f)
            {
                toPerch.Normalize();
                toPerch *= PerchSpeed;
                Projectile.velocity = (Projectile.velocity * 9f + toPerch) / 10f;
                Projectile.localAI[1] = MathHelper.Clamp(Projectile.localAI[1] + 0.04f, 0f, 1f);
                Projectile.rotation = MathHelper.Lerp(Projectile.rotation, MathHelper.Pi, 0.1f);
            }
            else
            {
                Projectile.velocity = Vector2.Zero;
                Projectile.Center = lockedPerchPos;
                Projectile.localAI[1] = 1f;
                Projectile.rotation = MathHelper.Pi;
                Projectile.frame = HangFrame;
            }
        }

        private bool TryFindPerchSpot(Player owner, out Vector2 perchPos)
        {
            perchPos = Vector2.Zero;
            Point playerTile = owner.Center.ToTileCoordinates();

            for (int y = 3; y <= 16; y++)
            {
                int checkY = playerTile.Y - y;
                if (checkY < 8) break;

                for (int x = -4; x <= 4; x++)
                {
                    int checkX = playerTile.X + x;
                    if (!WorldGen.InWorld(checkX, checkY)) continue;

                    Tile tile = Main.tile[checkX, checkY];
                    if (!tile.HasUnactuatedTile || !Main.tileSolid[tile.TileType] || Main.tileSolidTop[tile.TileType])
                        continue;

                    int belowY = checkY + 1;
                    if (!WorldGen.InWorld(checkX, belowY)) continue;

                    Tile below = Main.tile[checkX, belowY];
                    bool belowSolid = below.HasUnactuatedTile && Main.tileSolid[below.TileType] && !Main.tileSolidTop[below.TileType];
                    if (belowSolid) continue;

                    int below2Y = checkY + 2;
                    if (WorldGen.InWorld(checkX, below2Y))
                    {
                        Tile below2 = Main.tile[checkX, below2Y];
                        bool below2Solid = below2.HasUnactuatedTile && Main.tileSolid[below2.TileType] && !Main.tileSolidTop[below2.TileType];
                        if (below2Solid) continue;
                    }

                    Vector2 candidate = new Vector2(checkX * 16 + 8, (checkY + 1) * 16 + 6);

                    if (candidate.Y >= owner.Center.Y - 16f) continue;
                    if (Math.Abs(candidate.X - owner.Center.X) > 90f) continue;

                    perchPos = candidate;
                    return true;
                }
            }
            return false;
        }

        private void Animate()
        {
            if (Projectile.ai[0] == 3 && Projectile.localAI[1] >= 0.92f)
            {
                Projectile.frame = HangFrame;
                return;
            }

            int frameSpeed = Projectile.ai[0] == 1 ? 3 : 5;
            if (++Projectile.frameCounter >= frameSpeed)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= FlightFrames)
                    Projectile.frame = 0;
            }

            if (Projectile.ai[0] != 3)
                Projectile.rotation = Projectile.velocity.X * 0.08f;
        }

        private void SpawnAmbientParticles(int stacks)
        {
            if (Main.dedServ || !Main.rand.NextBool(4)) return;

            Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
            Vector2 vel = Main.rand.NextVector2Circular(0.6f, 0.6f);
            float scale = Main.rand.NextFloat(4.0f, 6.8f) * (0.9f + stacks * 0.18f);

            ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                pos.ToNumerics(),
                vel.ToNumerics(),
                Main.rand.NextFloat(MathHelper.TwoPi),
                new SysVector2(scale),
                IceColor * 0.85f,
                45
            ));
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            int frameHeight = texture.Height / Main.projFrames[Type];
            Rectangle source = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 origin = source.Size() * 0.5f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            if (Projectile.frame == HangFrame)
                drawPos.Y += 4f;

            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int stacks = Math.Max(1, Stacks);

            if (stacks >= 2 && oldPos != Projectile.Center && !oldPos.HasNaNs())
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("Terraria/Images/Extra_98", AssetRequestMode.ImmediateLoad).Value;

                Vector2 trailStart = Vector2.Lerp(oldPos, Projectile.Center, 0.55f);
                float trailLength = Vector2.Distance(Projectile.Center, trailStart);
                float trailScaleY = trailLength / trailTex.Height * (2.0f + stacks * 0.4f);
                Color trailColor = new Color(70, 170, 255, 0) * (0.55f + stacks * 0.05f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor,
                    (Projectile.Center - trailStart).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.7f, trailScaleY),
                    SpriteEffects.None, 0f);

                Main.EntitySpriteDraw(trailTex, Projectile.Center - Main.screenPosition,
                    new Rectangle(0, trailTex.Height / 2, trailTex.Width, trailTex.Height / 2),
                    trailColor * 0.4f,
                    (Projectile.Center - trailStart).ToRotation() + MathHelper.PiOver2,
                    new Vector2(trailTex.Width * 0.5f, 0f),
                    new Vector2(Projectile.scale * 0.35f, trailScaleY * 1.2f),
                    SpriteEffects.None, 0f);
            }

            Color outlineColor = new Color(90, 180, 255) * (0.50f + stacks * 0.07f);
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(1.5f + stacks * 0.18f, 0f).RotatedBy(MathHelper.TwoPi / 4f * i);
                Main.EntitySpriteDraw(texture, drawPos + offset, source, outlineColor, Projectile.rotation, origin, Projectile.scale, effects, 0f);
            }

            Main.EntitySpriteDraw(texture, drawPos, source, Color.White, Projectile.rotation, origin, Projectile.scale, effects, 0f);

            return false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            int stacks = Math.Max(1, Stacks);
            modifiers.SourceDamage *= 0.80f + stacks * 0.28f;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode == NetmodeID.Server) return;

            int stacks = Math.Max(1, Stacks);
            for (int i = 0; i < 10 + stacks * 3; i++)
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(14f, 14f);
                Vector2 vel = Main.rand.NextVector2Circular(1.5f, 1.5f);
                float scale = Main.rand.NextFloat(4.2f, 7.5f) * (0.9f + stacks * 0.15f);

                ParticleSystem.SnowFlakeBuffer?.Create(new ParticleInfo(
                    pos.ToNumerics(),
                    vel.ToNumerics(),
                    Main.rand.NextFloat(MathHelper.TwoPi),
                    new SysVector2(scale),
                    IceColor * 0.9f,
                    55
                ));
            }
        }
    }
}