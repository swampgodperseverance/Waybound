using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Collections.Generic;
using Waybound.Content.Buffs.Debuffs;
using Waybound.Common.Biome;
using Waybound.Content.Projectiles.Hostile;
using Waybound.Content.Items.Accessories.PreHardmode;
using Waybound.Content.Items.Placeable.Furniture;

namespace Waybound.Content.NPCs.CrystalGrove
{
    public class InfectedResident : ModNPC
    {
        private int frame;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 6;
            NPCID.Sets.TrailCacheLength[NPC.type] = 5;
            NPCID.Sets.TrailingMode[NPC.type] = 1;

            var drawModifier = new NPCID.Sets.NPCBestiaryDrawModifiers(0)
            {
                Position = new Vector2(0f, 2f),
                PortraitPositionXOverride = 0f,
                PortraitPositionYOverride = 0f
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, drawModifier);

            NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<DeepFire>()] = true;
        }

        public override void SetDefaults()
        {
            NPC.aiStyle = -1;
            NPC.lifeMax = 200;
            NPC.defense = 6;
            NPC.damage = 30;
            NPC.width = 38;
            NPC.height = 46;
            NPC.value = 300;
            NPC.knockBackResist = 0f;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.friendly = false;
            NPC.boss = false;
            NPC.dontTakeDamage = false;
            SpawnModBiomes = new int[1] { ModContent.GetInstance<CrystalDepthsBiome>().Type };
        }

        public override void AI()
        {
            NPC.TargetClosest(true);
            Player player = Main.player[NPC.target];

            Lighting.AddLight(NPC.Center, 0.80f, 0.51f, 0.56f);

            if (Main.rand.Next(100) < 65)
                NPC.ai[0]++;

            if (NPC.ai[0] > 180 && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height))
            {
                NPC.noTileCollide = false;
                NPC.velocity.X *= 0.95f;
                NPC.velocity.Y += 0.15f;
                if (NPC.ai[0] > 240)
                {
                    for (int i = 0; i < Main.rand.Next(5, 7); i++)
                    {
                        Vector2 direction = new Vector2(0, -1).RotatedByRandom(MathHelper.ToRadians(180)) * Main.rand.NextFloat(2.25f, 3f);
                        int type = ModContent.ProjectileType<DeepCrystalProj>();
                        int p = Projectile.NewProjectile(NPC.GetSource_GiftOrReward(), NPC.Center, direction, type, 15, 2, Main.myPlayer);
                        Main.projectile[p].friendly = false;
                        Main.projectile[p].timeLeft = Main.rand.Next(80, 100);
                        Main.projectile[p].hostile = true;
                    }
                    NPC.netUpdate = true;
                    NPC.ai[0] = 0;
                }
            }
            else
            {
                FighterBehavior(player, 0.04f, 1.6f, 10f, 0.15f);
            }
        }

        private void FighterBehavior(Player player, float moveSpeed, float maxSpeed, float jumpY, float gravity)
        {
            NPC.velocity.Y += gravity;

            if (NPC.velocity.X < -maxSpeed || NPC.velocity.X > maxSpeed)
            {
                NPC.velocity *= 0.8f;
            }
            else if (NPC.velocity.X < maxSpeed && NPC.direction == 1)
            {
                NPC.velocity.X += moveSpeed;
                if (NPC.velocity.X > maxSpeed)
                    NPC.velocity.X = maxSpeed;
            }
            else if (NPC.velocity.X > -maxSpeed && NPC.direction == -1)
            {
                NPC.velocity.X -= moveSpeed;
                if (NPC.velocity.X < -maxSpeed)
                    NPC.velocity.X = -maxSpeed;
            }

            WalkupHalfBricks();

            bool hasSide4 = HasTileOnSide(4, new Vector2(0, 5), false);
            bool onPlatform = IsOnPlatformNPC(new Vector2(0, 5));
            bool hasSide1 = HasTileOnSide(1, new Vector2(1f, -0.1f), true);
            bool hasSide2 = HasTileOnSide(2, new Vector2(1f, -0.1f), true);
            bool playerAbove = Math.Abs(player.Center.X - NPC.Center.X) < 25
                && player.velocity.Y == 0
                && NPC.Bottom.Y > player.Bottom.Y + 15
                && NPC.Bottom.Y - player.Bottom.Y < jumpY * 10
                && Collision.CanHitLine(NPC.position, NPC.width, NPC.height, player.position, player.width, player.height);

            if ((hasSide4 || onPlatform) && !NPC.noTileCollide
                && ((hasSide1 && NPC.velocity.X < 0)
                || (hasSide2 && NPC.velocity.X > 0)
                || playerAbove))
            {
                NPC.velocity.Y = -jumpY;
            }

            if (NPC.Bottom.Y < player.Bottom.Y && IsOnPlatformNPC(new Vector2(1, 1)))
                NPC.noTileCollide = true;
            else
                NPC.noTileCollide = false;
        }

        private void WalkupHalfBricks()
        {
            if (NPC.velocity.Y != 0f || NPC.noTileCollide)
                return;

            int dirX = NPC.velocity.X > 0f ? 1 : NPC.velocity.X < 0f ? -1 : 0;
            if (dirX == 0)
                return;

            int tileX = (int)((NPC.Center.X + dirX * (NPC.width * 0.5f + 8f)) / 16f);
            int tileY = (int)((NPC.Bottom.Y - 2f) / 16f);

            Tile tile = Framing.GetTileSafely(tileX, tileY);
            if (tile.HasTile && TileID.Sets.Platforms[tile.TileType])
            {
                NPC.position.Y -= 8f;
                NPC.velocity.Y = Math.Min(NPC.velocity.Y, 0f);
            }
        }

        private bool HasTileOnSide(int side, Vector2 offset, bool onlySolid)
        {
            Vector2 check = side switch
            {
                1 => new Vector2(NPC.position.X - 8f, NPC.position.Y + offset.Y),
                2 => new Vector2(NPC.position.X + NPC.width + 8f, NPC.position.Y + offset.Y),
                4 => new Vector2(NPC.position.X + NPC.width * 0.5f, NPC.position.Y + NPC.height + 8f),
                _ => NPC.Center
            };

            int tx = (int)(check.X / 16f);
            int ty = (int)(check.Y / 16f);

            if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
                return false;

            Tile tile = Framing.GetTileSafely(tx, ty);
            if (!tile.HasTile)
                return false;

            if (onlySolid && !Main.tileSolid[tile.TileType])
                return false;

            return true;
        }

        private bool IsOnPlatformNPC(Vector2 offset)
        {
            int tx1 = (int)((NPC.position.X + 4f) / 16f);
            int tx2 = (int)((NPC.position.X + NPC.width - 4f) / 16f);
            int ty = (int)((NPC.Bottom.Y + offset.Y) / 16f);

            for (int tx = tx1; tx <= tx2; tx++)
            {
                if (tx < 0 || tx >= Main.maxTilesX || ty < 0 || ty >= Main.maxTilesY)
                    continue;

                Tile tile = Framing.GetTileSafely(tx, ty);
                if (tile.HasTile && TileID.Sets.Platforms[tile.TileType])
                    return true;
            }
            return false;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = frame * frameHeight;
            NPC.spriteDirection = NPC.direction;
            NPC.frameCounter++;
            if (NPC.frameCounter > (int)(5 * (2.6 - Math.Abs(NPC.velocity.X))) && Math.Abs(NPC.velocity.X) > 0.1f)
            {
                frame++;
                if (frame >= Main.npcFrameCount[NPC.type])
                {
                    frame = 0;
                }
                NPC.frameCounter = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.InModBiome(ModContent.GetInstance<CrystalDepthsBiome>()))
            {
                return 0.1f;
            }
            else
            {
                return 0f;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Texture2D texture = ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/InfectedResident").Value;
            Texture2D textureG = ModContent.Request<Texture2D>("Waybound/Content/NPCs/CrystalGrove/InfectedResidentG").Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 pos = NPC.position - Main.screenPosition + drawOrigin + new Vector2(0, -2);
            for (int k = 0; k < NPC.oldPos.Length; k++)
            {
                Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0, -8);
                Color color = NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.65f * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                Main.EntitySpriteDraw(textureG, drawPos, NPC.frame, color, NPC.rotation, drawOrigin, NPC.scale - 0.05f, effects, 0);
            }
            Main.EntitySpriteDraw(texture, pos, NPC.frame, NPC.GetAlpha(drawColor), NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            Main.EntitySpriteDraw(textureG, pos, NPC.frame, NPC.GetAlpha(new Color(255, 168, 135, 0)) * 0.35f, NPC.rotation, drawOrigin, NPC.scale, effects, 0);
            return false;
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.OneFromOptions(33,
                ModContent.ItemType<CrystalHeartNecklace>(),
                ModContent.ItemType<DeepBoots>()));

            int deepCrystal = ModContent.ItemType<DeepCrystalShardItem>();
            var deepCrystalParameters = new DropOneByOne.Parameters()
            {
                ChanceNumerator = 1,
                ChanceDenominator = 2,
                MinimumStackPerChunkBase = 1,
                MaximumStackPerChunkBase = 3,
                MinimumItemDropsCount = 1,
                MaximumItemDropsCount = 1,
            };
            npcLoot.Add(new DropOneByOne(deepCrystal, deepCrystalParameters));
        }
    }
}