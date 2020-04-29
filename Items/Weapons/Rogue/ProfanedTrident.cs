using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class ProfanedTrident : RogueWeapon
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infernal Spear");
            Tooltip.SetDefault("Throws a homing spear that explodes on enemy hits\n" +
			"Stealth strikes summon fireballs as they fly before exploding into a fiery fountain");
        }

        public override void SafeSetDefaults()
        {
            item.width = 72;
            item.damage = 1000;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.useAnimation = 13;
            item.useStyle = 1;
            item.useTime = 13;
            item.knockBack = 8f;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.height = 72;
            item.value = Item.buyPrice(1, 80, 0, 0);
            item.rare = 10;
            item.shoot = ModContent.ProjectileType<InfernalSpearProjectile>();
            item.shootSpeed = 28f;
            item.Calamity().rogue = true;
            item.Calamity().customRarity = CalamityRarity.DarkBlue;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            int proj = Projectile.NewProjectile(position.X, position.Y, speedX, speedY, type, damage, knockBack, player.whoAmI, 0f, 0f);
            Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
            return false;
        }
    }
}
