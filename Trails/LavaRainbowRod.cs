using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;

namespace Waybound.Trails
{
	public struct LavaRainbowRod
	{
		public void Draw(Projectile proj)
		{
			MiscShaderData shader = GameShaders.Misc["RainbowRod"];

			shader.UseSaturation(1.6f);   
			shader.UseOpacity(3.2f);   
			shader.Apply(null);

			_vertexStrip.PrepareStripWithProceduralPadding(
				proj.oldPos,
				proj.oldRot,
				StripColors,
				StripWidth,
				-Main.screenPosition + proj.Size / 2f,
				false,
				true
			);

			_vertexStrip.DrawTrail();

			Main.pixelShader.CurrentTechnique.Passes[0].Apply();
		}

		private Color StripColors(float progressOnStrip)
		{
			Color color = Color.Lerp(Color.Yellow, Color.OrangeRed, progressOnStrip);
			color = Color.Lerp(color, Color.DarkRed, progressOnStrip * progressOnStrip);
			color.A = 0; 
			return color;
		}

		private float StripWidth(float progressOnStrip)
		{
			float width = Utils.GetLerpValue(0f, 0.25f, progressOnStrip, true);
			width = (float)Math.Sin(width * MathHelper.Pi);
			return MathHelper.Lerp(2f, 9.5f, width);
		}


		private static VertexStrip _vertexStrip = new VertexStrip();
	}
}
