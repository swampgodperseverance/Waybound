using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Waybound.Content.NPCs.Bosses.Themis
{
	public class SickleOfThemis : ModProjectile
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sickle Of Themis");
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
		}

		public override void SetDefaults()
		{
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.aiStyle = -1;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 300;
			DrawOriginOffsetY = -6;
        }

		public override void AI()
		{
            Lighting.AddLight(Projectile.position, 1.5f, 0.75f, 0.5f);
            Projectile.rotation += MathHelper.ToRadians(7.5f);

			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] == 30)
			{
				Projectile.velocity = ((Main.player[Projectile.owner].Center - Projectile.Center).SafeNormalize(Vector2.UnitX)).RotatedByRandom(MathHelper.ToRadians(10f)) * 10;
				Projectile.netUpdate = true;
			}
			if (Projectile.ai[0] > 30)
			{
				Projectile.velocity *= 1.0225f;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++)
			{
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}
			return true;
		}
	}
}