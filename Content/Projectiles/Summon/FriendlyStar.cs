using Microsoft.Xna.Framework;
using static Microsoft.Xna.Framework.MathHelper;
using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using System;
using Waybound.Content.Buffs.Minions;

namespace Waybound.Content.Projectiles.Summon
{
    public class FriendlyStar : ModProjectile
    {
        private int attackTimer = 0;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 3;

            // Важно для суммонеров
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 26;
            Projectile.height = 26;

            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

   
            if (player.dead || !player.HasBuff(ModContent.BuffType<Starlord>()))
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft = 2;

         
            Lighting.AddLight(Projectile.Center, 0.8f, 0.8f, 0.3f);

       
            Projectile.rotation = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;

       
            NPC target = FindClosestNPC(850f);

            if (target == null)
            {
                IdleMovement(player);
                Projectile.frame = 0;
            }
            else
            {
                CombatMovement(target);
                CombatAnimation();
                TryShoot(target);
            }
        }

        private void IdleMovement(Player player)
        {
            float orbitTime = Main.GameUpdateCount * 0.04f + Projectile.whoAmI * 1.4f;

            Vector2 idleOffset = new Vector2(
                MathF.Cos(orbitTime) * 60f,   
                MathF.Sin(orbitTime * 0.8f) * 45f 
            );

            Vector2 idlePos = player.Center + idleOffset;

       
            Vector2 desiredVel = Vector2.Normalize(idlePos - Projectile.Center) * 5.2f;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.12f);

            if (Projectile.velocity.Length() > 7f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 7f;

            if (Main.rand.NextBool(18)) 
            {
                Dust d = Dust.NewDustDirect(
                    Projectile.position,
                    Projectile.width,
                    Projectile.height,
                    DustID.GoldFlame,
                    0f, 0f, 150, default,
                    1.1f
                );
                d.velocity *= 0.3f;
                d.noGravity = true;
            }
        }


        private void CombatMovement(NPC target)
        {
     
            float idealDist = 120f;  
            float minDist = 140f;     
            //float maxDist = 400f;     

            Vector2 toTargetDir = Vector2.Normalize(target.Center - Projectile.Center);
            float currentDist = Vector2.Distance(Projectile.Center, target.Center);

            Vector2 desiredVel;
            if (currentDist > idealDist)
            {
          
                desiredVel = toTargetDir * 8.5f;
            }
            else if (currentDist < minDist)
            {
               
                desiredVel = -toTargetDir * 6f;
            }
            else
            {
      
                Vector2 perpDir = new Vector2(-toTargetDir.Y, toTargetDir.X);  
                desiredVel = (toTargetDir * 2f + perpDir * 4f);  
                desiredVel = Vector2.Normalize(desiredVel) * 5.5f;
            }

         
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, desiredVel, 0.065f);

    
            if (Projectile.velocity.Length() > 9.5f)
                Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 9.5f;
        }

        private void CombatAnimation()
        {
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 6)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % 3;
                if (Projectile.frame == 0) Projectile.frame = 1; 
            }
        }

        private void TryShoot(NPC target)
        {
            attackTimer++;
            float distToTarget = Vector2.Distance(Projectile.Center, target.Center);
            

            if (attackTimer >= 35 && distToTarget >= 100f && distToTarget <= 200f)
            {
                attackTimer = 0;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 velocity = Vector2.Normalize(target.Center - Projectile.Center) * 13f;
                    int proj = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center,
                        velocity,
                        ProjectileID.Flare,
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner
                    );
                }

           
                for (int i = 0; i < 18; i++)
                {
                    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                        DustID.GoldFlame, 0f, 0f, Scale: 1.4f);
                    d.velocity = Vector2.Normalize(target.Center - Projectile.Center) * 0.4f + Main.rand.NextVector2Circular(2f, 2f);
                    d.noGravity = true;
                }
            }
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float maxDistSq = maxDistance * maxDistance;

            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.CanBeChasedBy(Projectile) && 
                    Vector2.DistanceSquared(n.Center, Projectile.Center) < maxDistSq)
                {
                    float newDist = Vector2.DistanceSquared(n.Center, Projectile.Center);
                    if (closest == null || newDist < maxDistSq)
                    {
                        maxDistSq = newDist;
                        closest = n;
                    }
                }
            }

            return closest;
        }
    }
}