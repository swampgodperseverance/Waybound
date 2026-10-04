using Terraria.ModLoader;

namespace Waybound.Common.Biome
{
	public class CrystalDepthsBackgroundStyle : ModUndergroundBackgroundStyle
	{
		public override void FillTextureArray(int[] textureSlots)
		{
			textureSlots[0] = BackgroundTextureLoader.GetBackgroundSlot("Waybound/Assets/Textures/Backgrounds/CrystalDepthsBackground0");
			textureSlots[1] = BackgroundTextureLoader.GetBackgroundSlot("Waybound/Assets/Textures/Backgrounds/CrystalDepthsBackground1");
			textureSlots[2] = BackgroundTextureLoader.GetBackgroundSlot("Waybound/Assets/Textures/Backgrounds/CrystalDepthsBackground2");
			textureSlots[3] = BackgroundTextureLoader.GetBackgroundSlot("Waybound/Assets/Textures/Backgrounds/CrystalDepthsBackground3");
		}
	}
}