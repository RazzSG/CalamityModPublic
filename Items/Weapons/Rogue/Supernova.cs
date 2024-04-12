﻿using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class Supernova : RogueWeapon
    {
        public static readonly SoundStyle ExplosionSound = new("CalamityMod/Sounds/Item/SupernovaBoom") { Volume = 0.9f };
        public override void SetDefaults()
        {
            Item.width = 34;
            Item.height = 36;
            Item.damage = 375;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 48;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 70;
            Item.knockBack = 18f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.Rarity15BuyPrice;
            Item.shoot = ModContent.ProjectileType<SupernovaBomb>();
            Item.shootSpeed = 16f;
            Item.DamageType = RogueDamageClass.Instance;
            Item.rare = ModContent.RarityType<Violet>();
        }

        public override float StealthDamageMultiplier => 1.08f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealth.WithinBounds(Main.maxProjectiles))
                    Main.projectile[stealth].Calamity().stealthStrike = true;
                return false;
            }
            else
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SealedSingularity>().
                AddIngredient<StarofDestruction>().
                AddIngredient<TotalityBreakers>().
                AddIngredient<BallisticPoisonBomb>().
                AddIngredient<MiracleMatter>().
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
