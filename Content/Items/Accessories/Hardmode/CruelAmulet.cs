//using ValhallaMod.Items.AI;
//using Terraria;
//using Microsoft.Xna.Framework;
//using Terraria.ID;
//using Terraria.ModLoader;
//using System;
//using ValhallaMod.DamageClasses;
//using Synergia.Content.Projectiles.ActiveAccessoriesProjectiles;
//using static Terraria.ModLoader.ModContent;
//using Synergia.Common;
//using System.Collections.Generic;
//using Terraria.Audio;

//namespace Synergia.Content.Items.ActiveAccessories
//{
//    public class CruelAmulet : ValhallaMod.Items.AI.ActiveAccessoryItem
//    {
//        // Set this to true so item can be equiped
//        public override void SetStaticDefaults()
//        {
//            ValhallaMod.Values.ActiveAccessoryItems[Item.type] = true;
//        }

//        public override void SafeSetDefaults()
//        {
//            Item.width = 28;
//            Item.height = 34;
//            Item.value = Item.sellPrice(0, 0, 25, 0);
//            Item.rare = ItemRarityID.Purple;
//            Item.accessory = true;
//            Item.DamageType = GetInstance<AccessoryDamageClass>();
//            Item.damage = 0;

//            cooldown = 30 * 60; // Set Cooldown 60 is 1 sec
//        }

//        // Function that is triggered when Active Accessory is Used
//        public override bool Use(Player player, ref int time, ref int damage, ref bool silent)
//        {
//            Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero,
//                ProjectileType<BoneFlash>(), 0, 0, player.whoAmI);

//            DestroyHostileProjectiles();

//            SoundEngine.PlaySound(SoundID.NPCHit2, player.Center);
//            SoundEngine.PlaySound(SoundID.NPCHit11, player.Center);

//            return true;
//        }

//        private void DestroyHostileProjectiles()
//        {
//            for (int i = 0; i < Main.maxProjectiles; i++)
//            {
//                Projectile proj = Main.projectile[i];

//                if (proj != null && proj.active && proj.hostile && !proj.friendly && proj.damage > 0)
//                {
//                    proj.Kill();
//                }
//            }
//        }

//        // Passive accessory buffs
//        public override void SafeUpdateAccessory(Player player, bool hideVisual)
//        {
//            // Add any passive effects here
//        }
//    }

//    // Visual effect projectile, copies  the EnferBoom but bone-colored
//    public class BoneFlash : ModProjectile
//    {
//        private ref float Timer => ref Projectile.ai[0];

//        public override string Texture => "Synergia/Assets/Textures/Glow";

//        public override void SetDefaults()
//        {
//            Projectile.width = 80;
//            Projectile.height = 80;
//            Projectile.timeLeft = 18;
//            Projectile.penetrate = -1;
//            Projectile.damage = 0;
//            Projectile.friendly = true;
//            Projectile.hostile = false;
//            Projectile.tileCollide = false;
//            Projectile.ignoreWater = true;
//            Projectile.usesLocalNPCImmunity = true;
//            Projectile.localNPCHitCooldown = -1;
//        }

//        public override void AI()
//        {
//            Timer++;
//            Projectile.scale = MathHelper.Lerp(0.4f, 2.5f, Math.Min(Timer / 6f, 1f));
//            Projectile.rotation += 0.12f;
//            if (Timer > 12f)
//            {
//                Projectile.alpha = (int)(255f * ((Timer - 12f) / 6f));
//            }

//            if (Timer == 1)
//            {

//            }
//        }

//        public override bool PreDraw(ref Color lightColor)
//        {
//            SpriteBatch sb = Main.spriteBatch;

//            Texture2D glowTex = ModContent.Request<Texture2D>("Synergia/Assets/Textures/Glow").Value;
//            Texture2D coreTex = ModContent.Request<Texture2D>("Synergia/Assets/Textures/LightTrail_1").Value;

//            sb.End();
//            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
//                DepthStencilState.None, RasterizerState.CullNone, null,
//                Main.GameViewMatrix.TransformationMatrix);

//            float fade = MathHelper.Clamp(1f - Timer / 14f, 0f, 1f);
//            float pulse = 1f + MathF.Sin(Timer * 0.8f) * 0.1f;

            
//            Color outer = new Color(80, 75, 65) * fade * 0.7f;      
//            Color mid = new Color(160, 155, 140) * fade;          
//            Color core = new Color(220, 215, 200) * fade;         

//            sb.Draw(glowTex,
//                Projectile.Center - Main.screenPosition,
//                null,
//                outer,
//                -Projectile.rotation * 0.35f,
//                glowTex.Size() / 2f,
//                Projectile.scale * 1.2f,
//                SpriteEffects.None,
//                0f);
//            sb.Draw(glowTex,
//                Projectile.Center - Main.screenPosition,
//                null,
//                mid,
//                Projectile.rotation * 0.5f,
//                glowTex.Size() / 2f,
//                Projectile.scale,
//                SpriteEffects.None,
//                0f);
//            sb.Draw(coreTex,
//                Projectile.Center - Main.screenPosition,
//                null,
//                core * 0.4f,
//                Projectile.rotation * 0.2f,
//                coreTex.Size() / 2f,
//                Projectile.scale * 0.9f * pulse,
//                SpriteEffects.None,
//                0f);
//            sb.Draw(coreTex,
//                Projectile.Center - Main.screenPosition,
//                null,
//                core * 0.6f,
//                -Projectile.rotation * 0.15f,
//                coreTex.Size() / 2f,
//                Projectile.scale * 0.65f * pulse,
//                SpriteEffects.None,
//                0f);
//            sb.Draw(coreTex,
//                Projectile.Center - Main.screenPosition,
//                null,
//                core,
//                0f,
//                coreTex.Size() / 2f,
//                Projectile.scale * 0.4f,
//                SpriteEffects.None,
//                0f);
//            sb.End();
//            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
//                DepthStencilState.None, RasterizerState.CullNone, null,
//                Main.GameViewMatrix.TransformationMatrix);

//            return false;
//        }
//    }
//}