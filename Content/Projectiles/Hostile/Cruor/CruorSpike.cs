using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.Projectiles.Hostile.Cruor
{
	public class CruorSpike : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
			if(ModLoader.TryGetMod("Redemption", out Mod mor)) mor.Call("addElementProj", 12, base.Projectile.type);
		}
		public override void SetDefaults() {
			if(ModLoader.TryGetMod("CalamityMod", out Mod calamity)) calamity.Call("SetDefenseDamageProjectile", Projectile, true);
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
		}
		public override void AI() {
			if(Projectile.timeLeft > 290) Projectile.velocity *= 0.95f;
			else if(Projectile.timeLeft > 240) Projectile.velocity /= 0.95f;
			Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
		}
		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			lightColor = new Color(200, 200, 200, 0);
			for(int k = 0; k < Projectile.oldPos.Length; k++) Main.EntitySpriteDraw(texture, Projectile.oldPos[k] + Projectile.Size * 0.5f - Main.screenPosition, null, lightColor * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length), Projectile.oldRot[k], texture.Size() / 2, (Projectile.scale - k * 0.05f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}