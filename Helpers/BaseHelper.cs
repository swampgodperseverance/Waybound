using System;
using Terraria;
using Terraria.DataStructures;

namespace Waybound.Helpers
{
	public static class BaseHelper
	{
		public static PlayerScreen PlayerScreen(this Player player) => player.GetModPlayer<PlayerScreen>();
        //public static GlobalItemOther ItemOther(this Item item) => item.GetGlobalItem<GlobalItemOther>();
        //      public static GlobalItemThrowing ItemThrowing(this Item item) => item.GetGlobalItem<GlobalItemThrowing>();
        //      public static GlobalProjOther ProjOther(this Projectile projectile) => projectile.GetGlobalProjectile<GlobalProjOther>();
        public static void FighterAI(NPC npc, float moveSpeed = 0.075f, float maxSpeed = 1f, float jumpY = 10, float gravity = 0.15f, bool fleeWhenDay = true)
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            npc.velocity.Y += gravity;

            if (!Main.dayTime || !fleeWhenDay)
            {
                if (npc.velocity.X < -maxSpeed || npc.velocity.X > maxSpeed)
                {
                    npc.velocity *= 0.8f;
                }
                else if (npc.velocity.X < maxSpeed && npc.direction == 1)
                {
                    npc.velocity.X += moveSpeed;
                    if (npc.velocity.X > maxSpeed)
                        npc.velocity.X = maxSpeed;
                }
                else if (npc.velocity.X > -maxSpeed && npc.direction == -1)
                {
                    npc.velocity.X -= moveSpeed;
                    if (npc.velocity.X < -maxSpeed)
                        npc.velocity.X = -maxSpeed;
                }
            }
            WalkupHalfBricks(npc);

            if ((npc.HasTileOnSide(4, new Vector2(0, 5), false) || npc.IsOnPlatformNPC(new Vector2(0, 5))) && !npc.noTileCollide
            && ((npc.HasTileOnSide(1, new Vector2(1f, -0.1f), true) && npc.velocity.X < 0)
            || (npc.HasTileOnSide(2, new Vector2(1f, -0.1f), true) && npc.velocity.X > 0)
            || Math.Abs(player.Center.X - npc.Center.X) < 25 && player.velocity.Y == 0 && npc.Bottom.Y > player.Bottom.Y + 15 && npc.Bottom.Y - player.Bottom.Y < jumpY * 10 && Collision.CanHitLine(npc.position, npc.width, npc.height, player.position, player.width, player.height)))
            {
                npc.velocity.Y = -jumpY;
            }

            if (npc.Bottom.Y < player.Bottom.Y && npc.IsOnPlatformNPC(new Vector2(1, 1)))
            {
                npc.noTileCollide = true;
            }
            else
            {
                npc.noTileCollide = false;
            }
        }

