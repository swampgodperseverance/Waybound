using Waybound.Content.Items.Accessories.Hardmode;
using Waybound.Content.Projectiles.Hostile.Cruor;
using Waybound.Content.Dusts;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using Waybound.Content.Items.Placeable.Bosses;

namespace Waybound.Content.NPCs.Dungeon
{
    [AutoloadBossHead]
    public class Cruor : ModNPC
	{
        private bool isSpawning = true;
        private float spawnTimer = 0f;
        //Just change these
        public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 8;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers { Velocity = 1f };
			NPCID.Sets.NPCBestiaryDrawOffset.Add(base.Type, value);
		}
		//and the stats here
		public override void SetDefaults() {
			NPC.width = 40;
			NPC.height = 60;
			NPC.damage = 0; //no contact damage because that would be annoying
			NPC.defense = 30;
			NPC.lifeMax = 36000;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.netAlways = true;
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.DD2_SkeletonHurt;
			NPC.DeathSound = SoundID.DD2_SkeletonDeath;
			NPC.value = Item.sellPrice(0, 7, 9  , 0);
            NPC.alpha = 255;
        }
		//and the bestiary shit here
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
			BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheDungeon,
			new FlavorTextBestiaryInfoElement("Soulless creature.")
		});
        private void SpawnAnimation(){
            spawnTimer++;

            if (spawnTimer < 60){
                NPC.alpha = 255 - (int)(spawnTimer / 60f * 255);
                NPC.scale = 0.5f + (spawnTimer / 60f) * 0.5f;

                if (Main.rand.NextBool(2))
                {
                    Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(40, 40);
                    Dust.NewDustPerfect(
                        dustPos,
                        ModContent.DustType<CruorDust>(),
                        (NPC.Center - dustPos) * 0.05f
                    ).noGravity = true;
                }

                if (spawnTimer == 30) SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            }
            else{
                NPC.alpha = 0;
                NPC.scale = 1f;
                isSpawning = false;
                for (int i = 0; i < 20; i++)
                {
                    Dust.NewDustPerfect(NPC.Center, ModContent.DustType<CruorDust>(), Main.rand.NextVector2Circular(4f, 4f)).noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }
        }
        public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			if(++NPC.frameCounter >= 8) {
				NPC.frameCounter = 0;
				NPC.frame.Y += frameHeight;
				int half = frameHeight * Main.npcFrameCount[NPC.type] / 2;
				if(NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type] / (float)(NPC.localAI[0] > 0f ? 1 : 2)) NPC.frame.Y -= half;
				if(NPC.localAI[0] > 0f && NPC.frame.Y < half) NPC.frame.Y += half;
			}
			if(NPC.ai[0] == 3f && NPC.localAI[3] > 0f) NPC.rotation = Vector2.Normalize(Vector2.Lerp((NPC.velocity.X / 45f).ToRotationVector2(), (NPC.ai[2] + (NPC.spriteDirection - 1) * MathHelper.PiOver2).ToRotationVector2(), NPC.localAI[3])).ToRotation();
			else NPC.rotation = NPC.velocity.X / 45f;
		}
        public override void AI()
        {
            if (isSpawning)
            {
                SpawnAnimation();
                return;
            }

            Player target = NPC.target > -1 ? Main.player[NPC.target] : null;

            // FIX for checking for players
            if (target == null || !target.active || target.dead || target.Distance(NPC.Center) > 1000f){
                NPC.TargetClosest();
                target = NPC.target > -1 ? Main.player[NPC.target] : null;
                if (target == null || !target.active || target.dead || target.Distance(NPC.Center) > 1000f)
                {
                    if (NPC.alpha >= 255)
                        NPC.active = false;
                    else
                        NPC.alpha += 17;
                    return; 
                }
            }

            // NUll checking
            if (target != null){
                switch (NPC.ai[0])
                {
                    case 0:
                        if (NPC.ai[1] > 0f) NPC.ai[1]++;
                        if (NPC.ai[1] > (Main.getGoodWorld ? 60f : 120f))
                        {
                            List<float> attacks = new() { 1f, 2f, 3f };
                            attacks.Remove(NPC.ai[3]);
                            NPC.ai[0] = attacks[Main.rand.Next(attacks.Count)];
                            NPC.ai[1] = 0f;
                            NPC.netUpdate = true;
                            NPC.TargetClosest();
                        }
                        else
                        {
                            NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
                            Vector2 targetPos = target.Center - new Vector2(NPC.direction * 96f, 128f);
                            NPC.velocity += (targetPos - NPC.Center) * 0.0018f;
                            NPC.velocity *= 0.92f;
                            if (NPC.ai[1] == 0f && NPC.Distance(targetPos) < 160f) NPC.ai[1]++;
                        }
                        break;
                    case 1:
                        if (++NPC.ai[1] > 240f)
                        {
                            NPC.ai[3] = NPC.ai[0];
                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.localAI[2] = 0f;
                            NPC.netUpdate = true;
                            NPC.TargetClosest();
                        }
                        else
                        {
                            NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
                            Vector2 targetPos = target.Center - new Vector2(NPC.direction * 320f, 0f);
                            if (NPC.ai[1] % 60 < 20 || NPC.ai[1] % 60 > 40)
                            {
                                NPC.velocity += (targetPos - NPC.Center) * 0.0048f * (NPC.ai[1] % 60 > 40 ? (NPC.ai[1] % 60 - 40) / 20f : 1f);
                                if (NPC.ai[1] % 60 < 20 || NPC.ai[1] % 60 > 50) NPC.localAI[0] = 0f;
                            }
                            else
                            {
                                if (NPC.ai[1] % 60 < 50)
                                {
                                    if (NPC.ai[1] % 60 > 30) NPC.localAI[2] = (float)Math.Sin(MathHelper.Pi * (NPC.ai[1] % 60 - 30f) / 30f) * 3f;
                                    int e = Dust.NewDust(NPC.Center - new Vector2(1f - NPC.direction * NPC.width, 1f) + Main.rand.NextVector2Circular(NPC.width, NPC.height), 2, 2, 182, 0f, 0f, 150, default(Color), (NPC.ai[1] % 60 - 20f) / 20f);
                                    Main.dust[e].velocity = (NPC.Center - Vector2.One - Main.dust[e].position) * 0.1f + NPC.velocity;
                                    Main.dust[e].noGravity = true;
                                }
                                if (NPC.ai[1] % 60 == 40)
                                {
                                    NPC.velocity.X -= NPC.direction * 4f;
                                    if (Main.netMode != 1) Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center + NPC.rotation.ToRotationVector2() * NPC.direction * NPC.width / 3, Vector2.UnitX * NPC.direction * 4f, 270, 27, 1f, Main.myPlayer).tileCollide = !Main.expertMode;
                                }
                                else if (NPC.ai[1] % 60 == 20)
                                {
                                    NPC.localAI[1] = 10f;
                                    SoundEngine.PlaySound(SoundID.DD2_SkeletonSummoned, NPC.Center);
                                }
                                NPC.localAI[0] = 1f;
                            }
                            NPC.velocity *= 0.92f;
                        }
                        break;
                    case 2:
                        if (++NPC.ai[1] > 180f)
                        {
                            NPC.ai[3] = NPC.ai[0];
                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.localAI[2] = 0f;
                            NPC.netUpdate = true;
                            NPC.TargetClosest();
                        }
                        else
                        {
                            NPC.localAI[0] = NPC.ai[1] % 90 < 80 && NPC.ai[1] % 90 > 40 && (NPC.ai[1] < 80f || Main.expertMode) ? 1f : 0f;
                            if (NPC.ai[1] < 40f) NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
                            else if (NPC.ai[1] % 90 == 60 && (NPC.ai[1] == 60f || Main.expertMode) && Main.netMode != 1)
                            {
                                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                                if (NPC.ai[1] == 60f) NPC.ai[2] = Main.rand.NextFloat(MathHelper.TwoPi);
                                for (int i = 0; i < (Main.getGoodWorld ? 7 : 5); i++) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + NPC.rotation.ToRotationVector2() * NPC.direction * NPC.width / 3, (NPC.ai[2] + i / (Main.getGoodWorld ? 7f : 5f) * MathHelper.TwoPi).ToRotationVector2(), ModContent.ProjectileType<CruorStar>(), 38, 1f, Main.myPlayer, NPC.ai[1] == 60f ? NPC.direction : -NPC.direction);
                            }
                            else if (NPC.ai[1] % 90 == 40 && (NPC.ai[1] == 40f || Main.expertMode))
                            {
                                SoundEngine.PlaySound(SoundID.Item4, NPC.Center);
                                NPC.localAI[1] = 10f;
                            }
                            if (NPC.ai[1] % 90 > 45) NPC.localAI[2] = (float)Math.Sin(MathHelper.Pi * (NPC.ai[1] % 90 - 45f) / 30f) * 5f;
                            NPC.velocity *= 0.92f;
                        }
                        break;
                    case 3:
                        if (++NPC.ai[1] > 120f)
                        {
                            NPC.ai[3] = NPC.ai[0];
                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.localAI[0] = 0f;
                            NPC.localAI[3] = 0f;
                            NPC.netUpdate = true;
                            NPC.TargetClosest();
                        }
                        else
                        {
                            NPC.localAI[0] = NPC.ai[1] < 60f ? 0f : 1f;
                            float lerp = (NPC.ai[1] - 60f) / 60f;
                            if (NPC.ai[1] < 60f)
                            {
                                NPC.direction = Math.Sign(target.Center.X - NPC.Center.X);
                                NPC.ai[2] = (target.Center - NPC.Center).ToRotation();
                            }
                            else if (NPC.ai[1] % (Main.getGoodWorld ? 6f : Main.expertMode ? 10f : 12f) == 0f)
                            {
                                SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
                                if (Main.netMode != 1) Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + NPC.rotation.ToRotationVector2() * NPC.direction * NPC.width / 4, (NPC.ai[2] - (MathHelper.PiOver2 * 0.6f - MathHelper.PiOver2 * 1.2f * lerp) * NPC.direction).ToRotationVector2(), ModContent.ProjectileType<CruorSpike>(), 36, 1f, Main.myPlayer);
                            }
                            lerp = NPC.ai[1] / 120f;
                            NPC.localAI[3] = NPC.localAI[0] * MathHelper.SmoothStep(0f, 0.2f, (float)Math.Sin(MathHelper.Pi * lerp * lerp * lerp));
                            NPC.velocity *= 0.92f;
                        }
                        break;
                }
                if (NPC.localAI[1] > 0f) NPC.localAI[1]--;
            }
        }
        public override bool PreDraw(SpriteBatch sprite, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;
			Vector2 origin = NPC.frame.Size() / 2f;
			Color color = NPC.GetNPCColorTintedByBuffs(NPC.GetAlpha(drawColor));
			Vector2 offset = Vector2.Zero;
			SpriteEffects spriteEffects = NPC.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			for(int i = 0; i < 4; i++) sprite.Draw(texture, NPC.Center + Vector2.UnitX.RotatedBy(MathHelper.PiOver2 * i) * (3f + NPC.localAI[2] + (float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi)) * NPC.Opacity - screenPos, NPC.frame, new Color(100, 0, 0, 0) * NPC.Opacity, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			sprite.Draw(texture, NPC.Center - screenPos, NPC.frame, color, NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			texture = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			sprite.Draw(texture, NPC.Center - screenPos, NPC.frame, NPC.GetNPCColorTintedByBuffs(NPC.GetAlpha(Color.White * NPC.Opacity)), NPC.rotation, origin, NPC.scale, spriteEffects, 0f);
			texture = ModContent.Request<Texture2D>("Waybound/Assets/Textures/Shortsword").Value;
			if(NPC.localAI[1] > 0f) sprite.Draw(texture, NPC.Center - new Vector2(7f * -NPC.spriteDirection, 18f).RotatedBy(NPC.rotation) - screenPos, null, new Color(NPC.ai[0] == 2f ? 255 : 200, NPC.ai[0] == 2f ? 55 : 0, NPC.ai[0] == 2f ? 75 : 0, 0) * (float)Math.Sin(NPC.localAI[1] * 0.1f * MathHelper.Pi), NPC.rotation, texture.Size() / 2f, NPC.scale * new Vector2(1f - NPC.localAI[1] * 0.1f, 0.8f), spriteEffects, 0f);
			return false;
		}
        public override void ModifyNPCLoot(NPCLoot npcLoot){
            npcLoot.Add(ItemDropRule.OneFromOptions(1, 3, 5,
                ItemID.Ectoplasm
            ));

            npcLoot.Add(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<TheOriginOfSymmetry>(),
              //  ModContent.ItemType<CruelAmulet>(),
                ModContent.ItemType<Jeweler>(),
                ModContent.ItemType<MortalStones>()
            ));

            npcLoot.Add(ItemDropRule.Common(ItemID.Nazar, 10));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CruorTrophy>(), 10));
            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CruorRelicItem>()));
        }
        public override void OnKill()
        {
            for (int i = 0; i < 30; i++)
            {
                Dust.NewDustPerfect(NPC.Center, ModContent.DustType<CruorDust>(), Main.rand.NextVector2Circular(5f, 5f), 0, default, 1.8f).noGravity = true;
            }
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            NPC.SetEventFlagCleared(ref Common.ModSystems.WayboundWorld.cruorDead, -1);
        }
    }
}
