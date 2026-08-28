
using Waybound.Helpers;
using Terraria;
using Terraria.ID;

namespace Waybound.Common.GlobalPlayer {
    public class ArmorPlayers : ModPlayer {
        //public bool equipBronzeSet;
        public bool thunderSet = false;

        public override void Initialize() {
            //equipBronzeSet = false;
            thunderSet = false;
        }
        public override void ResetEffects() {
            //equipBronzeSet = false;
            thunderSet = false;
        }
        public override void PostUpdateRunSpeeds()
        {
            if (Player.velocity.Length() <= 2f)
                return;

            if (thunderSet)
            {
                if (Main.rand.NextBool(2))
                {
                    Dust dust = Dust.NewDustDirect(
                        Player.position,
                        Player.width,
                        Player.height,
                        DustID.GoldFlame, 
                        0f,
                        0f,
                        150,
                        default,
                        1.2f
                    );

                    dust.noGravity = true;

                    Vector2 vel = Player.velocity * -0.3f;
                    vel += new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -0.2f);

                    dust.velocity = vel;

                    dust.fadeIn = 0.1f;
                }
            }
        }
        
        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
            if (thunderSet && item.DamageType == DamageClass.Melee) {
                SpawnBurst(target.Center, DustID.GemTopaz);
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
            Item item = PlayerHelpers.GetLocalItem(Player);
            if (thunderSet && (proj.CountsAsClass(DamageClass.Melee) || item.CountsAsClass(DamageClass.Throwing))) {
                SpawnBurst(target.Center, DustID.GemTopaz);
            }
        }
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
            if (thunderSet) {
                Player.immune = true;
                Player.immuneTime += 30;
            }
        }
        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
            if (thunderSet) {
                Player.immune = true;
                Player.immuneTime += 30;
            }
        }
        public static void SpawnBurst(Vector2 position, int dustID) {
            for (int i = 0; i < 10; i++) {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 8f);
                Dust d = SpawnBurstDust(position + vel * 1.5f, dustID);
                d.velocity = vel;
                d.scale *= 1.3f;
            }
        }
        public static Dust SpawnBurstDust(Vector2 position, int dustID) {
            Dust dust = Dust.NewDustPerfect(position, dustID);
            dust.noGravity = true;
            dust.scale = Main.rand.NextFloat(0.9f, 1.4f);
            dust.fadeIn = 1.2f;
            dust.alpha = 80;
            return dust;
        }
    }
}