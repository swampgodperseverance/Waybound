
using Terraria;
using Waybound.Common.ModSystems;
using Waybound.Common.ModSystems.WorldGens;
using Waybound.Helpers;

namespace Waybound.Common.Biome {
    public class SnowVillage : ModBiome {
        public override bool IsBiomeActive(Player player) {
            bool active = WorldHelper.CheckBiome(player, 103, 25, WayboundGenVars.SnowVillagePositionX, WayboundGenVars.SnowVillagePositionY - 25);
            if (active && !WayboundWorld.FirstEnterInSnowVillage) { WayboundWorld.FirstEnterInSnowVillage = true; }
            return active;
        }
    }
}