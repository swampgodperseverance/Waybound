using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ModLoader;
using Waybound.Content.NPCs.Bosses.Themis;

namespace Waybound.Content.BossBars
{
	public class ThemisBossBar : ModBossBar
	{
		private int bossHeadIndex = -1;

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame)
		{
			if (bossHeadIndex != -1)
			{
				return TextureAssets.NpcHeadBoss[bossHeadIndex];
			}

			return null;
		}

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float life, ref float lifeMax, ref float shield, ref float shieldMax)/* tModPorter Note: life and shield current and max values are now separate to allow for hp/shield number text draw */
		{
			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active || npc.type != ModContent.NPCType<NPCs.Bosses.Themis.Themis>())
				return false;

			bossHeadIndex = npc.GetBossHeadTextureIndex();
			life = npc.life;
			lifeMax = npc.lifeMax;
			// lifePercent = Utils.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);

			return true;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams)
		{
			drawParams.BarCenter.Y -= 20f;

			return true;
		}
	}
}