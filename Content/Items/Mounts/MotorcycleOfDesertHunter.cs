using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Waybound.Content.Mounts;

namespace Waybound.Content.Items.Mounts
{
	public class MotorcycleOfDesertHunter : ModItem
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Desert motorbike key");
			// Tooltip.SetDefault("");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{

			Item.damage = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 16;
			Item.height = 30;
			Item.UseSound = SoundID.Item14;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.rare = -12;
			Item.noMelee = true;
			Item.master = true;
			Item.value = Item.sellPrice(0, 5, 50);
			Item.mountType = ModContent.MountType<MotorcycleOfDesertHunterM>();
		}


		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
			{
				player.AddBuff(Item.buffType, 3600);
			}
		}
	}

	public class MotorcycleOfDesertHunterB : ModBuff
	{
		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Motorcycle of Desert Hunter");
			// Description.SetDefault("");
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(ModContent.MountType<MotorcycleOfDesertHunterM>(), player);
			player.buffTime[buffIndex] = 10;
			player.noKnockback = true;
			Lighting.AddLight((int)player.Center.X / 16, (int)player.Center.Y / 16, 0.75f, 0.75f, 0.75f);
			if (player.ownedProjectileCounts[ModContent.ProjectileType<MotorcycleOfDesertHunterP>()] == 0 && Math.Abs(player.velocity.X) > 10f)
			{
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, new Vector2(0, 0), ModContent.ProjectileType<MotorcycleOfDesertHunterP>(), 40, 0f, player.whoAmI);
			}
		}
	}
    public class MotorcycleOfDesertHunterP : ModProjectile
    {
        public int animationTimer;
        public int scaleCounter = 0;

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Shield of Desert Hunter");
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.ignoreWater = true;
            Projectile.light = 0.5f;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.scale = 1f;
            Projectile.DamageType = DamageClass.Generic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.HasBuff(ModContent.BuffType<MotorcycleOfDesertHunterB>()) || Math.Abs(player.velocity.X) <= 10f)
            {
                Projectile.Kill();
            }

            Projectile.Center = player.Center;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire, 120, false);
        }
    }
}