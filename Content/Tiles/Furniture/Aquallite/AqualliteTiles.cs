using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;
using Waybound.Content.Items.Placeable.Furniture.Aquallite;

namespace Waybound.Content.Tiles.Furniture.Aquallite;

public class AqualliteChest : Abstract.Chest {
    public override Color MapColor => new(80, 160, 220);
    public override int Dust => DustID.BlueCrystalShard;
    public override int ChestID => ItemType<AqualliteChestItem>();

    public class AquallitePlatformTile : ModTile {
        public override bool CanExplode(int i, int j) {
            return false;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch) {

        }
        public override void SetStaticDefaults() {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileTable[Type] = true;
            TileObjectData.newTile.CoordinateHeights = new[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.StyleMultiplier = 27;
            TileObjectData.newTile.StyleWrapLimit = 27;
            TileObjectData.newTile.UsesCustomCanPlace = false;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsDoor);

            DustType = DustID.BlueCrystalShard;

            AddMapEntry(new Color(80, 160, 220));
            //ItemDrop/* tModPorter Note: Removed. Tiles and walls will drop the item which places them automatically. Use RegisterItemDrop to alter the automatic drop if necessary. */ = ModContent.ItemType<TidalPlatingPlatform>();
            AdjTiles = new int[] { TileID.Platforms };
            TileID.Sets.Platforms[Type] = true;
        }
        public override void PostSetDefaults() {
            Main.tileNoSunLight[Type] = false;
        }
        public override void NumDust(int i, int j, bool fail, ref int num) {
            num = fail ? 1 : 3;
        }
    }

    public class AqualliteLampTile : Lamp {
        public override void SetLampDefaults() {
            DustType = DustID.BlueCrystalShard;
            AddMapEntry(new Color(60, 140, 220));
        }

        public override Vector3 GetLightColor() => new Vector3(0.28f, 0.68f, 1.25f);

        public override int GetItemDropType() => ModContent.ItemType<AqualliteLamp>();

        public override Asset<Texture2D> GlowTexture => ModContent.Request<Texture2D>("Waybound/Content/Tiles/Furniture/Aquallite/AqualliteLampTileGlow");
    }
}