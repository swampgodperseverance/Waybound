using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Waybound.Helpers;

namespace Waybound.Content.NPCs
{
	public abstract class WormHead : ModNPC
	{
		public float moveSpeed;
		public float inertiaMax;
		public float inertia;
		public int segmentsCount;
		public float hbSizeScale;
		public bool hasGravity;
		public int digTimerMax;
		public int aiType;
		public float idleModeMaxX;
		public float idleModeMaxY;
		public int headSize;
		public int bodySize;
		public int tailSize;
		public Texture2D textureHead;
		public Texture2D textureBody;
		public Texture2D textureTail;
		public int npcTypeHead;
		public int npcTypeBody;
		public int npcTypeTail;
		public float inertiaUp;
		public float inertiaDown;
		public float rotateCheck;
		public bool customSpawn = false;
		public int spawnRotate;
		public bool showHB = true;

		public float toRotate;
		public float toRotate2;
		public Vector2 toMove;
		public float minRotate;
		public float distance;
		public NPC toFollow;
		public int digTimer;
		public Vector2 stablePosition;
		public float rotate;
		public int gravity = 0;
		public Vector2 move;

		public int npc;

		public virtual void SetStats()
        {

        }
		public virtual void ExtraAI(Player player, NPC npc)
		{

		}
		public override void AI()
		{
			//NPC.netUpdate = true;
			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];
			ExtraAI(player, NPC);
			SetStats();
			switch (aiType)
			{
				case 0:
					if (hasGravity && !(Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16].HasTile && Main.tileSolid[Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16].TileType]))
					{
						gravity++;
						digTimer = digTimerMax - 1;
						if (gravity >= 15)
						{
							gravity = 15;
							NPC.velocity.Y = NPC.velocity.Y + 0.3f;
						}
						if (NPC.velocity.Y > moveSpeed * (1f + inertiaMax))
							NPC.velocity.Y = moveSpeed * (1f + inertiaMax);
						NPC.velocity.X *= 0.995f;

						NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);
						rotate = MathHelper.ToDegrees(NPC.rotation) - 90f;
						if (rotate > 360f)
							rotate -= 360f;
						if (rotate < 0f)
							rotate += 360f;
						if (rotate == 360f)
							rotate = 5f;
						if (rotate == 0f)
							rotate = 355f;
						inertia = inertiaMax;
						NPC.ai[1] = (float)Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y);
						NPC.ai[3] = NPC.whoAmI;
					}
					else
					{
						digTimer++;
						toRotate = MathHelper.ToDegrees((player.Center - NPC.Center).ToRotation());

						if (hasGravity && digTimer >= digTimerMax)
						{
							Collision.HitTiles(NPC.position, NPC.velocity, NPC.width, NPC.height);
							SoundEngine.PlaySound(SoundID.WormDigQuiet, NPC.Center);
							digTimer = 0;
						}
						gravity = 0;

						if (toRotate - rotateCheck <= rotate && toRotate + rotateCheck >= rotate)
							inertia = Math.Min(inertiaMax, inertia + inertiaUp);
						else
							inertia = Math.Max(0f, inertia - inertiaDown);

						AdjustMagnitude(ref NPC.velocity);

						move = player.Center - NPC.Center;
						AdjustMagnitude(ref move);
						NPC.velocity = moveSpeed * (1f + inertia) * NPC.velocity + move;
						AdjustMagnitude(ref NPC.velocity);

						NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);
						rotate = MathHelper.ToDegrees(NPC.velocity.ToRotation());
						NPC.ai[1] = (float)Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y);
						NPC.ai[3] = NPC.whoAmI;
					}
					break;
                case 1:
                    if (Math.Abs(toMove.X - NPC.Center.X) <= 10f && Math.Abs(toMove.X - NPC.Center.X) <= 10f)
                        toMove = stablePosition + new Vector2(Main.rand.NextFloat(-idleModeMaxX, idleModeMaxX), Main.rand.NextFloat(-idleModeMaxY, idleModeMaxY));

                    toRotate = MathHelper.ToDegrees((toMove - NPC.Center).SafeNormalize(Vector2.UnitX).ToRotation());

                    if (toRotate - rotateCheck <= rotate && toRotate + rotateCheck >= rotate)
                        inertia = Math.Min(inertiaMax, inertia + inertiaUp);
                    else
                        inertia = Math.Max(0, inertia - inertiaDown);

                    AdjustMagnitude(ref NPC.velocity);

                    move = toMove - NPC.Center;
                    AdjustMagnitude(ref move);
                    NPC.velocity = moveSpeed * (1f + inertia) * NPC.velocity + move;
                    AdjustMagnitude(ref NPC.velocity);

                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);
                    rotate = MathHelper.ToDegrees(NPC.velocity.ToRotation());
                    NPC.ai[1] = (float)Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y);
                    NPC.ai[3] = NPC.whoAmI;
                    break;
                case 2:
                    toRotate = MathHelper.ToDegrees((toMove - NPC.Center).SafeNormalize(Vector2.UnitX).ToRotation());

                    if (toRotate - rotateCheck <= rotate && toRotate + rotateCheck >= rotate)
                        inertia = Math.Min(inertiaMax, inertia + inertiaUp);
                    else
                        inertia = Math.Max(0, inertia - inertiaDown);

                    AdjustMagnitude(ref NPC.velocity);

                    move = toMove - NPC.Center;
                    AdjustMagnitude(ref move);
                    NPC.velocity = moveSpeed * (1f + inertia) * NPC.velocity + move;
                    AdjustMagnitude(ref NPC.velocity);

                    NPC.rotation = NPC.velocity.ToRotation() + MathHelper.ToRadians(90f);
                    rotate = MathHelper.ToDegrees(NPC.velocity.ToRotation());
                    NPC.ai[1] = (float)Math.Sqrt(NPC.velocity.X * NPC.velocity.X + NPC.velocity.Y * NPC.velocity.Y);
                    NPC.ai[3] = NPC.whoAmI;
                    break;
                default:
					break;
			}
			if (Vector2.Distance(NPC.Center, player.Center) >= 8000 || player.dead)
			{
				for (int i = 1; i <= segmentsCount; i++)
					if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeBody, true, false, null, -1, new float[] { i, -1, -1, NPC.whoAmI }))
						toFollow.active = false;
				if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeTail, true, false, null, -1, new float[] { -1, -1, -1, NPC.whoAmI }))
					toFollow.active = false;
				NPC.active = false;
			}
		}

		private void AdjustMagnitude(ref Vector2 vector)
		{
			float magnitude = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
			if (magnitude > moveSpeed * (1f + inertia))
			{
				vector *= moveSpeed * (1f + inertia) / magnitude;
			}
		}
		public void noTarget()
        {

        }
		public virtual void ExtraHit(NPC npc, int damage, float knockback, bool crit)
		{

		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
        public override bool PreKill()
		{
			NPC.netUpdate = true;
			ExtraPreKill(NPC);
			return true;
		}
		public virtual void ExtraPreKill(NPC npc)
        {

		}
		public override void OnSpawn(IEntitySource source)
		{
			NPC.netUpdate = true;
			SetStats();
			ExtraSpawn(NPC, source);
			if (!customSpawn)
				for (int i = 1; i <= segmentsCount; i++)
				{
					switch (spawnRotate)
					{
						case 0:
							npc = NPC.NewNPC(source, (int)NPC.position.X + (NPC.width / 2) - textureHead.Height - (textureBody.Height * (i - 1)), (int)NPC.position.Y + NPC.height, npcTypeBody, ai0: i, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
							Main.npc[npc].realLife = NPC.whoAmI;
							if (i == segmentsCount)
							{
								npc = NPC.NewNPC(source, (int)NPC.position.X + (NPC.width / 2) - textureHead.Height - (textureBody.Height * i), (int)NPC.position.Y + NPC.height, npcTypeTail, ai0: -1, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
								Main.npc[npc].realLife = NPC.whoAmI;
							}
							break;
						case 1:
							npc = NPC.NewNPC(source, (int)-(NPC.position.X + (NPC.width / 2) - textureHead.Height - (textureBody.Height * (i - 1))), (int)NPC.position.Y + NPC.height, npcTypeBody, ai0: i, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
							Main.npc[npc].realLife = NPC.whoAmI;
							if (i == segmentsCount)
							{
								npc = NPC.NewNPC(source, (int)-(NPC.position.X + (NPC.width / 2) - textureHead.Height - (textureBody.Height * i)), (int)NPC.position.Y + NPC.height, npcTypeTail, ai0: -1, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
								Main.npc[npc].realLife = NPC.whoAmI;
							}
							break;
						case 2:
							npc = NPC.NewNPC(source, (int)NPC.position.X + NPC.height, (int)NPC.position.Y - (NPC.height / 2) + textureHead.Height + (textureBody.Height * (i - 1)), npcTypeBody, ai0: i, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
							Main.npc[npc].realLife = NPC.whoAmI;
							if (i == segmentsCount)
							{
								npc = NPC.NewNPC(source, (int)NPC.position.X + NPC.height, (int)NPC.position.Y - (NPC.height / 2) + textureHead.Height + (textureBody.Height * i), npcTypeTail, ai0: -1, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
								Main.npc[npc].realLife = NPC.whoAmI;
							}
							break;
						case 3:
							npc = NPC.NewNPC(source, (int)NPC.position.X + NPC.height, (int)-(NPC.position.Y - (NPC.height / 2) + textureHead.Height + (textureBody.Height * (i - 1))), npcTypeBody, ai0: i, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
							Main.npc[npc].realLife = NPC.whoAmI;
							if (i == segmentsCount)
							{
								npc = NPC.NewNPC(source, (int)NPC.position.X + NPC.height, (int)-(NPC.position.Y - (NPC.height / 2) + textureHead.Height + (textureBody.Height * i)), npcTypeTail, ai0: -1, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
								Main.npc[npc].realLife = NPC.whoAmI;
							}
							break;
						case 4:
							npc = NPC.NewNPC(source, (int)NPC.position.X, (int)NPC.position.Y, npcTypeBody, ai0: i, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
							Main.npc[npc].realLife = NPC.whoAmI;
							if (i == segmentsCount)
							{
								npc = NPC.NewNPC(source, (int)NPC.position.X, (int)NPC.position.Y, npcTypeTail, ai0: -1, ai1: -1, ai2: -1, ai3: NPC.whoAmI);
								Main.npc[npc].realLife = NPC.whoAmI;
							}
							break;
					}
				}
			stablePosition = NPC.Center;
			toMove = stablePosition + new Vector2(Main.rand.NextFloat(-idleModeMaxX, idleModeMaxX), Main.rand.NextFloat(-idleModeMaxY, idleModeMaxY));
			NPC.width = headSize;
			NPC.height = headSize;
		}
		public virtual void ExtraSpawn(NPC npc, IEntitySource source)
        {

        }
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			NPC.netUpdate = true;
			if (showHB)
			{
				scale = hbSizeScale;
				if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeTail, true, false, null, -1, new float[] { -1, -1, -1, NPC.ai[3] }))
				{
					position = new Vector2((NPC.Center.X + toFollow.Center.X) / 2, (NPC.Center.Y + toFollow.Center.Y) / 2);
				}
				return true;
			}
			else
				return false;
		}
	}
	public abstract class WormBody : ModNPC
	{
		public int segmentsCount;
		public int headSize;
		public int bodySize;
		public int npcTypeHead;
		public int npcTypeBody;
		public Texture2D textureBody;

		public NPC toFollow;

		public virtual void SetStats()
		{

		}
		public virtual void ExtraAI(Player player, NPC npc)
		{

		}
		public override void AI()
		{
			NPC.netUpdate = true;
			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];
			ExtraAI(player, NPC);
			SetStats();
			if (NPC.ai[0] == 1)
			{
				if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeHead, true, false, null, -1, new float[] { -1, -1, -1, NPC.ai[3] }))
				{
					Texture2D texture = TextureAssets.Npc[NPC.type].Value;

					float dirX = toFollow.Center.X - NPC.Center.X;
					float dirY = toFollow.Center.Y - NPC.Center.Y;

					Vector2 rot = toFollow.Center + new Vector2(0f, NPC.height / 4).RotatedBy(toFollow.rotation) - NPC.Center;

					NPC.rotation = rot.ToRotation() + MathHelper.ToRadians(90f);

					float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

					float dist = (length - NPC.height) / length;
					float posX = dirX * dist;
					float posY = dirY * dist;

					NPC.velocity = Vector2.Zero;

					NPC.position.X += posX;
					NPC.position.Y += posY;

					NPC.ai[1] = toFollow.ai[1];
				}
				if (!WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeHead, true, false, null, -1, new float[] { -1, -1, -1, NPC.ai[3] }))
					KillSegment(NPC);
			}
			else if (NPC.ai[0] <= segmentsCount)
			{
				if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeBody, true, false, null, -1, new float[] { NPC.ai[0] - 1, -1, -1, NPC.ai[3] }))
				{
					Texture2D texture = TextureAssets.Npc[NPC.type].Value;

					float dirX = toFollow.Center.X - NPC.Center.X;
					float dirY = toFollow.Center.Y - NPC.Center.Y;

					Vector2 rot = toFollow.Center - NPC.Center;

					NPC.rotation = rot.ToRotation() + MathHelper.ToRadians(90f);

					float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

					float dist = (length - NPC.height) / length;
					float posX = dirX * dist;
					float posY = dirY * dist;

					NPC.velocity = Vector2.Zero;

					NPC.position.X += posX;
					NPC.position.Y += posY;

					NPC.ai[1] = toFollow.ai[1];
				}
				if (!WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeBody, true, false, null, -1, new float[] { NPC.ai[0] - 1, -1, -1, NPC.ai[3] }))
					KillSegment(NPC);
			}
		}
		public virtual void ExtraHit(NPC npc, int damage, float knockback, bool crit)
		{

		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
		public override bool PreKill()
		{
			NPC.netUpdate = true;
			return false;
		}
        public override bool CheckActive()
        {
			return false;
        }
        public virtual void KillSegment(NPC npc)
		{
			NPC.netUpdate = true;
			ExtraPreKill(npc);
			npc.active = false;
		}
		public virtual void ExtraPreKill(NPC npc)
		{

		}
		public override void OnSpawn(IEntitySource source)
		{
			NPC.netUpdate = true;
			SetStats();
			ExtraSpawn(NPC, source);
			NPC.width = bodySize;
			NPC.height = bodySize;
		}
		public virtual void ExtraSpawn(NPC npc, IEntitySource source)
		{

		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return false;
		}
	}
	public abstract class WormTail : ModNPC
	{
		public int segmentsCount;
		public int tailSize;
		public int npcTypeHead;
		public int npcTypeBody;
		public int npcTypeTail;
		public Texture2D textureTail;

		public NPC toFollow;

		public virtual void SetStats()
		{

		}
		public virtual void ExtraAI(Player player, NPC npc)
		{

		}
		public override void AI()
		{
			NPC.netUpdate = true;
			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];
			ExtraAI(player, NPC);
			SetStats();
			if (WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeBody, true, false, null, -1, new float[] { segmentsCount, -1, -1, NPC.ai[3] }))
			{
				Texture2D texture = TextureAssets.Npc[NPC.type].Value;
				float dirX = toFollow.Center.X - NPC.Center.X;
				float dirY = toFollow.Center.Y - NPC.Center.Y;

				Vector2 rot = toFollow.Center + new Vector2(0f, NPC.height / 2).RotatedBy(toFollow.rotation) - NPC.Center;

				NPC.rotation = rot.ToRotation() + MathHelper.ToRadians(90f);

				float length = (float)Math.Sqrt(dirX * dirX + dirY * dirY);

				float dist = (length - NPC.height - (texture.Height - NPC.height) / 2) / length;
				float posX = dirX * dist;
				float posY = dirY * dist;

				NPC.velocity = Vector2.Zero;

				NPC.position.X += posX;
				NPC.position.Y += posY;
				NPC.ai[1] = toFollow.ai[1];
			}
			if (!WayboundHelper.ClosestNPC(ref toFollow, NPC.Center, 0, npcTypeBody, true, false, null, -1, new float[] { segmentsCount, -1, -1, NPC.ai[3] }))
				KillSegment(NPC);
		}
		public virtual void ExtraHit(NPC npc, int damage, float knockback, bool crit)
		{

		}
		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			NPC.netUpdate = true;
			ExtraHit(NPC, hit.Damage, hit.Knockback, hit.Crit);
		}
		public override bool PreKill()
		{
			NPC.netUpdate = true;
			return false;
		}
		public override bool CheckActive()
		{
			return false;
		}
		public virtual void KillSegment(NPC npc)
		{
			NPC.netUpdate = true;
			ExtraPreKill(npc);
			npc.active = false;
		}
		public virtual void ExtraPreKill(NPC npc)
		{

		}
		public override void OnSpawn(IEntitySource source)
		{
			NPC.netUpdate = true;
			SetStats();
			ExtraSpawn(NPC, source);
			NPC.width = tailSize;
			NPC.height = tailSize;
		}
		public virtual void ExtraSpawn(NPC npc, IEntitySource source)
		{

		}
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)
		{
			return false;
		}
	}
}