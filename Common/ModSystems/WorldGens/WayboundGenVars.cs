// Code by SerNik
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader.IO;

namespace Waybound.Common.ModSystems.WorldGens {
    public class WayboundGenVars : ModSystem {
        public static List<Vector2> VillageTiles { get; set; } = [];
        public static List<Vector2> VillageWalles { get; set; } = [];
        public static List<Vector2> DesertPillar1Tiles { get; set; } = [];
        public static List<Vector2> DesertPillar1Walles { get; set; } = [];
        public static List<GenVector> CryoSpringPos { get; set; } = [];
        public static int SnowVillagePositionX { get; set; }
        public static int SnowVillagePositionY { get; set; }
        public static int DesertPillar1PositionX { get; set; }
        public static int DesertPillar1PositionY { get; set; }

        public static bool SnowVillageGen { get; set; }
        public static bool DesertPillar1Gen { get; set; }
        public override void ClearWorld() {
            CryoSpringPos.Clear();
        }
        public override void OnWorldLoad() {
            VillageTiles.Clear();
            VillageWalles.Clear();
            DesertPillar1Tiles.Clear();
            DesertPillar1Walles.Clear();
            CryoSpringPos.Clear();

            SnowVillagePositionX = 0;
            SnowVillagePositionY = 0;
            DesertPillar1PositionX = 0;
            DesertPillar1PositionY = 0;

            SnowVillageGen = false;
            SnowVillageGen = false;
        }
        public override void SaveWorldData(TagCompound tag) {
            tag["VillageTiles"] = VillageTiles;
            tag["VilageWalles"] = VillageWalles;
            tag["DesertPillar1Tiles"] = DesertPillar1Tiles;
            tag["DesertPillar1Walles"] = DesertPillar1Walles;
            tag[$"{Waybound.ModName}: Cryo Spring Pos"] = CryoSpringPos.Select(vector => {
                return new TagCompound() { ["SrartPos"] = vector.Start, ["EndPos"] = vector.End };
            }).ToList();

            tag["SnowVillagePositionX"] = SnowVillagePositionX;
            tag["SnowVillagePositionY"] = SnowVillagePositionY;
            tag["DesertPillar1PositionX"] = DesertPillar1PositionX;
            tag["DesertPillar1PositionY"] = DesertPillar1PositionY;

            tag["SnowVillageGen"] = SnowVillageGen;
            tag["DesertPillar1Gen"] = DesertPillar1Gen;
        }
        public override void LoadWorldData(TagCompound tag) {
            VillageTiles = tag.Get<List<Vector2>>("VillageTiles");
            VillageWalles = tag.Get<List<Vector2>>("VilageWalles");
            DesertPillar1Tiles = tag.Get<List<Vector2>>("DesertPillar1Tiles");
            DesertPillar1Walles = tag.Get<List<Vector2>>("DesertPillar1Walles");
            if (tag.ContainsKey($"{Waybound.ModName}: Cryo Spring Pos")) {
                CryoSpringPos = [.. tag.GetList<TagCompound>($"{Waybound.ModName}: Cryo Spring Pos").Select(tag => {
                    return new GenVector(tag.Get<Vector2>("SrartPos"), tag.Get<Vector2>("EndPos"));
                })];
            }
            else { CryoSpringPos = []; }

            SnowVillagePositionX = tag.GetInt("SnowVillagePositionX");
            SnowVillagePositionY = tag.GetInt("SnowVillagePositionY");

            DesertPillar1PositionX = tag.GetInt("DesertPillar1PositionX");
            DesertPillar1PositionY = tag.GetInt("DesertPillar1PositionY");

            SnowVillageGen = tag.GetBool("SnowVillageGen");
            DesertPillar1Gen = tag.GetBool("DesertPillar1Gen");
        }
        public override void NetSend(BinaryWriter writer) {
            SendVector(ref writer, VillageTiles);
            SendVector(ref writer, VillageWalles);
            SendVector(ref writer, DesertPillar1Tiles);
            SendVector(ref writer, DesertPillar1Walles);
            SendVector(ref writer, CryoSpringPos);

            writer.Write(SnowVillagePositionX);
            writer.Write(SnowVillagePositionY);

            writer.Write(DesertPillar1PositionX);
            writer.Write(DesertPillar1PositionY);

            writer.Write(SnowVillageGen);
            writer.Write(DesertPillar1Gen);
        }
        public override void NetReceive(BinaryReader reader) {
            ReceiveVector(ref reader, VillageTiles);
            ReceiveVector(ref reader, VillageWalles);
            ReceiveVector(ref reader, DesertPillar1Tiles);
            ReceiveVector(ref reader, DesertPillar1Walles);
            ReceiveVector(ref reader, CryoSpringPos);

            SnowVillagePositionX = reader.ReadInt32();
            SnowVillagePositionY = reader.ReadInt32();

            DesertPillar1PositionX = reader.ReadInt32();
            DesertPillar1PositionY = reader.ReadInt32();

            SnowVillageGen = reader.ReadBoolean();
            DesertPillar1Gen = reader.ReadBoolean();
        }
        static void SendVector(ref BinaryWriter writer, List<Vector2> vector) {
            writer.Write(vector.Count);
            foreach (Vector2 v in vector) writer.WriteVector2(v);
        }
        static void ReceiveVector(ref BinaryReader reader, List<Vector2> vector) {
            int count = reader.ReadInt32(); vector.Clear();
            for (int i = 0; i < count; i++) { vector.Add(reader.ReadVector2()); }
        }
        static void SendVector(ref BinaryWriter writer, List<GenVector> vector) {
            writer.Write(vector.Count);
            foreach (GenVector v in vector) {
                writer.WriteVector2(v.Start);
                writer.WriteVector2(v.End);
            }
        }
        static void ReceiveVector(ref BinaryReader reader, List<GenVector> vector) {
            int count = reader.ReadInt32(); vector.Clear();
            for (int i = 0; i < count; i++) {
                Vector2 start = reader.ReadVector2();
                Vector2 end = reader.ReadVector2();
                vector.Add(new(start, end));
            }
        }
    }
    public struct GenVector(float x1, float x2, float y1, float y2) {
        public Vector2 Start { get; private set; } = new(x1, y1);
        public Vector2 End { get; private set; } = new(x2, y2);
        public GenVector(Vector2 start, Vector2 end) : this (start.X, start.Y, end.X, end.Y) {
            Start = start;
            End = end;
        }
        public override readonly string ToString() => "Start = " + Start.ToString() + "\n" + "End = " + End.ToString();
        public static GenVector operator *(GenVector v1, float mult) => new(v1.Start * mult, v1.End * mult);
    }
}