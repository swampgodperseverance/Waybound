using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Dusts.DeepDusts;
using Waybound.Content.Tiles.Blocks;

namespace Waybound.Common.Biome
{
	public class CrystalDepthsBiome : ModBiome
	{
		public override ModUndergroundBackgroundStyle UndergroundBackgroundStyle => ModContent.Find<ModUndergroundBackgroundStyle>("Waybound/CrystalDepthsBackgroundStyle");

		public override int Music => MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrystalDepths");

		public override SceneEffectPriority Priority => SceneEffectPriority.BiomeHigh;

		public override string BackgroundPath => "Waybound/Assets/Textures/Bestiary/CrystalDepthsBestiary_Background";
		public override string BestiaryIcon => "Waybound/Assets/Textures/Bestiary/CrystalDepthsBestiary";
		public override string MapBackground => "Waybound/Assets/Textures/Bestiary/CrystalDepthsBackgroundMap";
		public override Color? BackgroundColor => base.BackgroundColor;

		public override bool IsBiomeActive(Player player)
		{
			return (player.ZoneDirtLayerHeight || player.ZoneRockLayerHeight) && (ModContent.GetInstance<crystalDepthsTileCount>().deepDirtCount >= 100);
		}

		public override void OnInBiome(Player player)
		{
			player.AddBuff(22, 2);

			if (Main.rand.NextBool(10))
			{
				Vector2 pos = new(Main.rand.Next(-250, Main.screenWidth + 250) + (int)Main.screenPosition.X,
                    Main.rand.Next(-100, Main.screenHeight + 100) + (int)Main.screenPosition.Y);
                if (Main.LocalPlayer.velocity.Y > 0.0)
                    pos.Y -= (int)Main.player[Main.myPlayer].velocity.Y;
				while (Vector2.Distance(player.position, pos) < 500)
					pos += (pos - player.position).SafeNormalize(Vector2.UnitX) * 100;
                Dust dust = Main.dust[Dust.NewDust(pos, 10, 10, ModContent.DustType<DeepGlowDustScreen>(), Main.rand.NextFloat(-0.25f, 0.25f),Main.rand.NextFloat(-0.25f, 0.25f), 255, new Color(), Main.rand.NextFloat(0.6f, 1.2f))];
            }
		}
	}

	public class crystalDepthsTileCount : ModSystem
	{
		public int deepDirtCount;

		public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
		{
			deepDirtCount = tileCounts[ModContent.TileType<DeepDirt>()];
		}
	}
}