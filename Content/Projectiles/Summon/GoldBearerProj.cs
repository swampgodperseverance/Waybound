using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Waybound.Trails;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Buffs.Minions;

namespace Waybound.Content.Projectiles.Summon
{
    public class GoldBearerProj : ModProjectile
    {
        public ref float State => ref Projectile.ai[0];
        public ref float Target => ref Projectile.ai[1];
        public ref float Timer => ref Projectile.ai[2];
        public ref float Recorder => ref Projectile.localAI[0];

        public Vector2 BasePos;
        public float alpha = 0f;
        public bool CanDrawTrail;
        public bool Init = true;
        private readonly VertexStrip vertexStrip = new VertexStrip();

        public static readonly Color GoldSoft = new Color(255, 210, 120);
        public static readonly Color GoldBright = new Color(255, 235, 160);
        public static readonly Color GoldTrail = new Color(255, 200, 90);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionTargettingFeature[Type] = true;
            Main.projPet[Type] = true;
            ProjectileID.Sets.MinionSacrificable[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 14;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.timeLeft = 2;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.localNPCHitCooldown = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.netImportant = true;
            Projectile.minion = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckMinionOwnerActive(owner))
                return;

            owner.AddBuff(ModContent.BuffType<GoldBearerBuff>(), 2);

            if (Init)
            {
                Projectile.oldPos = new Vector2[14];
                Projectile.oldRot = new float[14];
                for (int i = 0; i < 14; i++)
                {
                    Projectile.oldPos[i] = Projectile.Center;
                    Projectile.oldRot[i] = Projectile.rotation;
                }
                Init = false;
                alpha = 0f;
            }

            if (alpha < 1f)
                alpha = MathHelper.Clamp(alpha + 0.045f, 0f, 1f);

            float state = State;

