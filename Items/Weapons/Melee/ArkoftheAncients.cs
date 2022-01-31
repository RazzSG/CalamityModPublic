using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class ArkoftheAncients : ModItem
    {
        public float Combo = 1f;
        public float Charge = 0f;
        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Ark of the Ancients");
            Tooltip.SetDefault("Using RMB will extend the Ark out in front of you. Hitting an enemy with it will parry them, granting you a small window of invulnerability\n" +
                "You can also parry projectiles and temporarily make them deal 100 less damage\n" +
                "Parrying will empower the next 10 swings of the sword, boosting their damage and letting them throw projectiles out\n" +
                "A heavenly blade forged to vanquish all evil");
        }

        public override string Texture => "CalamityMod/Items/Weapons/Melee/ArkOfTheAncients";

        public override void SetDefaults()
        {
            item.width = item.height = 60;
            item.damage = 80;
            item.melee = true;
            item.noUseGraphic = true;
            item.noMelee = true;
            item.useAnimation = 22;
            item.useTime = 22;
            item.useTurn = true;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.knockBack = 6.25f;
            item.UseSound = null;
            item.autoReuse = true;
            item.value = Item.buyPrice(0, 48, 0, 0);
            item.rare = ItemRarityID.LightPurple;
            item.shoot = ModContent.ProjectileType<EonBeam>();
            item.shootSpeed = 15f;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<ArkoftheAncientsSwungBlade>());
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.altFunctionUse == 2)
            {
                if (!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && (n.type == ProjectileType<ArkoftheAncientsParryHoldout>())))
                    Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<ArkoftheAncientsParryHoldout>(), damage, knockBack, player.whoAmI, 0, 0);
                return false;
            }

            //Failsafe
            if (Combo != -1 && Combo != 1)
                Combo = 1;

            if (Charge > 0)
                damage *= 2;
            Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ProjectileType<ArkoftheAncientsSwungBlade>(), damage, knockBack, player.whoAmI, Combo, Charge);

            Combo *= -1f;
            Charge --;
            if (Charge < 0)
                Charge = 0;

            return false;
        }



        public override ModItem Clone(Item item)
        {
            var clone = base.Clone(item);

            (clone as ArkoftheAncients).Charge = (item.modItem as ArkoftheAncients).Charge;

            return clone;
        }
        public override ModItem Clone()
        {
            var clone = base.Clone();

            (clone as ArkoftheAncients).Charge = Charge;

            return clone;
        }


        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(Charge);
        }

        public override void NetRecieve(BinaryReader reader)
        {
            Charge = reader.ReadInt32();
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Starfury);
            recipe.AddIngredient(ItemID.EnchantedSword);
            recipe.AddIngredient(ItemID.Excalibur);
            recipe.AddIngredient(ModContent.ItemType<EssenceofCinder>(), 3);
            recipe.AddIngredient(ModContent.ItemType<EssenceofEleum>(), 3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
            recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.Starfury);
            recipe.AddIngredient(ItemID.Arkhalis);
            recipe.AddIngredient(ItemID.Excalibur);
            recipe.AddIngredient(ModContent.ItemType<EssenceofCinder>(), 3);
            recipe.AddIngredient(ModContent.ItemType<EssenceofEleum>(), 3);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }


        public override void OnHitNPC(Player player, NPC target, int damage, float knockback, bool crit)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);
        }

        public override void OnHitPvp(Player player, Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 300);
        }
    }
}