        public static void WalkupHalfBricks(NPC npc)
        {
            WalkupHalfBricks(npc, ref npc.gfxOffY, ref npc.stepSpeed);
        }
        public static void WalkupHalfBricks(Entity codable, ref float gfxOffY, ref float stepSpeed)
        {
            if (codable == null)
                return;
            if (codable.velocity.Y >= 0f)
            {
                int offset = 0;
                if (codable.velocity.X < 0f) offset = -1;
                if (codable.velocity.X > 0f) offset = 1;
                Vector2 pos = codable.position;
                pos.X += codable.velocity.X;
                int tileX = (int)((pos.X + (double)(codable.width / 2) + (codable.width / 2 + 1) * offset) / 16.0);
                int tileY = (int)((pos.Y + (double)codable.height - 1.0) / 16.0);

                if (tileX * 16 < pos.X + (double)codable.width && tileX * 16 + 16 > (double)pos.X && (Main.tile[tileX, tileY].HasUnactuatedTile && Main.tile[tileX, tileY].Slope == 0 && Main.tile[tileX, tileY - 1].Slope == 0 && Main.tileSolid[Main.tile[tileX, tileY].TileType] && !Main.tileSolidTop[Main.tile[tileX, tileY].TileType] || Main.tile[tileX, tileY - 1].IsHalfBlock && Main.tile[tileX, tileY - 1].HasUnactuatedTile) && (!Main.tile[tileX, tileY - 1].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 1].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 1].TileType] || Main.tile[tileX, tileY - 1].IsHalfBlock && (!Main.tile[tileX, tileY - 4].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 4].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 4].TileType])) && (!Main.tile[tileX, tileY - 2].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 2].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 2].TileType]) && (!Main.tile[tileX, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX, tileY - 3].TileType] || Main.tileSolidTop[Main.tile[tileX, tileY - 3].TileType]) && (!Main.tile[tileX - offset, tileY - 3].HasUnactuatedTile || !Main.tileSolid[Main.tile[tileX - offset, tileY - 3].TileType]))
                {
                    float tileWorldY = tileY * 16;
                    if (Main.tile[tileX, tileY].IsHalfBlock)
                        tileWorldY += 8f;
                    if (Main.tile[tileX, tileY - 1].IsHalfBlock)
                        tileWorldY -= 8f;
                    if (tileWorldY < pos.Y + (double)codable.height)
                    {
                        float tileWorldYHeight = pos.Y + codable.height - tileWorldY;
                        float heightNeeded = 16.1f;
                        if (tileWorldYHeight <= (double)heightNeeded)
                        {
                            gfxOffY += codable.position.Y + codable.height - tileWorldY;
                            codable.position.Y = tileWorldY - codable.height;
                            stepSpeed = tileWorldYHeight >= 9.0 ? 2f : 1f;
                        }
                    }
                    else
                    {
                        gfxOffY = Math.Max(0f, gfxOffY - stepSpeed);
                    }
                }
                else
                {
                    gfxOffY = Math.Max(0f, gfxOffY - stepSpeed);
                }
            }
            else
            {
                gfxOffY = Math.Max(0f, gfxOffY - stepSpeed);
            }
        }
    }
    public class PlayerScreen : ModPlayer
    {
        public int rumbleDuration;
        public int rumbleStrength;
        public int interpolantTimer;
        public bool lockScreen = false;
        public bool cutscene = false;
        public bool notHurt = false;

        public override void PostUpdate()
        {
            if (rumbleDuration > 0)
            {
                rumbleDuration--;
            }
            if (lockScreen)
            {
                if (interpolantTimer < 100) interpolantTimer += 2;
            }
            else
            {
                if (interpolantTimer > 0) interpolantTimer -= 2;
            }
            ScreenFocusInterpolant = Utils.GetLerpValue(15f, 80f, interpolantTimer, true);
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (cutscene && notHurt)
                base.ModifyHurt(ref modifiers);
            // 	return false;
            // else
            //              return base.ModifyHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override bool CanUseItem(Item item)
        {
            return base.CanUseItem(item) && !cutscene;
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (cutscene && notHurt)
            {
                Player.statLife = 1;
                return false;
            }
            return base.PreKill(damage, hitDirection, pvp, ref playSound, ref genGore, ref damageSource);
        }

        public override void UpdateDead()
        {
            lockScreen = false;
            rumbleDuration = 0;
        }

        public void Rumble(int duration, int intensity = 10)
        {
            rumbleDuration = duration;
            rumbleStrength = intensity;
        }

        public float ScreenShakeIntensity;
        public float fastScreenShake;
        public Vector2 ScreenFocusPosition;
        public float ScreenFocusInterpolant;

        public override void ModifyScreenPosition()
        {
            if (ScreenFocusInterpolant > 0f)
            {
                Vector2 idealScreenPosition = ScreenFocusPosition - new Vector2(Main.screenWidth, Main.screenHeight) * 0.5f;
                Main.screenPosition = Vector2.Lerp(Main.screenPosition, idealScreenPosition, ScreenFocusInterpolant);
            }
            Waybound.Instance.CameraOffset *= 0.9f;
            Main.screenPosition += Waybound.Instance.CameraOffset;
            if (rumbleDuration > 0)
            {
                int r = rumbleStrength;
                Main.screenPosition.X += Main.rand.Next(-r, r + 1);
                Main.screenPosition.Y += Main.rand.Next(-r, r + 1);
            }
            if (ScreenShakeIntensity > 0.1f)
            {
                Main.screenPosition += new Vector2(Main.rand.NextFloat(ScreenShakeIntensity),
                    Main.rand.NextFloat(ScreenShakeIntensity));

                ScreenShakeIntensity *= 0.9f;
            }

            if (fastScreenShake > 0.1f)
            {
                Main.screenPosition += new Vector2(
                    Main.rand.NextFloat(fastScreenShake * 0.75f, fastScreenShake) * (Main.rand.NextBool(2) ? -1 : 1),
                    Main.rand.NextFloat(fastScreenShake * 0.75f, fastScreenShake) * (Main.rand.NextBool(2) ? -1 : 1));

                fastScreenShake *= 0.15f;
            }
        }
    }

}