            if (state == -1f)
            {
                GetMyGroupIndex(out int index, out int total);
                Vector2 idleSpot = CircleMovement(32f + total * 4f, 36f, 0.6f, 5f, 0.9f, index * MathHelper.TwoPi / Math.Max(total, 1));

                if (Projectile.Distance(idleSpot) < 32f)
                {
                    Timer = 0f;
                    State = 0f;
                    Projectile.netUpdate = true;
                    CanDrawTrail = false;
                }
                Projectile.rotation = Projectile.velocity.ToRotation();
            }
            else if (state == 0f)
            {
                GetMyGroupIndex(out int index, out int total);
                CircleMovement(32f + total * 4f, 28f, 0.4f, 5f, 0.9f, index * MathHelper.TwoPi / Math.Max(total, 1));
                Projectile.rotation = (owner.Center - Projectile.Center).ToRotation();

                if (Main.rand.NextBool(20))
                {
                    int targetIndex = FindTarget(1000f);
                    if (targetIndex != -1)
                    {
                        State = 1f;
                        Timer = 0f;
                        Target = targetIndex;
                        Projectile.netUpdate = true;
                        CanDrawTrail = false;
                    }
                }
            }
            else if (state == 1f)
            {
                if (!GetNPC(Target, out NPC target))
                {
                    ResetToIdle();
                    return;
                }

                float progress = Timer / 30f;

                if (Timer == 0f)
                    BasePos = Projectile.Center;

                if (Timer == 15f)
                    CanDrawTrail = true;
                else if (Timer > 15f)
                {
                    for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
                        Projectile.oldPos[i] = Vector2.Lerp(Projectile.Center, BasePos, (float)i / Projectile.oldPos.Length);

                    SpawnGoldDust();
                    if (Timer > 22f)
                        alpha = MathHelper.Clamp(alpha - 0.06f, 0.25f, 1f);
                }

                Vector2 origin = BasePos;
                origin.Y -= Utils.GetLerpValue(0f, 0.4f, progress, true) * 100f;

                Vector2 toTarget = target.Center - origin;
                Vector2 offset = toTarget.SafeNormalize(Vector2.Zero) * MathHelper.Clamp(toTarget.Length(), 60f, 150f);
                Vector2 endPos = target.Center + offset;

                float lerpMid = Utils.GetLerpValue(0.4f, 0.6f, progress, true);
                float lerpEnd = Utils.GetLerpValue(0.6f, 1f, progress, true);

                Projectile.rotation = Projectile.rotation.AngleTowards(toTarget.ToRotation(), 0.628f);
                Projectile.Center = Vector2.Lerp(origin, target.Center, lerpMid);
                if (lerpEnd > 0f)
                    Projectile.Center = Vector2.Lerp(target.Center, endPos, lerpEnd);

                Timer++;

                if (Timer > 30f)
                {
                    alpha = 1f;
                    int next = FindTarget(1000f);
                    if (next != -1)
                    {
                        State = 2f;
                        Timer = 0f;
                        Target = next;
                        Recorder = -1.57f + Main.rand.NextFloat(-0.7f, 0.7f);
                        CanDrawTrail = false;
                        Projectile.Center = target.Center + Recorder.ToRotationVector2() * target.height;
                    }
                    else
                        ResetToIdle();
                }
            }
            else if (state == 2f)
            {
                if (!GetNPC(Target, out NPC target))
                {
                    ResetToIdle();
                    return;
                }

                if (Timer == 2f)
                {
                    for (int i = 0; i < Projectile.oldPos.Length; i++)
                    {
                        Projectile.oldPos[i] = Projectile.Center;
                        Projectile.oldRot[i] = Projectile.rotation;
                    }
                }

                if (Timer < 25f)
                {
                    Projectile.velocity = Vector2.Zero;
                    Projectile.Center = target.Center + Recorder.ToRotationVector2() * (Timer * 4f + target.height);
                    Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
                }
                else if (Timer == 25f)
                {
                    CanDrawTrail = true;
                    BasePos = Projectile.Center;
                    Projectile.velocity = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) *
                                          (Timer * 4f + target.height) / 10f;
                }
                else if (Timer < 35f)
                {
                    for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
                        Projectile.oldPos[i] = Vector2.Lerp(Projectile.Center, BasePos, (float)i / Projectile.oldPos.Length);

                    SpawnGoldDust();
                }
                else if (Timer < 43f)
                {
                    Projectile.velocity *= 0.95f;
                    alpha = MathHelper.Clamp(alpha - 0.09f, 0.15f, 1f);

                    for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
                        Projectile.oldPos[i] = Vector2.Lerp(Projectile.Center, BasePos, (float)i / Projectile.oldPos.Length);
                }
                else
                {
                    CanDrawTrail = false;
                    alpha = 1f;

                    int next = FindTarget(1000f);
                    if (next != -1)
                    {
                        State = 2f;
                        Timer = 0f;
                        Target = next;
                        Recorder = -1.57f + Main.rand.NextFloat(-0.7f, 0.7f);
                        Projectile.Center = target.Center + Recorder.ToRotationVector2() * target.height;
                    }
                    else
                        ResetToIdle();
                }

                Timer++;
            }

