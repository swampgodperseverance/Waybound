using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Waybound.Common.Systems
{
    public class DownedBossSystem : ModSystem
    {
        public static bool DownedDeepStoneGolem = false;
        public static bool DownedThemis = false;
        public static bool DownedDeepLunatic = false;
        public static bool DownedForgottenSoul = false;
        public static bool DownedLeviathan = false;
        public static bool DownedGlowingOracle = false;

        public override void OnWorldLoad()
        {
            DownedDeepStoneGolem = false;
            DownedThemis = false;
            DownedDeepLunatic = false;
            DownedForgottenSoul = false;
            DownedLeviathan = false;
            DownedGlowingOracle = false;
        }

        public override void OnWorldUnload()
        {
            DownedDeepStoneGolem = false;
            DownedThemis = false;
            DownedDeepLunatic = false;
            DownedForgottenSoul = false;
            DownedLeviathan = false;
            DownedGlowingOracle = false;
        }

        public override void SaveWorldData(TagCompound tag)
        {
            if (DownedDeepStoneGolem)
            {
                tag["DownedDeepStoneGolem"] = true;
            }
            if (DownedThemis)
            {
                tag["DownedThemis"] = true;
            }
            if (DownedDeepLunatic)
            {
                tag["DownedDeepLunatic"] = true;
            }
            if (DownedForgottenSoul)
            {
                tag["DownedForgottenSoul"] = true;
            }
            if (DownedLeviathan)
            {
                tag["DownedLeviathan"] = true;
            }
            if (DownedGlowingOracle)
            {
                tag["DownedGlowingOracle"] = true;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            DownedDeepStoneGolem = tag.ContainsKey("DownedDeepStoneGolem");
            DownedThemis = tag.ContainsKey("DownedThemis");
            DownedDeepLunatic = tag.ContainsKey("DownedDeepLunatic");
            DownedForgottenSoul = tag.ContainsKey("DownedForgottenSoul");
            DownedLeviathan = tag.ContainsKey("DownedLeviathan");
            DownedGlowingOracle = tag.ContainsKey("DownedGlowingOracle");
        }

        public override void NetSend(BinaryWriter writer)
        {
            var flags = new BitsByte();
            flags[0] = DownedDeepStoneGolem;
            flags[1] = DownedThemis;
            flags[2] = DownedDeepLunatic ;
            flags[3] = DownedForgottenSoul;
            flags[4] = DownedLeviathan;
            flags[5] = DownedGlowingOracle;
            writer.Write(flags);
        }

        public override void NetReceive(BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            DownedDeepStoneGolem = flags[0];
            DownedThemis = flags[1];
            DownedDeepLunatic = flags[2];
            DownedForgottenSoul = flags[3];
            DownedLeviathan = flags[4];
            DownedGlowingOracle = flags[5];
        }
    }
}
