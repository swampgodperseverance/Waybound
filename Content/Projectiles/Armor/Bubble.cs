using System;
using Terraria;

namespace Waybound.Content.Projectiles.Armor;

public class Bubble : ModProjectile {
    int _timeLeft = -1;
    bool _kiling = false;

    public override void SetDefaults() {
        Projectile.width = 14;
        Projectile.height = 14;
        Projectile.friendly = true;
        Projectile.timeLeft = 60;
        Projectile.alpha = 255;
    }
    public override void AI() {
        if (!_kiling) {
            if (_timeLeft == -1) {
                _timeLeft = Main.rand.Next(240, 361);
                Projectile.timeLeft = _timeLeft;
            };
            if (Projectile.alpha > 0) {
                Projectile.position.Y -= 0.4f;
                Projectile.alpha -= Main.rand.Next(3, 9);
                Projectile.alpha = Math.Max(Projectile.alpha, 0);
            } else {
                Projectile.velocity += Main.rand.NextVector2Circular(0.03f, 0.03f);
                for (int i = 0; i < Main.maxProjectiles; i++) {
                    Projectile other = Main.projectile[i];
                    if (i == Projectile.whoAmI || !other.active || other.type != Projectile.type) { continue; };

                    Vector2 delta = Projectile.Center - other.Center;
                    float dist = delta.Length();
                    if (dist < 14 && dist > 0.1f) { Projectile.velocity += Vector2.Normalize(delta) * (1f - dist / 14) * 0.08f; };
                };
                if (Projectile.velocity.Length() > 1.5f) { Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 1.5f; };
                Projectile.velocity *= 0.98f;
                Projectile.position += Projectile.velocity;
            };
        };
        if (Projectile.timeLeft <= 10) {
            Projectile.alpha += Main.rand.Next(3, 9);
            Projectile.timeLeft = 10;
            _kiling = true;
        };
        if (Projectile.alpha >= 255) { Projectile.Kill(); };
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => Projectile.Kill();
    public override void OnKill(int timeLeft) {
        for (int i = 0; i < 10; i++) {
            int index = Dust.NewDust(Projectile.position, 24, 24, Terraria.ID.DustID.BubbleBurst_Blue);
            Main.dust[index].noGravity = true;
        };
    }
}