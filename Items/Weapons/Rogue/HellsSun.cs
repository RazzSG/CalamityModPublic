using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.CalPlayer;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class HellsSun : RogueWeapon
    {
        private static int damage = 250;
        private static int knockBack = 5;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Hell's Sun");
            Tooltip.SetDefault("Shoots a gravity-defying spiky ball. Stacks up to 10.\n" +
                "Once stationary, periodically emits small suns that explode on hit\n" +
                "Stealth strikes emit suns at a faster rate and last for a longer amount of time\n" +
				"Right click to delete all existing spiky balls");
        }

        public override void SafeSetDefaults()
        {
            item.damage = damage;
            item.Calamity().rogue = true;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.width = 1;
            item.height = 1;
            item.useTime = 15;
            item.useAnimation = 15;
            item.useStyle = 1;
            item.knockBack = knockBack;
            item.value = Item.buyPrice(0, 12, 0, 0);
            item.rare = 10;
            item.Calamity().postMoonLordRarity = 12;;
            item.UseSound = SoundID.Item1;
            item.autoReuse = true;
            item.maxStack = 10;

            item.shootSpeed = 5f;
            item.shoot = ModContent.ProjectileType<HellsSunProj>();

        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.ownedProjectileCounts[item.shoot] > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
			else
			{
				int UseMax = item.stack - 1;

				if (player.ownedProjectileCounts[item.shoot] > UseMax)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            CalamityPlayer modPlayer = player.Calamity();
			modPlayer.killSpikyBalls = false;
            if (player.altFunctionUse == 2)
			{
				modPlayer.killSpikyBalls = true;
				return false;
			}
            if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                int stealth = Projectile.NewProjectile(position, new Vector2(speedX, speedY), ModContent.ProjectileType<HellsSunProj>(), damage, knockBack, player.whoAmI, 0f, 0f);
                Main.projectile[stealth].Calamity().stealthStrike = true;
                Main.projectile[stealth].penetrate = -1;
                Main.projectile[stealth].timeLeft = 2400;
                return false;
            }
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        /*public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);

            recipe.AddIngredient(ItemID.SpikyBall, 100);
            recipe.AddIngredient(ModContent.ItemType<UnholyEssence>(), 10);
            r.AddTile(TileID.LunarCraftingStation);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }*/
    }
}
