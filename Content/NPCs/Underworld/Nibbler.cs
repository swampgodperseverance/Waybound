using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace Waybound.Content.NPCs.Underworld
{
	public class Nibbler : ModNPC
	{
		//this prevents multiple nibblers from attacking at the same time so the swarming doesn't feel unfair
		private static int lastAttackingNibbler = -1;
		//Just change these
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 6;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(base.Type, value);
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
		}
		//and the stats here
		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 34;
			NPC.damage = 40;
			NPC.defense = 10;
			NPC.lifeMax = 400;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.netAlways = true;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 500;
		}
		//and the bestiary shit here
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
			new FlavorTextBestiaryInfoElement("Flies around the bridge and trying to escape its own nightmares.")
		});
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			if(NPC.frame.Y != frameHeight || NPC.ai[0] < 60f) if(++NPC.frameCounter >= (NPC.ai[0] > 60f ? 2 : NPC.ai[0] > 0f ? 4 : 5 - (int)NPC.velocity.Length() / 4)) {
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				NPC.frame.Y %= frameHeight * (Main.npcFrameCount[NPC.type] - 1);
			}
			NPC.rotation = NPC.velocity.Y / 45f * NPC.spriteDirection;
		}
		public override void AI() {
			Player target = NPC.target > -1 ? Main.player[NPC.target] : null;
			if(target == null || !target.active || target.dead) {
				NPC.TargetClosest();
				target = NPC.target > -1 ? Main.player[NPC.target] : null;
				if (target == null || !target.active || target.dead) {
					if (NPC.timeLeft > 60) NPC.timeLeft = 60;
					return; 
				}
			}
			if(target != null) if(NPC.ai[0] > 0f && (lastAttackingNibbler == NPC.whoAmI || lastAttackingNibbler == -1)) {
				if(++NPC.ai[0] == 60f) {
					NPC.velocity = Vector2.UnitX * NPC.direction * 16f;
					SoundEngine.PlaySound(SoundID.Item46 with {MaxInstances = 8}, NPC.Center);
				}
				else if(NPC.ai[0] < 60f) {
					if(NPC.ai[0] < 3f) lastAttackingNibbler = NPC.whoAmI;
					if(Main.expertMode) NPC.ai[0]++;
					NPC.velocity += (target.Center - Vector2.UnitX * NPC.direction * 160f - NPC.Center).SafeNormalize(Vector2.Zero) * 0.08f * Vector2.UnitX;
					NPC.velocity *= 0.96f;
				}
				else if(Math.Sign(NPC.velocity.X) != Math.Sign(target.Center.X - NPC.Center.X) || NPC.velocity.X == 0f) {
					lastAttackingNibbler = -1;
					NPC.ai[0] = -Main.rand.Next(120, 181);
					NPC.ai[1] = Main.rand.Next(-16, 33);
					NPC.netUpdate = true;
					NPC.TargetClosest();
				}
			}
			else {
				if(++NPC.ai[3] >= 240f) NPC.ai[3] = 0f;
				NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
				Vector2 hoverPos = target.Center;
				hoverPos.X -= NPC.direction * (NPC.ai[1] + 160f);
				hoverPos.Y -= 64f + (float)Math.Sin(NPC.ai[3] / 120f * MathHelper.Pi) * 32f;
				NPC.velocity += (hoverPos - NPC.Center).SafeNormalize(Vector2.Zero) * (Math.Sign(NPC.velocity.X) == NPC.direction ? 0.24f : 0.14f);
				NPC.velocity *= 0.96f;
				if(NPC.ai[0] != 0f) NPC.ai[0]++;
				else if(lastAttackingNibbler == -1 && Main.rand.NextBool(Main.getGoodWorld ? 2 : Main.expertMode ? 15 : 25) && NPC.Bottom.Y >= target.position.Y && NPC.position.Y <= target.Bottom.Y && Main.netMode != 1) {
					lastAttackingNibbler = NPC.whoAmI;
					NPC.ai[0]++;
					NPC.netUpdate = true;
				}
				if(NPC.ai[0] == 1f) if(lastAttackingNibbler != -1 && lastAttackingNibbler != NPC.whoAmI) NPC.ai[0]--;
				else SoundEngine.PlaySound(NPC.direction > 0 ? SoundID.Zombie22 : SoundID.Zombie23, NPC.Center);
				
			}
		}
		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			if(NPC.ai[0] >= 120f) modifiers.Knockback *= 0f;
		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 8; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 2, -2f, 80, default, 1f);
			if (NPC.life <= 0) {
				for (int i = 0; i < 30; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, hit.HitDirection * 3, -3f, 100, default, 1.5f);
				SoundEngine.PlaySound(SoundID.NPCDeath1, NPC.Center);
			}
		}
		public override void OnKill() {
			lastAttackingNibbler = -1;
			for (int i = 0; i < 40; i++) Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, Main.rand.Next(-3, 4), Main.rand.Next(-3, 4), 100, default, 1.8f);
		}
		public override bool PreDraw(SpriteBatch sprite, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 origin = NPC.frame.Size() / 2f;
			if(NPC.ai[0] > 60f) for(int i = 0; i < MathHelper.Min(4f, NPC.ai[0] - 60f); i++) sprite.Draw(texture, NPC.Center - NPC.velocity * i - screenPos, NPC.frame, Color.Red with {A = 0} * NPC.Opacity * (1f - i * 0.25f), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			else if(NPC.ai[0] > 0f && NPC.ai[0] < 60f) for(int i = 0; i < 4; i++) sprite.Draw(texture, NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + NPC.ai[0] / 60f * MathHelper.TwoPi * NPC.spriteDirection) * (float)Math.Sin(MathHelper.Pi * NPC.ai[0] / 60f) * 6f - screenPos, NPC.frame, Color.Red with {A = 0} * 0.7f * NPC.Opacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			sprite.Draw(texture, NPC.Center - screenPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(NPC.GetAlpha(drawColor)), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			return false;
		}
		public override void SendExtraAI(BinaryWriter writer) => writer.Write(lastAttackingNibbler);
		public override void ReceiveExtraAI(BinaryReader reader) => lastAttackingNibbler = reader.ReadInt32();
		public override bool? CanFallThroughPlatforms() => NPC.target > -1 && NPC.Center.Y < Main.player[NPC.target].Center.Y ? true : null;
	}
}
