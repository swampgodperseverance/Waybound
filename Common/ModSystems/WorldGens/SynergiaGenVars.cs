// Code by SerNik
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader.IO;

namespace Waybound.Common.ModSystems.WorldGens {
    public class WayboundGenVars : ModSystem {
        public static List<Vector2> VillageTiles { get; set; } = [];
        public static List<Vector2> VillageWalles { get; set; } = [];

        public static int SnowVillagePositionX { get; set; }
        public static int SnowVillagePositionY { get; set; }
        //public static int HellArenaPositionX { get; set; }
        //public static int HellArenaPositionY { get; set; }
        //public static int HellVillageX { get; set; }
        //public static int HellVillageY { get; set; }
        //public static int HellLakeX { get; set; }
        //public static int HellLakeY { get; set; }
        //public static int HLOX { get; set; }
        //public static int HLOY { get; set; }
        //public static int HLTX { get; set; }
        //public static int HLTY { get; set; }

        public static bool SnowVillageGen { get; set; }
        //public static bool HellVillageGen { get; set; }

        public override void OnWorldLoad() {
            VillageTiles.Clear();
            VillageWalles.Clear();

            SnowVillagePositionX = 0;
            SnowVillagePositionY = 0;
            //HellArenaPositionX = 0;
            //HellArenaPositionY = 0;
            //HellVillageX = 0;
            //HellVillageY = 0;
            //HellLakeX = 0;
            //HellLakeY = 0;
            //HLOX = 0;
            //HLOY = 0;
            //HLTX = 0;
            //HLTY = 0;

            SnowVillageGen = false;
            //HellVillageGen = false;
        }
        public override void SaveWorldData(TagCompound tag) {
            tag["VillageTiles"] = VillageTiles;
            tag["VilageWalles"] = VillageWalles;

            tag["SnowVillagePositionX"] = SnowVillagePositionX;
            tag["SnowVillagePositionY"] = SnowVillagePositionY;
            //tag["HellArenaPositionX"] = HellArenaPositionX;
            //tag["HellArenaPositionY"] = HellArenaPositionY;
            //tag["hellVillageX"] = HellVillageX;
            //tag["hellVillageY"] = HellVillageY;
            //tag["HellLakeX"] = HellLakeX;
            //tag["HellLakeY"] = HellLakeY;
            //tag["HLOX"] = HLOX;
            //tag["HLOY"] = HLOY;
            //tag["HLTX"] = HLTX;
            //tag["HLTY"] = HLTY;

            tag["SnowVillageGen"] = SnowVillageGen;
            //tag["HellVillageGen"] = HellVillageGen;
        }
        public override void LoadWorldData(TagCompound tag) {
            VillageTiles = tag.Get<List<Vector2>>("VillageTiles");
            VillageWalles = tag.Get<List<Vector2>>("VilageWalles");

            SnowVillagePositionX = tag.GetInt("SnowVillagePositionX");
            SnowVillagePositionY = tag.GetInt("SnowVillagePositionY");
            //HellArenaPositionX = tag.GetInt("HellArenaPositionX");
            //HellArenaPositionY = tag.GetInt("HellArenaPositionY");
            //HellVillageX = tag.GetInt("hellVillageX");
            //HellVillageY = tag.GetInt("hellVillageY");
            //HellLakeX = tag.GetInt("HellLakeX");
            //HellLakeY = tag.GetInt("HellLakeY");
            //HLOX = tag.GetInt("HLOX");
            //HLOY = tag.GetInt("HLOY");
            //HLTX = tag.GetInt("HLTX");
            //HLTY = tag.GetInt("HLTY");

            SnowVillageGen = tag.GetBool("SnowVillageGen");
            //HellVillageGen = tag.GetBool("HellVillageGen");

            //if (ModList.Fargo != null) {
            //    Rectangle arena = new((Main.maxTilesX - 1493 + HellArenaPositionX - HellLakeX) * 16, (Main.maxTilesY - 162) * 16, (HellLakeX + 236) * 16, (HellLakeY - 119) * 16);
            //    ModList.Fargo.Call("AddIndestructibleRectangle", arena);
            
        }
        public override void NetSend(BinaryWriter writer) {
            writer.Write(VillageTiles.Count);
            foreach (Vector2 v in VillageTiles) writer.WriteVector2(v);
            writer.Write(VillageWalles.Count);
            foreach (Vector2 v in VillageWalles) writer.WriteVector2(v);

            writer.Write(SnowVillagePositionX);
            writer.Write(SnowVillagePositionY);
            //writer.Write(HellArenaPositionX);
            //writer.Write(HellArenaPositionY);
            //writer.Write(HellVillageX);
            //writer.Write(HellVillageY);
            //writer.Write(HellLakeX);
            //writer.Write(HellLakeY);
            //writer.Write(HLOX);
            //writer.Write(HLOY);
            //writer.Write(HLTX);
            //writer.Write(HLTY);

            writer.Write(SnowVillageGen);
            //writer.Write(HellVillageGen);
        }
        public override void NetReceive(BinaryReader reader) {
            int count = reader.ReadInt32();  VillageTiles.Clear();
            for (int i = 0; i < count; i++)  { VillageTiles.Add(reader.ReadVector2()); }
            int count2 = reader.ReadInt32(); VillageWalles.Clear();
            for (int i = 0; i < count2; i++) { VillageWalles.Add(reader.ReadVector2()); }

            SnowVillagePositionX = reader.ReadInt32();
            SnowVillagePositionY = reader.ReadInt32();
            //HellArenaPositionX = reader.ReadInt32();
            //HellArenaPositionY = reader.ReadInt32();
            //HellVillageX = reader.ReadInt32();
            //HellVillageY = reader.ReadInt32();
            //HellLakeX = reader.ReadInt32();
            //HellLakeY = reader.ReadInt32();
            //HLOX = reader.ReadInt32();
            //HLOY = reader.ReadInt32();
            //HLTX = reader.ReadInt32();
            //HLTY = reader.ReadInt32();

            SnowVillageGen = reader.ReadBoolean();
            //HellVillageGen = reader.ReadBoolean();
        }
    }
}