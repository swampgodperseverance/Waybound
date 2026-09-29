using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Waybound.Common.Biome;
using Waybound.Common.NetCodeUtil;
using Waybound.Common.Water;
using Waybound.Common.WUtils;
using Waybound.Particles;
namespace Waybound.Common.GlobalPlayer;

public class IceShimerPlayer : ModPlayer {
    public float Alpha { get; private set; } = 0f;
    public bool WasInWater { get; private set; } = false;

    public override void PostUpdate() {
        bool inWater = Collision.DrownCollision(Player.position, Player.width, Player.height, Player.gravDir);
        if (Player.InModBiome<CryoLake>()) { Alpha = MathHelper.Clamp(Alpha + 0.008f, 0f, 1f); }
        else {
            if (Alpha <= 0) { Alpha = 0; }
            else { Alpha = MathHelper.Clamp(Alpha - 0.02f, 0f, 1f); }
        };
        if (inWater && !WasInWater && Player.velocity.Y > 3f && Player.InModBiome<CryoLake>()) { TryTeleportToSnow(); }
        WasInWater = inWater;
    }
    void TryTeleportToSnow() {
        if (!TryFindSnowTeleport(out Vector2 destination)) { return; };
        Vector2 oldPos = Player.Center;
        if (Main.netMode == NetmodeID.SinglePlayer) { ExecuteTeleport(oldPos, destination); }
        else if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer) { MultiplayerSystem.SendPacket(new CryoPacket((byte)Player.whoAmI, destination)); };
    }
    public void ExecuteTeleport(Vector2 oldPos, Vector2 destination) {
        Player.Teleport(destination, 1, 0);
        Player.velocity = Vector2.Zero;
        if (Main.netMode != NetmodeID.Server) { SpawnTeleportEffects(oldPos, destination); }
    }
    void SpawnTeleportEffects(Vector2 oldPos, Vector2 newPos) {
        for (int i = 0; i < 30; i++) { ParticleSystem.MegasparkBuffer.Create(new ParticleInfo((oldPos + Main.rand.NextVector2Circular(24f, 24f)).ToNumerics(), (Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * 1.5f).ToNumerics(), Main.rand.NextFloat(MathHelper.TwoPi), new System.Numerics.Vector2(Main.rand.NextFloat(8f, 16f)), new Color(160, 220, 255, 255), Main.rand.Next(30, 55))); ; }
        for (int i = 0; i < 40; i++) { ParticleSystem.MegasparkBuffer.Create(new ParticleInfo((newPos + Main.rand.NextVector2Circular(40f, 40f)).ToNumerics(), (-Main.rand.NextVector2Circular(2f, 2f)).ToNumerics(), Main.rand.NextFloat(MathHelper.TwoPi), new System.Numerics.Vector2(Main.rand.NextFloat(10f, 20f)), new Color(150, 215, 255, 255), Main.rand.Next(35, 65))); };
        SoundEngine.PlaySound(SoundID.Item8, oldPos);
    }
    bool TryFindSnowTeleport(out Vector2 destination) {
        destination = Vector2.Zero;
        int attempts = 300;

        for (int i = 0; i < attempts; i++) {
            int x = Main.rand.Next(200, Main.maxTilesX - 200);
            int y = Main.rand.Next(100, Main.maxTilesY - 200);

            if (!WorldGen.InWorld(x, y, 10)) { continue; }
            Tile tile = Main.tile[x, y];
            bool isSnow = tile.TileType == TileID.SnowBlock || tile.TileType == TileID.IceBlock || tile.TileType == TileID.BreakableIce;
            if (!isSnow || !tile.HasTile) { continue; }
            int belowY = y + 1;
            if (!WorldGen.InWorld(x, belowY, 10)) { continue; }
            Tile below = Main.tile[x, belowY];
            if (!below.HasTile || !Main.tileSolid[below.TileType]) { continue; }
            Tile above1 = Main.tile[x, y - 1];
            Tile above2 = Main.tile[x, y - 2];
            if (above1.HasTile || above2.HasTile) { continue; }
            if (tile.LiquidAmount > 0 || above1.LiquidAmount > 0) { continue; }
            destination = new Vector2(x * 16f + 8f, y * 16f);
            return true;
        }

        return false;
    }
};
