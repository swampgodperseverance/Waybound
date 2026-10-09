using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Items.Accessories.PreHardmode
{
    public class EmbalmedSnake : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
            Item.value = Item.sellPrice(0, 1, 0, 0);
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetDamage(DamageClass.Magic) -= 0.08f;
            player.GetModPlayer<SerpentPlayer>().Equipped = true;
        }
    }

    public class SerpentPlayer : ModPlayer
    {
        public const int MaxCharges = 3;

        public bool Equipped;
        public int BiteCharges;
        public int BiteDelay;
        public const int HitsPerCharge = 6;
        public const float PoisonedMagicBonus = 1.15f;
        public int HitCount;
        public override void ResetEffects() => Equipped = false;

        public override void PostUpdateEquips()
        {
            if (BiteDelay > 0)
                BiteDelay--;

            if (!Equipped)
            {
                BiteCharges = 0;
                return;
            }

            if (Player.whoAmI == Main.myPlayer && Player.ownedProjectileCounts[ModContent.ProjectileType<SnakeProj>()] == 0)
            {
                Projectile.NewProjectile(Player.GetSource_Misc("SerpentIdol"), Player.Center, Vector2.Zero,
                    ModContent.ProjectileType<SnakeProj>(), SnakeProj.BaseDamage, 2f, Player.whoAmI);
            }
        }

        public static int PoisonTicks(NPC n)
        {
            int ticks = 0;
            int i = n.FindBuffIndex(BuffID.Poisoned);
            if (i != -1)
                ticks += n.buffTime[i];
            i = n.FindBuffIndex(BuffID.Venom);
            if (i != -1)
                ticks += n.buffTime[i];
            return ticks;
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!Equipped || proj.type == ModContent.ProjectileType<SnakeProj>() || !proj.CountsAsClass(DamageClass.Magic))
                return;

            if (PoisonTicks(target) > 0)
                modifiers.SourceDamage *= PoisonedMagicBonus;
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!Equipped || proj.type == ModContent.ProjectileType<SnakeProj>() || !proj.CountsAsClass(DamageClass.Magic))
                return;

            if (target.lifeMax <= 5 || target.friendly)
                return;

            target.AddBuff(BuffID.Poisoned, 180);

            if (Player.whoAmI == Main.myPlayer && ++HitCount >= HitsPerCharge)
            {
                HitCount = 0;
                BiteCharges = Math.Min(MaxCharges, BiteCharges + 1);
            }
        }
    }

    public class SnakeProj : ModProjectile
    {
        public const int BaseDamage = 32;

        private const int Segments = 10;
        private const float SegmentSpacing = 12f;
        private const int ManaRestore = 6;

        private Vector2[] segs;
        private Vector2 headDir = Vector2.UnitY;
        private const float DamagePerSecondLeft = 14f;
        private int consumedTicks;

        private ref float State => ref Projectile.ai[0];
        private ref float TargetIndex => ref Projectile.ai[1];
        private ref float Timer => ref Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 600;
        }

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.timeLeft = 2;
        }

        public override bool? CanDamage() => State == 1f ? null : false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            SerpentPlayer sp = owner.GetModPlayer<SerpentPlayer>();

            if (!owner.active || owner.dead || !sp.Equipped)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;
            Projectile.damage = (int)owner.GetTotalDamage(DamageClass.Magic).ApplyTo(BaseDamage);
            Projectile.CritChance = (int)owner.GetTotalCritChance(DamageClass.Magic);

            bool isOwner = Projectile.owner == Main.myPlayer;
            Vector2 idleGoal = IdleGoal(owner);

            if (Vector2.DistanceSquared(Projectile.Center, owner.Center) > 1400f * 1400f)
            {
                Projectile.Center = owner.Center;
                Projectile.velocity = Vector2.Zero;
                State = 0f;
                Projectile.netUpdate = true;
            }

            switch ((int)State)
            {
                case 0:
                    MoveTo(idleGoal, 14f, 8f, true);

                    if (isOwner && sp.BiteCharges > 0 && sp.BiteDelay <= 0)
                    {
                        int target = FindTarget(owner);
                        if (target != -1)
                        {
                            sp.BiteCharges--;
                            sp.BiteDelay = 10;
                            State = 1f;
                            TargetIndex = target;
                            Timer = 0f;
                            Projectile.netUpdate = true;
                        }
                    }
                    break;

                case 1:
                    Timer++;
                    int idx = (int)TargetIndex;
                    NPC t = idx >= 0 && idx < Main.maxNPCs ? Main.npc[idx] : null;
                    bool valid = t != null && t.active && t.CanBeChasedBy(this);

                    if (valid)
                        MoveTo(t.Center, 22f, 5f, false);
                    else
                        MoveTo(idleGoal, 20f, 6f, false);

                    if (isOwner && (!valid || Timer > 50f))
                        EndLunge();
                    break;

                default:
                    MoveTo(idleGoal, 20f, 6f, false);
                    if (isOwner && Vector2.DistanceSquared(Projectile.Center, idleGoal) < 50f * 50f)
                    {
                        State = 0f;
                        Projectile.netUpdate = true;
                    }
                    break;
            }

            if (Projectile.velocity.LengthSquared() > 1f)
                headDir = Vector2.Normalize(Vector2.Lerp(headDir, Vector2.Normalize(Projectile.velocity), 0.25f) + new Vector2(0.0001f));

            if (Main.dedServ)
                return;

            UpdateSegments();

            if (State == 1f && Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Poisoned, -Projectile.velocity * 0.1f, 100, default, 1.1f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.1f, 0.4f, 0.12f);
        }

        private static Vector2 IdleGoal(Player owner)
        {
            float t = Main.GlobalTimeWrappedHourly;
            return owner.Center + new Vector2(MathF.Cos(t * 2.2f) * 60f, -50f + MathF.Sin(t * 3.1f) * 20f);
        }

        private void MoveTo(Vector2 goal, float maxSpeed, float inertia, bool soft)
        {
            Vector2 to = goal - Projectile.Center;
            float len = to.Length();
            Vector2 desired = Vector2.Zero;

            if (len > 1f)
                desired = to / len * (soft ? Math.Min(maxSpeed, len * 0.15f) : maxSpeed);

            Projectile.velocity = (Projectile.velocity * (inertia - 1f) + desired) / inertia;
        }

        private void EndLunge()
        {
            State = 2f;
            Projectile.netUpdate = true;
        }

        private static int FindTarget(Player owner)
        {
            int best = -1;
            float bestDist = float.MaxValue;
            Vector2 cursor = Main.MouseWorld;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC n = Main.npc[i];
                if (!n.active || !n.CanBeChasedBy())
                    continue;
                if (Vector2.DistanceSquared(n.Center, owner.Center) > 700f * 700f)
                    continue;
                if (!Collision.CanHit(owner.Center, 1, 1, n.position, n.width, n.height))
                    continue;

                float d = Vector2.DistanceSquared(n.Center, cursor);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = i;
                }
            }

            return best;
        }

        private void UpdateSegments()
        {
            if (segs == null)
            {
                segs = new Vector2[Segments];
                for (int i = 0; i < Segments; i++)
                    segs[i] = Projectile.Center - headDir * SegmentSpacing * i;
            }

            segs[0] = Projectile.Center;
            for (int i = 1; i < Segments; i++)
            {
                Vector2 d = segs[i - 1] - segs[i];
                float len = d.Length();
                Vector2 dir = len > 0.01f ? d / len : headDir;
                segs[i] = segs[i - 1] - dir * SegmentSpacing;
            }
        }


        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            consumedTicks = SerpentPlayer.PoisonTicks(target);
            modifiers.FlatBonusDamage += consumedTicks / 60f * DamagePerSecondLeft;
            modifiers.Knockback *= 0.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool detonated = consumedTicks > 0;

            if (detonated)
            {
                target.RequestBuffRemoval(BuffID.Poisoned);
                target.RequestBuffRemoval(BuffID.Venom);
            }

            target.AddBuff(BuffID.Poisoned, 120);

            if (Projectile.owner == Main.myPlayer)
            {
                if (detonated)
                {
                    Player owner = Main.player[Projectile.owner];
                    int restored = Math.Min(ManaRestore, owner.statManaMax2 - owner.statMana);
                    if (restored > 0)
                    {
                        owner.statMana += restored;
                        owner.ManaEffect(restored);
                    }
                }

                EndLunge();
            }

            if (Main.dedServ)
                return;

            int count = detonated ? 18 : 8;
            for (int i = 0; i < count; i++)
            {
                Dust d = Dust.NewDustPerfect(target.Center, DustID.Poisoned,
                    Main.rand.NextVector2Circular(detonated ? 6f : 4f, detonated ? 6f : 4f), 100, default, 1.4f);
                d.noGravity = true;
            }
        }   

        public override bool PreDraw(ref Color lightColor)
        {
            if (segs == null)
                return false;

            Texture2D tex = TextureAssets.Projectile[Type].Value;
            int fh = tex.Height / 4;
            Vector2 origin = new Vector2(tex.Width * 0.5f, fh * 0.5f);
            float wave = State == 1f ? 1.5f : 4f;

            for (int i = Segments - 1; i >= 0; i--)
            {
                int frame = i == 0 ? 0 : i == Segments - 1 ? 3 : i < Segments / 2 ? 1 : 2;

                Vector2 dir = i == 0 ? headDir : segs[i - 1] - segs[i];
                dir = dir.LengthSquared() > 0.0001f ? Vector2.Normalize(dir) : headDir;

                Vector2 perp = new Vector2(-dir.Y, dir.X);
                Vector2 pos = segs[i] + perp * MathF.Sin(Main.GlobalTimeWrappedHourly * 8f - i * 0.8f) * wave * (i / (float)Segments + 0.3f);

                Color c = Lighting.GetColor((int)(pos.X / 16f), (int)(pos.Y / 16f));
                Rectangle src = new Rectangle(0, frame * fh, tex.Width, fh);
                float rot = dir.ToRotation() - MathHelper.PiOver2;

                Main.EntitySpriteDraw(tex, pos - Main.screenPosition, src, c, rot, origin, 1f, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}