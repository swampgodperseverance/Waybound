using Waybound.Content.Projectiles.Hostile;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Waybound.Content.NPCs.Ice
{
	public class IcePrincess : ModNPC
	{
		//Just change these
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 9;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(base.Type, value);
		}
		//and the stats here
		public override void SetDefaults() {
			NPC.width = 32;
			NPC.height = 40;
			NPC.damage = 0; //no contact damage because that would be annoying
			NPC.defense = 10;
			NPC.lifeMax = 400;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.netAlways = true;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit5;
			NPC.DeathSound = SoundID.NPCDeath7;
			NPC.value = Item.sellPrice(0, 0, 9, 9);
		}
		//and the bestiary shit here
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
			new FlavorTextBestiaryInfoElement("Put description here")
		});
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			if(++NPC.frameCounter >= (NPC.ai[0] > -120f && NPC.ai[0] < 0f ? 4 : 8)) {
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				NPC.frame.Y %= frameHeight * Main.npcFrameCount[NPC.type] / 3;
				if(NPC.ai[0] < -120f) NPC.frame.Y += frameHeight * 3;
				else if(NPC.ai[0] < 0f) NPC.frame.Y += frameHeight * 6;
			}
			NPC.rotation = NPC.velocity.X / 45f;
		}
		public override void AI() {
			Lighting.AddLight(NPC.Center, (Color.Aquamarine * 0.3f).ToVector3());
			Player target = NPC.target > -1 ? Main.player[NPC.target] : null;
			if(target == null || !target.active || target.dead || target.Distance(NPC.Center) > 1000f){
				NPC.TargetClosest();
				target = NPC.target > -1 ? Main.player[NPC.target] : null;
				if (target == null || !target.active || target.dead || target.Distance(NPC.Center) > 1000f) {
					if (NPC.timeLeft > 60) NPC.timeLeft = 60;
					NPC.velocity.Y += 0.1f;
					return; 
				}
			}
			if(target != null) if(NPC.ai[0] < 0f) {
				if(NPC.ai[0] > -120f && NPC.ai[0] % 10 == 0) {
					List<Projectile> existingIcicles = new();
					foreach(Projectile projectile in Main.ActiveProjectiles) if(projectile.ModProjectile is IcePrincessArm && projectile.ai[0] == NPC.whoAmI + 1 && projectile.ai[2] != 0f) existingIcicles.Add(projectile);
					if(existingIcicles.Count > 0) {
						Projectile projectile = existingIcicles[Main.rand.Next(existingIcicles.Count)];
						bool canTurn = false;
						foreach(Projectile other in Main.ActiveProjectiles) if(other.ai[1] == projectile.ai[1] && projectile.ai[2] == -other.ai[2]) canTurn = true;
						else if(canTurn) break;
						projectile.ai[2] = 0f;
						projectile.netUpdate = true;
						if(canTurn) NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
					}
					else NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
				}
				if(NPC.ai[0] > -150f && NPC.ai[0] < -115f) NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
				NPC.velocity *= 0.96f;
			}
			else {
				NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
				Vector2 hoverPos = target.Center;
				hoverPos.X -= NPC.direction * 240f;
				hoverPos.Y -= 64f;
				NPC.velocity += (hoverPos - NPC.Center).SafeNormalize(Vector2.Zero) * (Math.Sign(NPC.velocity.X) == NPC.direction ? 0.24f : 0.14f);
				NPC.velocity *= 0.96f;
			}
			if(++NPC.ai[0] > 60f) {
				NPC.ai[0] = 0f;
				List<Projectile> existingIcicles = new();
				int proj = ModContent.ProjectileType<IcePrincessArm>();
				if(Main.hardMode) foreach(Projectile projectile in Main.ActiveProjectiles) if(projectile.type == proj && projectile.ai[0] == NPC.whoAmI + 1) existingIcicles.Add(projectile);
				if(Main.netMode != 1) for(int i = -1; i <= 1; i += 2) if(Main.hardMode) {
					List<Projectile> existingSameIcicles = new();
					foreach(Projectile projectile in existingIcicles) if(projectile.ai[2] == i) existingSameIcicles.Add(projectile);
					int[] counters = new int[3];
					if(existingSameIcicles.Count > 0) foreach(Projectile projectile in existingSameIcicles) if(projectile.ai[1] == 0f) counters[0]++;
					else if(projectile.ai[1] == 1f) counters[1]++;
					else if(projectile.ai[2] == -1f) counters[2]++;
					if(counters[0] == 0) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, proj, 16, 0f, Main.myPlayer, NPC.whoAmI + 1, 0f, i);
					else if(counters[1] == 0) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, proj, 16, 0f, Main.myPlayer, NPC.whoAmI + 1, 1f, i);
					else if(counters[2] == 0) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, proj, 16, 0f, Main.myPlayer, NPC.whoAmI + 1, -1f, i);
					else NPC.ai[0] = -180f;
				}
				else {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, proj, 12, 0f, Main.myPlayer, NPC.whoAmI + 1, -1f, i);
					NPC.ai[0] = -180f;
				}
			}
		}
		public override bool PreDraw(SpriteBatch sprite, Vector2 screenPos, Color drawColor) {
			if(NPC.ai[0] > -120f && NPC.ai[0] < 0f) sprite.Draw(TextureAssets.Extra[174].Value, NPC.Center - Vector2.UnitY.RotatedBy(NPC.rotation) * 16f - screenPos, null, Color.Aquamarine with {A = 0} * 0.3f * MathHelper.Min(1f, (float)Math.Sin(MathHelper.Pi * (NPC.ai[0] + 120f) / 120f) * 2.5f) * NPC.Opacity, NPC.rotation, TextureAssets.Extra[174].Value.Size() / 2, NPC.scale * 0.25f, SpriteEffects.None, 0f); 
			Texture2D texture = TextureAssets.Npc[Type].Value;
			SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			Vector2 origin = NPC.frame.Size() / 2f;
			if(NPC.ai[0] > -180f && NPC.ai[0] < -120f) for(int i = 0; i < 4; i++) sprite.Draw(texture, NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i + (NPC.ai[0] + 180f) / 60f * MathHelper.TwoPi * NPC.spriteDirection) * (float)Math.Sin(MathHelper.Pi * (NPC.ai[0] + 180f) / 60f) * 6f - screenPos, NPC.frame, Color.Aquamarine with {A = 0} * 0.3f * NPC.Opacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			else if(NPC.ai[0] > -120f && NPC.ai[0] < 0f) for(int i = 0; i < 4; i++) sprite.Draw(texture, NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i) * 4f - screenPos, NPC.frame, Color.Aquamarine with {A = 0} * 0.3f * MathHelper.Min(1f, (float)Math.Sin(MathHelper.Pi * (NPC.ai[0] + 120f) / 120f) * 2.5f) * NPC.Opacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			sprite.Draw(texture, NPC.Center - screenPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(NPC.GetAlpha(drawColor)), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			if(NPC.ai[0] > -180f && NPC.ai[0] < 0f) sprite.Draw(texture, NPC.Center - screenPos, NPC.frame, Color.Aquamarine with {A = 0} * 0.3f * MathHelper.Min(1f, (float)Math.Sin(MathHelper.Pi * (NPC.ai[0] + 180f) / 180f) * 3f) * NPC.Opacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			return false;
		}
	}
}
