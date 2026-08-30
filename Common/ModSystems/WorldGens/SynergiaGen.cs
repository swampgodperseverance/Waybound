//using Terraria;

//namespace Synergia.Common.ModSystems.WorldGens {
//    public class SynergiaGen : ModSystem {
//        public override void PostWorldGen() {
//            for (int y = 0; y < Main.maxTilesY; y++) {
//                for (int x = 0; x < Main.maxTilesX; x++) {
//                    if (!WorldGen.InWorld(x, y, 10)) { continue; }
//                    if (Main.tile[x, y].type == ModList.Valhalla.Find<ModTile>("DwarvenAnvil").Type) {
//                        SnowCaven.Gen(x - 1, y + 2);
//                        x += 40;
//                    }
//                }
//            }
//            WorldGen.PlaceObject(SynergiaGenVars.HellVillageX - 280 + 155, SynergiaGenVars.HellVillageY - 72, ModList.Valhalla.Find<ModTile>("DwarvenAnvil").Type, false, 0);
//        }
//    }
//}