            Lighting.AddLight(Projectile.Center, GoldSoft.ToVector3() * 0.28f * alpha);
        }

        private bool CheckMinionOwnerActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<GoldBearerBuff>());
                return false;
            }
            if (owner.ownedProjectileCounts[Type] > 0)
                Projectile.timeLeft = 2;
            return true;
        }

        private void ResetToIdle()
        {
            Timer = 0f;
            State = -1f;
            CanDrawTrail = false;
            alpha = MathHelper.Clamp(alpha, 0.3f, 1f);
        }

        private bool GetNPC(float index, out NPC npc)
        {
            npc = null;
            int i = (int)index;
            if (i < 0 || i >= Main.maxNPCs)
                return false;
            npc = Main.npc[i];
            return npc.active && npc.CanBeChasedBy(Projectile);
        }

        private int FindTarget(float maxDist)
        {
            int result = -1;
            float minDist = maxDist;

            if (Main.player[Projectile.owner].HasMinionAttackTargetNPC)
            {
                NPC t = Main.npc[Main.player[Projectile.owner].MinionAttackTargetNPC];
                if (t.CanBeChasedBy(Projectile) && Projectile.Distance(t.Center) < maxDist)
                    return t.whoAmI;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.CanBeChasedBy(Projectile))
                    continue;

                float d = Projectile.Distance(n.Center);
                if (d < minDist)
                {
                    minDist = d;
                    result = i;
                }
            }
            return result;
        }

        private void GetMyGroupIndex(out int index, out int total)
        {
            index = 0;
            total = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Projectile.owner && p.type == Type)
                {
                    if (i == Projectile.whoAmI)
                        index = total;
                    total++;
                }
            }
        }

        private Vector2 CircleMovement(float distance, float speedMax, float accel = 0.25f,
                                       float rolling = 5f, float angleFactor = 0.9f, float baseRot = 0f)
        {
            Player owner = Main.player[Projectile.owner];
            Vector2 offset = (baseRot + Main.GlobalTimeWrappedHourly / rolling * MathHelper.TwoPi).ToRotationVector2() * distance;
            offset.Y /= 4f;
            Vector2 center = owner.Center + new Vector2(0f, -48f) + offset;

            Vector2 dir = center - Projectile.Center;
            if (dir.Length() > 2000f)
                Projectile.Center = center;

            float velRot = Projectile.velocity.ToRotation();
            float targetRot = dir.ToRotation();
            float speed = Projectile.velocity.Length();
            float aimSpeed = MathHelper.Clamp(dir.Length() / 100f, 0f, 1f) * speedMax;

            Projectile.velocity = velRot.AngleTowards(targetRot, angleFactor).ToRotationVector2() *
                                  MathHelper.Lerp(speed, aimSpeed, accel);
            return center;
        }

        private void SpawnGoldDust()
        {
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.GoldFlame,
                    Projectile.velocity * 0.15f,
                    80,
                    GoldSoft,
                    Main.rand.NextFloat(0.55f, 0.95f)
                );
                d.noGravity = true;
                d.fadeIn = 0.5f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 origin = tex.Size() / 2f;
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            float fade = alpha;

            if (CanDrawTrail)
            {
                sb.End();
                sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                GameShaders.Misc["MagicMissile"].Apply();
                vertexStrip.PrepareStripWithProceduralPadding(
                    Projectile.oldPos,
                    Projectile.oldRot,
                    p => Color.Lerp(
                        GoldTrail.MultiplyAlpha(fade * 0.95f),
                        GoldBright.MultiplyAlpha(fade * 0.25f),
                        p
                    ),
                    p => 32f * Projectile.scale * (1f - p * 0.85f),
                    -Main.screenPosition + Projectile.Size / 2f,
                    true
                );
                vertexStrip.DrawTrail();
                Main.pixelShader.CurrentTechnique.Passes[0].Apply();

                sb.End();
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Color outlineCol = GoldSoft * (0.4f + 0.15f * (float)Math.Sin(Main.GlobalTimeWrappedHourly * 4.5f)) * fade;
            for (int i = 0; i < 8; i++)
            {
                Vector2 off = (MathHelper.TwoPi / 8f * i).ToRotationVector2() * 1.8f;
                sb.Draw(tex, Projectile.Center - Main.screenPosition + off, null, outlineCol,
                    Projectile.rotation, origin, Projectile.scale * 1.06f, effects, 0f);
            }

            Color mainCol = Color.Lerp(lightColor, Color.White, 0.35f) * fade;
            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, mainCol,
                Projectile.rotation, origin, Projectile.scale, effects, 0f);

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null, GoldBright * 0.28f * fade,
                Projectile.rotation, origin, Projectile.scale * 0.9f, effects, 0f);

            return false;
        }
    }
}