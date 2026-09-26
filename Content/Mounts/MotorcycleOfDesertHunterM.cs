using System;
using System.Linq;
using Microsoft.Xna.Framework;
using ParticleLibrary;
using ParticleLibrary.Core.V3.Particles;
using ParticleLibrary.Utilities;
using Terraria;
using Terraria.ModLoader;
using Waybound.Content.Items.Mounts;
using Waybound.Particles;

namespace Waybound.Content.Mounts
{
	public class MotorcycleOfDesertHunterM : ModMount
	{
		public override void SetStaticDefaults()
		{
			MountData.jumpHeight = 10;
			MountData.acceleration = 0.2f;
			MountData.jumpSpeed = 5f;
			MountData.blockExtraJumps = false;
			MountData.constantJump = false;
			MountData.heightBoost = 20;
			MountData.fallDamage = 0.5f;
			MountData.runSpeed = 5f;
			MountData.dashSpeed = 15f;
			MountData.flightTimeMax = 0;

			MountData.fatigueMax = 0;
			MountData.buff = ModContent.BuffType<MotorcycleOfDesertHunterB>();

			MountData.spawnDust = 6;

			MountData.totalFrames = 4;
			MountData.playerYOffsets = Enumerable.Repeat(16, MountData.totalFrames).ToArray();
			MountData.xOffset = 6;
			MountData.yOffset = 18;
			MountData.playerHeadOffset = 0;
			MountData.bodyFrame = 3;

			MountData.standingFrameCount = 0;
			MountData.standingFrameDelay = 0;
			MountData.standingFrameStart = 0;

			MountData.runningFrameCount = 3;
			MountData.runningFrameDelay = 18;
			MountData.runningFrameStart = 0;

			MountData.flyingFrameCount = 0;
			MountData.flyingFrameDelay = 0;
			MountData.flyingFrameStart = 3;

			MountData.inAirFrameCount = 0;
			MountData.inAirFrameDelay = 0;
			MountData.inAirFrameStart = 3;

			MountData.idleFrameCount = 0;
			MountData.idleFrameDelay = 0;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = false;

			MountData.swimFrameCount = 0;
			MountData.swimFrameDelay = 0;
			MountData.swimFrameStart = 3;

			if (!Main.dedServ)
			{
				MountData.textureWidth = MountData.backTexture.Width();
				MountData.textureHeight = MountData.backTexture.Height();
			}
		}

        public override void UpdateEffects(Player player)
        {
            Rectangle rect = player.getRect();

            if (Math.Abs(player.velocity.X) > 10f)
            {
                for (int i = 0; i < 6; i++)
                {
                    Vector2 pos = new Vector2(
                        rect.X + Main.rand.NextFloat(rect.Width) + player.velocity.X * 4.25f,
                        rect.Y + Main.rand.NextFloat(rect.Height)
                    );
                    Vector2 vel = Vector2.Zero;

                    ParticleManager.NewParticle<FlameParticleOld>(
                        pos,
                        vel,
                        Color.White,
                        Main.rand.NextFloat(0.2f, 0.4f),
                        1f
                    );
                    if (Main.rand.NextBool(2))
                    {
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            pos.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                            new Color(255, 160, 50, 200),
                            Main.rand.Next(16, 28)
                        ));
                    }
                }

                for (int i = 0; i < 5; i++)
                {
                    Vector2 pos = new Vector2(
                        rect.X + Main.rand.NextFloat(rect.Width) + player.velocity.X * 4.25f,
                        rect.Y + Main.rand.NextFloat(rect.Height)
                    );
                    Vector2 vel = Main.rand.NextVector2Circular(2f, 2f);

                    ParticleManager.NewParticle<FlameParticleOld>(
                        pos,
                        vel,
                        Color.White,
                        Main.rand.NextFloat(0.15f, 0.3f),
                        1f
                    );
                    if (Main.rand.NextBool(2))
                    {
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            pos.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                            new Color(255, 160, 50, 200),
                            Main.rand.Next(16, 28)
                        ));
                    }
                }

                for (int i = 0; i < 4; i++)
                {
                    Vector2 pos = new Vector2(
                        rect.X + Main.rand.NextFloat(rect.Width) + player.velocity.X,
                        rect.Y + Main.rand.NextFloat(rect.Height)
                    );
                    Vector2 vel = Vector2.Zero;

                    ParticleManager.NewParticle<FlameParticleOld>(
                        pos,
                        vel,
                        Color.White,
                        Main.rand.NextFloat(0.2f, 0.4f),
                        1f
                    );
                    if (Main.rand.NextBool(2))
                    {
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            pos.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                            new Color(255, 160, 50, 200),
                            Main.rand.Next(16, 28)
                        ));
                    }
                }
            }

            if (Math.Abs(player.velocity.Y) == 0 && Math.Abs(player.velocity.X) > 7.5f)
            {
                for (int i = 0; i < 5; i++)
                {
                    Vector2 pos = new Vector2(
                        rect.X + Main.rand.NextFloat(rect.Width) + player.velocity.X * 5f,
                        rect.Y + rect.Height - 8 - Main.rand.NextFloat(rect.Height * 0.35f)
                    );
                    Vector2 vel = Vector2.Zero / 4f;

                    ParticleManager.NewParticle<FlameParticleOld>(
                        pos,
                        vel,
                        Color.White,
                        Main.rand.NextFloat(0.3f, 0.5f),
                        1f
                    );
                    if (Main.rand.NextBool(2))
                    {
                        ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                            pos.ToNumerics(),
                            vel.ToNumerics(),
                            Main.rand.NextFloat(MathHelper.TwoPi),
                            new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                            new Color(255, 160, 50, 200),
                            Main.rand.Next(16, 28)
                        ));
                    }
                }

                if (Math.Abs(player.velocity.X) <= 10f && Main.rand.NextBool(3))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 pos = new Vector2(
                            rect.X + Main.rand.NextFloat(rect.Width) + player.velocity.X * 5,
                            rect.Y + Main.rand.NextFloat(rect.Height)
                        );
                        Vector2 vel = Vector2.Zero;

                        ParticleManager.NewParticle<FlameParticleOld>(
                            pos,
                            vel,
                            Color.White,
                            Main.rand.NextFloat(0.2f, 0.4f),
                            1f
                        );
                        if (Main.rand.NextBool(2))
                        {
                            ParticleSystem.MegasparkBuffer.Create(new ParticleInfo(
                                pos.ToNumerics(),
                                vel.ToNumerics(),
                                Main.rand.NextFloat(MathHelper.TwoPi),
                                new System.Numerics.Vector2(Main.rand.NextFloat(4f, 8f)),
                                new Color(255, 160, 50, 200),
                                Main.rand.Next(16, 28)
                            ));
                        }
                    }
                }
            }
        }
    }
}