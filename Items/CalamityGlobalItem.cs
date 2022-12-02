﻿using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Potions;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Potions;
using CalamityMod.Items.VanillaArmorChanges;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.Other;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using CalamityMod.UI;
using CalamityMod.UI.CalamitasEnchants;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace CalamityMod.Items
{
    public partial class CalamityGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        // TODO -- split out a separate GlobalItem for rogue behavior?
        internal float StealthGenBonus;
        internal float StealthStrikePrefixBonus;

        #region Chargeable Item Variables
        public bool UsesCharge = false;
        public float Charge = 0f;
        public float MaxCharge = 1f;
        public float ChargePerUse = 0f;
        // If left at the default value of -1, ChargePerUse is automatically used for alt fire.
        // If you want a different amount of charge used for alt fire, then set a different value here.
        public float ChargePerAltUse = -1f;
        public float ChargeRatio
        {
            get
            {
                float ratio = Charge / MaxCharge;
                return float.IsNaN(ratio) || float.IsInfinity(ratio) ? 0f : MathHelper.Clamp(ratio, 0f, 1f);
            }
        }
        #endregion

        #region Enchantment Variables
        public bool CannotBeEnchanted = false;
        public Enchantment? AppliedEnchantment = null;
        public float DischargeEnchantExhaustion = 0;
        public float DischargeExhaustionRatio
        {
            get
            {
                float ratio = DischargeEnchantExhaustion / DischargeEnchantExhaustionCap;
                return float.IsNaN(ratio) || float.IsInfinity(ratio) ? 0f : MathHelper.Clamp(ratio, 0f, 1f);
            }
        }
        public const float DischargeEnchantExhaustionCap = 1600f;
        public const float DischargeEnchantMinDamageFactor = 0.77f;
        public const float DischargeEnchantMaxDamageFactor = 1.26f;
        #endregion

        // Miscellaneous stuff
        public int timesUsed = 0;
        public bool donorItem = false;
        public bool devItem = false;
        public bool canFirePointBlankShots = false;

        public static readonly Color ExhumedTooltipColor = new Color(198, 27, 64);

        public CalamityGlobalItem()
        {
            StealthGenBonus = 1f;
            StealthStrikePrefixBonus = 0f;
        }

        // Ozzatron 21MAY2022: This function is required by TML 1.4's new clone behavior.
        // This behavior is sadly mandatory because there are a few places in vanilla Terraria which use cloning.
        // Most notably: reforging and item tooltips.
        //
        // It manually copies everything because I don't trust the base clone behavior after seeing the insane bugs.
        //
        // ANY TIME YOU ADD A VARIABLE TO CalamityGlobalItem, IT MUST BE COPIED IN THIS FUNCTION.
        public override GlobalItem Clone(Item item, Item itemClone)
        {
            CalamityGlobalItem myClone = (CalamityGlobalItem)base.Clone(item, itemClone);

            // Rogue
            myClone.StealthGenBonus = StealthGenBonus;
            myClone.StealthStrikePrefixBonus = StealthStrikePrefixBonus;

            // Charge (Draedon's Arsenal)
            myClone.UsesCharge = UsesCharge;
            myClone.Charge = Charge;
            myClone.MaxCharge = MaxCharge;
            myClone.ChargePerUse = ChargePerUse;
            myClone.ChargePerAltUse = ChargePerAltUse;

            // Enchantments
            myClone.CannotBeEnchanted = CannotBeEnchanted;
            myClone.AppliedEnchantment = AppliedEnchantment.HasValue ? AppliedEnchantment.Value : null;
            myClone.DischargeEnchantExhaustion = DischargeEnchantExhaustion;

            // Miscellaneous
            myClone.timesUsed = timesUsed;
            myClone.donorItem = donorItem;
            myClone.devItem = devItem;
            myClone.canFirePointBlankShots = canFirePointBlankShots;

            return myClone;
        }

        #region SetDefaults
        public override void SetDefaults(Item item)
        {
            // All items that stack to 30, 50, 75, 99 , or 999 now stack to 9999 instead.
            if (item.maxStack == 30 || item.maxStack == 50 || item.maxStack == 75 || item.maxStack == 99 || item.maxStack == 999)
                item.maxStack = 9999;

            // Having more than this causes integer overflow
            if (item.type == ItemID.PlatinumCoin)
                item.maxStack = 2147;

            // Shield of Cthulhu cannot be enchanted (it is an accessory with a damage value).
            // TODO -- there are better ways to do this. Just stop letting accessories be enchanted, even if they do have a damage value.
            if (item.type == ItemID.EoCShield)
                CannotBeEnchanted = true;

            // Star Cannon no longer makes noise.
            if (item.type == ItemID.StarCannon)
                item.UseSound = null;

            // Fix Bones being attracted to the player when you have open ammo slots.
            if (item.type == ItemID.Bone)
                item.notAmmo = true;

            // Modified Pearlwood items are now Light Red.
            if (item.type == ItemID.PearlwoodBow || item.type == ItemID.PearlwoodHammer || item.type == ItemID.PearlwoodSword)
                item.rare = ItemRarityID.LightRed;

            // Volatile Gelatin is pre-mech post-WoF so it should use the pink rarity.
            if (item.type == ItemID.VolatileGelatin)
                item.rare = ItemRarityID.Pink;

            // Soaring Insignia is post-Golem so it should use the yellow rarity.
            if (item.type == ItemID.EmpressFlightBooster)
                item.rare = ItemRarityID.Yellow;

            // Let every accessory be equipped in vanity slots.
            if (item.accessory)
                item.canBePlacedInVanityRegardlessOfConditions = true;

            // Make most expert items no longer expert because they drop in all modes now.
            switch (item.type)
            {
                case ItemID.RoyalGel:
                case ItemID.EoCShield:
                case ItemID.WormScarf:
                case ItemID.BrainOfConfusion:
                case ItemID.HiveBackpack:
                case ItemID.BoneHelm:
                case ItemID.BoneGlove:
                // case ItemID.DemonHeart:
                case ItemID.VolatileGelatin:
                case ItemID.MechanicalBatteryPiece:
                case ItemID.MechanicalWagonPiece:
                case ItemID.MechanicalWheelPiece:
                case ItemID.MinecartMech:
                case ItemID.SporeSac:
                case ItemID.WitchBroom:
                case ItemID.EmpressFlightBooster:
                case ItemID.ShinyStone:
                case ItemID.ShrimpyTruffle:
                case ItemID.GravityGlobe:
                case ItemID.SuspiciousLookingTentacle:
                case ItemID.LongRainbowTrailWings:
                    item.expert = false;
                    break;
            }

            // Apply Calamity Global Item Tweaks.
            SetDefaults_ApplyTweaks(item);

            // Items which are "classic true melee" (melee items with no fired projectile) are automatically reclassed as True Melee class.
            if (item.shoot == ProjectileID.None)
            {
                if (item.DamageType == DamageClass.Melee)
                    item.DamageType = TrueMeleeDamageClass.Instance;
                else if (item.DamageType == DamageClass.MeleeNoSpeed)
                    item.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
            }
        }
        #endregion

        #region Shoot
        public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockBack)
        {
            CalamityPlayer modPlayer = player.Calamity();

            if (item.CountsAsClass<RogueDamageClass>())
            {
                velocity *= modPlayer.rogueVelocity;
                if (modPlayer.gloveOfRecklessness)
                    velocity = velocity.RotatedByRandom(MathHelper.ToRadians(12f));
            }
            if (modPlayer.eArtifact && item.CountsAsClass<RangedDamageClass>())
                velocity *= 1.25f;
        }


        public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockBack)
        {
            CalamityPlayer modPlayer = player.Calamity();
            if (Main.myPlayer == player.whoAmI && player.Calamity().cursedSummonsEnchant && NPC.CountNPCS(ModContent.NPCType<CalamitasEnchantDemon>()) < 2)
            {
                CalamityNetcode.NewNPC_ClientSide(Main.MouseWorld, ModContent.NPCType<CalamitasEnchantDemon>(), player);
                SoundEngine.PlaySound(SoundID.DD2_DarkMageSummonSkeleton, Main.MouseWorld);
            }

            bool belowHalfMana = player.statMana < player.statManaMax2 * 0.5f;
            if (Main.myPlayer == player.whoAmI && player.Calamity().manaMonsterEnchant && Main.rand.NextBool(12) && player.ownedProjectileCounts[ModContent.ProjectileType<ManaMonster>()] <= 0 && belowHalfMana)
            {
                // TODO -- 165,000 base damage? seriously? there's no way that can be right
                int monsterDamage = (int)player.GetTotalDamage<MagicDamageClass>().ApplyTo(165000);
                Vector2 shootVelocity = player.SafeDirectionTo(Main.MouseWorld, -Vector2.UnitY).RotatedByRandom(0.07f) * Main.rand.NextFloat(4f, 5f);
                Projectile.NewProjectile(source, player.Center + shootVelocity, shootVelocity, ModContent.ProjectileType<ManaMonster>(), monsterDamage, 0f, player.whoAmI);
            }

            if (modPlayer.luxorsGift && !item.channel)
            {
                // useTime 9 = 0.9 useTime 2 = 0.2
                double damageMult = 1.0;
                if (item.useTime < 10)
                    damageMult -= (10 - item.useTime) / 10.0;

                double newDamage = damage * damageMult;

                if (player.whoAmI == Main.myPlayer)
                {
                    if (item.CountsAsClass<MeleeDamageClass>())
                    {
                        double meleeDamage = newDamage * 0.25;
                        if (meleeDamage >= 1D)
                        {
                            int projectile = Projectile.NewProjectile(source, position, velocity * 0.5f, ModContent.ProjectileType<LuxorsGiftMelee>(), (int)meleeDamage, 0f, player.whoAmI);
                            if (projectile.WithinBounds(Main.maxProjectiles))
                                Main.projectile[projectile].DamageType = DamageClass.Generic;
                        }
                    }
                    else if (item.CountsAsClass<ThrowingDamageClass>())
                    {
                        double throwingDamage = newDamage * 0.2;
                        if (throwingDamage >= 1D)
                        {
                            int projectile = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LuxorsGiftRogue>(), (int)throwingDamage, 0f, player.whoAmI);
                            if (projectile.WithinBounds(Main.maxProjectiles))
                                Main.projectile[projectile].DamageType = DamageClass.Generic;
                        }
                    }
                    else if (item.CountsAsClass<RangedDamageClass>())
                    {
                        // This projectile is channeled and has no cooldown unless the gun fires
                        // The damage of the projectile is also always the max damage of the weapon, and the shot damage is calculated based off of that
                        // You can see how this may cause issues
                        // The projectile is fired inside of the scope's code instead
                        if (type != ModContent.ProjectileType<TitaniumRailgunScope>())
                        {
                            double rangedDamage = newDamage * 0.15;
                            if (rangedDamage >= 1D)
                            {
                                int projectile = Projectile.NewProjectile(source, position, velocity * 1.5f, ModContent.ProjectileType<LuxorsGiftRanged>(), (int)rangedDamage, 0f, player.whoAmI);
                                if (projectile.WithinBounds(Main.maxProjectiles))
                                    Main.projectile[projectile].DamageType = DamageClass.Generic;
                            }
                        }
                    }
                    else if (item.CountsAsClass<MagicDamageClass>())
                    {
                        double magicDamage = newDamage * 0.3;
                        if (magicDamage >= 1D)
                        {
                            int projectile = Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LuxorsGiftMagic>(), (int)magicDamage, 0f, player.whoAmI);
                            if (projectile.WithinBounds(Main.maxProjectiles))
                                Main.projectile[projectile].DamageType = DamageClass.Generic;
                        }
                    }
                    else if (item.CountsAsClass<SummonDamageClass>() && player.ownedProjectileCounts[ModContent.ProjectileType<LuxorsGiftSummon>()] < 1)
                    {
                        if (damage >= 1D)
                        {
                            int projectile = Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<LuxorsGiftSummon>(), damage, 0f, player.whoAmI);
                            if (projectile.WithinBounds(Main.maxProjectiles))
                            {
                                Main.projectile[projectile].DamageType = DamageClass.Generic;
                                Main.projectile[projectile].originalDamage = item.damage;
                            }
                        }
                    }
                }
            }
            if (modPlayer.bloodflareMage && modPlayer.canFireBloodflareMageProjectile)
            {
                if (item.CountsAsClass<MagicDamageClass>() && !item.channel)
                {
                    modPlayer.canFireBloodflareMageProjectile = false;
                    if (player.whoAmI == Main.myPlayer)
                    {
                        // Bloodflare Mage Bolt: 130%, soft cap starts at 2000 base damage
                        int bloodflareBoltDamage = CalamityUtils.DamageSoftCap(damage * 1.3, 2600);
                        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<GhostlyBolt>(), bloodflareBoltDamage, 1f, player.whoAmI);
                    }
                }
            }
            if (modPlayer.bloodflareRanged && modPlayer.canFireBloodflareRangedProjectile)
            {
                if (item.CountsAsClass<RangedDamageClass>() && !item.channel)
                {
                    modPlayer.canFireBloodflareRangedProjectile = false;
                    if (player.whoAmI == Main.myPlayer)
                    {
                        // Bloodflare Ranged Bloodsplosion: 80%, soft cap starts at 150 base damage
                        // This is intentionally extremely low because this effect can be grossly overpowered with sniper rifles and the like.
                        int bloodsplosionDamage = CalamityUtils.DamageSoftCap(damage * 0.8, 120);
                        Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<BloodBomb>(), bloodsplosionDamage, 2f, player.whoAmI);
                    }
                }
            }
            if (modPlayer.tarraMage && !item.channel)
            {
                if (modPlayer.tarraCrits >= 5 && player.whoAmI == Main.myPlayer)
                {
                    modPlayer.tarraCrits = 0;
                    // Tarragon Mage Leaves: (8-10) x 20%, soft cap starts at 200 base damage
                    int leafAmt = 8 + Main.rand.Next(3); // 8, 9, or 10
                    int leafDamage = (int)(damage * 0.2);

                    for (int l = 0; l < leafAmt; l++)
                    {
                        float spreadMult = 0.025f * l;
                        float xDiff = velocity.X + Main.rand.Next(-25, 26) * spreadMult;
                        float yDiff = velocity.Y + Main.rand.Next(-25, 26) * spreadMult;
                        float speed = velocity.Length();
                        speed = item.shootSpeed / speed;
                        xDiff *= speed;
                        yDiff *= speed;
                        int projectile = Projectile.NewProjectile(source, position, new Vector2(xDiff, yDiff), ProjectileID.Leaf, leafDamage, knockBack, player.whoAmI);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].DamageType = DamageClass.Generic;
                    }
                }
            }
            if (modPlayer.ataxiaBolt && modPlayer.canFireAtaxiaRangedProjectile)
            {
                if (item.CountsAsClass<RangedDamageClass>() && !item.channel)
                {
                    modPlayer.canFireAtaxiaRangedProjectile = false;
                    if (player.whoAmI == Main.myPlayer)
                    {
                        int ataxiaFlareDamage = (int)(damage * 0.25);
                        Projectile.NewProjectile(source, position, velocity * 1.25f, ModContent.ProjectileType<HydrothermicFlare>(), ataxiaFlareDamage, 2f, player.whoAmI);
                    }
                }
            }
            if (modPlayer.godSlayerRanged && modPlayer.canFireGodSlayerRangedProjectile)
            {
                if (item.CountsAsClass<RangedDamageClass>() && !item.channel)
                {
                    modPlayer.canFireGodSlayerRangedProjectile = false;
                    if (player.whoAmI == Main.myPlayer)
                    {
                        // God Slayer Ranged Shrapnel: 100%, soft cap starts at 800 base damage
                        int shrapnelRoundDamage = CalamityUtils.DamageSoftCap(damage, 800);
                        Projectile.NewProjectile(source, position, velocity * 1.25f, ModContent.ProjectileType<GodSlayerShrapnelRound>(), shrapnelRoundDamage, 2f, player.whoAmI);
                    }
                }
            }
            if (modPlayer.ataxiaVolley && modPlayer.canFireAtaxiaRogueProjectile)
            {
                if (item.CountsAsClass<ThrowingDamageClass>() && !item.channel)
                {
                    modPlayer.canFireAtaxiaRogueProjectile = false;
                    int flareID = ModContent.ProjectileType<HydrothermicFlareRogue>();

                    // Ataxia Rogue Flares: 8 x 50%, soft cap starts at 200 base damage
                    int flareDamage = CalamityUtils.DamageSoftCap(damage * 0.5, 100);
                    if (player.whoAmI == Main.myPlayer)
                    {
                        SoundEngine.PlaySound(SoundID.Item20, player.Center);
                        float spread = 45f * 0.0174f;
                        double startAngle = Math.Atan2(player.velocity.X, player.velocity.Y) - spread / 2;
                        double deltaAngle = spread / 8f;
                        double offsetAngle;
                        for (int i = 0; i < 4; i++)
                        {
                            offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
                            Projectile.NewProjectile(source, player.Center.X, player.Center.Y, (float)(Math.Sin(offsetAngle) * 5f), (float)(Math.Cos(offsetAngle) * 5f), flareID, flareDamage, 1f, player.whoAmI);
                            Projectile.NewProjectile(source, player.Center.X, player.Center.Y, (float)(-Math.Sin(offsetAngle) * 5f), (float)(-Math.Cos(offsetAngle) * 5f), flareID, flareDamage, 1f, player.whoAmI);
                        }
                    }
                }
            }
            if (modPlayer.victideSet)
            {
                if ((item.CountsAsClass<RangedDamageClass>() || item.CountsAsClass<MeleeDamageClass>() || item.CountsAsClass<MagicDamageClass>() ||
                    item.CountsAsClass<ThrowingDamageClass>() || item.CountsAsClass<SummonDamageClass>()) &&
                    Main.rand.NextBool(10) && !item.channel)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        // Victide All-class Seashells: 200%, soft cap starts at 23 base damage
                        int seashellDamage = CalamityUtils.DamageSoftCap(damage * 2, 46);
                        Projectile.NewProjectile(source, position, velocity * 1.25f, ModContent.ProjectileType<Seashell>(), seashellDamage, 1f, player.whoAmI);
                    }
                }
            }
            if (modPlayer.dynamoStemCells)
            {
                if (item.CountsAsClass<RangedDamageClass>() && Main.rand.NextBool(20) && !item.channel)
                {
                    double damageMult = item.useTime / 30D;
                    if (damageMult < 0.35)
                        damageMult = 0.35;

                    int newDamage = (int)(damage * 2 * damageMult);

                    if (player.whoAmI == Main.myPlayer)
                    {
                        int projectile = Projectile.NewProjectile(source, position, velocity * 1.25f, ModContent.ProjectileType<MiniatureFolly>(), newDamage, 2f, player.whoAmI);
                        if (projectile.WithinBounds(Main.maxProjectiles))
                            Main.projectile[projectile].DamageType = DamageClass.Generic;
                    }
                }
            }
            if (modPlayer.prismaticRegalia)
            {
                if (item.CountsAsClass<MagicDamageClass>() && Main.rand.NextBool(20) && !item.channel)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        for (int i = -5; i <= 5; i += 5)
                        {
                            if (i != 0)
                            {
                                Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(i));
                                int rocket = Projectile.NewProjectile(source, position, perturbedSpeed, ModContent.ProjectileType<MiniRocket>(), (int)(damage * 0.25), 2f, player.whoAmI);
                                if (rocket.WithinBounds(Main.maxProjectiles))
                                    Main.projectile[rocket].DamageType = DamageClass.Generic;
                            }
                        }
                    }
                }
            }
            if (modPlayer.harpyWingBoost && modPlayer.harpyRing)
            {
                if (Main.rand.NextBool(5) && !item.channel)
                {
                    if (player.whoAmI == Main.myPlayer)
                    {
                        float spreadX = velocity.X + Main.rand.NextFloat(-0.75f, 0.75f);
                        float spreadY = velocity.X + Main.rand.NextFloat(-0.75f, 0.75f);
                        int feather = Projectile.NewProjectile(source, position, new Vector2(spreadX, spreadY) * 1.25f, ModContent.ProjectileType<TradewindsProjectile>(), (int)(damage * 0.3), 2f, player.whoAmI);
                        if (feather.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[feather].usesLocalNPCImmunity = true;
                            Main.projectile[feather].localNPCHitCooldown = 10;
                            Main.projectile[feather].DamageType = DamageClass.Generic;
                        }
                    }
                }
            }
            if (item.type == ItemID.StarCannon)
            {
                Vector2 muzzleOffset = velocity.SafeNormalize(Vector2.Zero) * 5f;
                if (Collision.CanHit(position, 0, 0, position + muzzleOffset, 0, 0))
                {
                    position += muzzleOffset;
                }
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<FallenStarProj>(), damage, knockBack, player.whoAmI);
                SoundEngine.PlaySound(SoundID.Item11 with { PitchVariance = 0.05f }, position);
                return false;
            }
            if (item.type == ItemID.PearlwoodBow)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && (Main.projectile[i].type == ModContent.ProjectileType<RainbowFront>() || Main.projectile[i].type == ModContent.ProjectileType<RainbowTrail>()) && Main.projectile[i].owner == player.whoAmI)
                    {
                        Main.projectile[i].Kill();
                    }
                }
                for (int i = -8; i <= 8; i += 8)
                {
                    Vector2 perturbedSpeed = velocity.RotatedBy(MathHelper.ToRadians(i));
                    Projectile.NewProjectile(source, position, perturbedSpeed, ModContent.ProjectileType<RainbowFront>(), damage, 0f, player.whoAmI);
                }
            }
            return true;
        }
        #endregion

        #region Saving And Loading
        public override void SaveData(Item item, TagCompound tag)
        {
            tag.Add("timesUsed", timesUsed);
            tag.Add("charge", Charge);
            tag.Add("enchantmentID", AppliedEnchantment.HasValue ? AppliedEnchantment.Value.ID : 0);
            tag.Add("DischargeEnchantExhaustion", DischargeEnchantExhaustion);
            tag.Add("canFirePointBlankShots", canFirePointBlankShots);
        }

        public override void LoadData(Item item, TagCompound tag)
        {
            canFirePointBlankShots = tag.GetBool("canFirePointBlankShots");
            timesUsed = tag.GetInt("timesUsed");

            // Changed charge from int to float. If an old charge int is present, load that instead.
            if (tag.ContainsKey("Charge"))
                Charge = tag.GetInt("Charge");
            else
                Charge = tag.GetFloat("charge");

            DischargeEnchantExhaustion = tag.GetFloat("DischargeEnchantExhaustion");
            Enchantment? savedEnchantment = EnchantmentManager.FindByID(tag.GetInt("enchantmentID"));
            if (savedEnchantment.HasValue)
            {
                AppliedEnchantment = savedEnchantment.Value;
                bool hasCreationEffect = AppliedEnchantment.Value.CreationEffect != null;
                item.Calamity().AppliedEnchantment.Value.CreationEffect?.Invoke(item);
            }
        }

        public override void NetSend(Item item, BinaryWriter writer)
        {
            BitsByte flags = new BitsByte();
            flags[0] = canFirePointBlankShots;
            // rip, no other flags. what a byte.

            writer.Write(flags);
            writer.Write(timesUsed);
            writer.Write(Charge);
            writer.Write(AppliedEnchantment.HasValue ? AppliedEnchantment.Value.ID : 0);
            writer.Write(DischargeEnchantExhaustion);
        }

        public override void NetReceive(Item item, BinaryReader reader)
        {
            BitsByte flags = reader.ReadByte();
            canFirePointBlankShots = flags[0];

            timesUsed = reader.ReadInt32();
            Charge = reader.ReadSingle();

            Enchantment? savedEnchantment = EnchantmentManager.FindByID(reader.ReadInt32());
            if (savedEnchantment.HasValue)
            {
                AppliedEnchantment = savedEnchantment.Value;
                bool hasCreationEffect = AppliedEnchantment.Value.CreationEffect != null;
                if (hasCreationEffect)
                    item.Calamity().AppliedEnchantment.Value.CreationEffect(item);
            }
            DischargeEnchantExhaustion = reader.ReadSingle();
        }
        #endregion

        #region Pickup Item Changes
        public override bool OnPickup(Item item, Player player)
        {
            if (item.type == ItemID.Heart || item.type == ItemID.CandyApple || item.type == ItemID.CandyCane)
            {
                bool boostedHeart = player.Calamity().photosynthesis;
                if (boostedHeart)
                {
                    player.statLife += 5;
                    if (Main.myPlayer == player.whoAmI)
                        player.HealEffect(5, true);
                }
            }
            return true;
        }
        #endregion

        #region Use Item Changes
        public override bool? UseItem(Item item, Player player)
        {
            if (player.Calamity().evilSmasherBoost > 0)
            {
                if (item.type != ModContent.ItemType<EvilSmasher>())
                    player.Calamity().evilSmasherBoost = 0;
            }

            if (player.HasBuff(BuffID.ParryDamageBuff))
            {
                if (item.type != ItemID.DD2SquireDemonSword)
                {
                    player.parryDamageBuff = false;
                    player.ClearBuff(BuffID.ParryDamageBuff);
                }
            }

            // Give 2 minutes of Honey buff when drinking Bottled Honey.
            if (item.type == ItemID.BottledHoney)
                player.AddBuff(BuffID.Honey, 7200);

            // Moon Lord instantly spawns when Celestial Sigil is used.
            if (item.type == ItemID.CelestialSigil)
            {
                NPC.MoonLordCountdown = 1;
                NetMessage.SendData(MessageID.MoonlordCountdown, -1, -1, null, NPC.MoonLordCountdown);
            }

            return base.UseItem(item, player);
        }

        public override bool AltFunctionUse(Item item, Player player)
        {
            if (player.Calamity().profanedCrystalBuffs && item.pick == 0 && item.axe == 0 && item.hammer == 0 && item.autoReuse && (item.CountsAsClass<ThrowingDamageClass>() || item.CountsAsClass<MagicDamageClass>() || item.CountsAsClass<RangedDamageClass>() || item.CountsAsClass<MeleeDamageClass>()))
            {
                NPC closest = Main.MouseWorld.ClosestNPCAt(1000f, true);
                if (closest != null)
                {
                    player.MinionAttackTargetNPC = closest.whoAmI;
                    player.UpdateMinionTarget();
                }
                return false;
            }
            if (player.ActiveItem().type == ModContent.ItemType<IgneousExaltation>())
            {
                bool hasBlades = false;
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<IgneousBlade>() && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].localAI[1] == 0f)
                    {
                        hasBlades = true;
                        break;
                    }
                }
                if (hasBlades)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        if (Main.projectile[i].ModProjectile is IgneousBlade)
                        {
                            if (Main.projectile[i].ModProjectile<IgneousBlade>().Firing)
                                continue;
                        }
                        if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<IgneousBlade>() && Main.projectile[i].owner == player.whoAmI && Main.projectile[i].localAI[1] == 0f)
                        {
                            Main.projectile[i].rotation = MathHelper.PiOver2 + MathHelper.PiOver4;
                            Main.projectile[i].velocity = Main.projectile[i].SafeDirectionTo(Main.MouseWorld, Vector2.UnitY) * 22f;
                            Main.projectile[i].rotation += Main.projectile[i].velocity.ToRotation();
                            Main.projectile[i].ai[0] = 180f;
                            Main.projectile[i].ModProjectile<IgneousBlade>().Firing = true;
                            Main.projectile[i].tileCollide = true;
                            Main.projectile[i].netUpdate = true;
                        }
                    }
                }
                return false;
            }
            if (player.ActiveItem().type == ModContent.ItemType<VoidConcentrationStaff>() && player.ownedProjectileCounts[ModContent.ProjectileType<VoidConcentrationBlackhole>()] == 0)
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    if (Main.projectile[i].ModProjectile is VoidConcentrationAura)
                    {
                        if (Main.projectile[i].active && Main.projectile[i].owner == player.whoAmI)
                        {
                            Main.projectile[i].ModProjectile<VoidConcentrationAura>().HandleRightClick();
                            break;
                        }
                    }
                }
                return false;
            }
            if (player.ActiveItem().type == ModContent.ItemType<ColdDivinity>())
            {
                bool canContinue = true;
                int count = 0;
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    if (Main.projectile[i].active && Main.projectile[i].type == ModContent.ProjectileType<ColdDivinityPointyThing>() && Main.projectile[i].owner == player.whoAmI)
                    {
                        if (Main.projectile[i].ai[1] > 1f)
                        {
                            canContinue = false;
                            break;
                        }
                        else if (Main.projectile[i].ai[1] == 0f)
                        {
                            if (((ColdDivinityPointyThing)Main.projectile[i].ModProjectile).circlingPlayer)
                                count++;
                        }
                    }
                }
                if (canContinue && count > 0)
                {
                    NPC unluckyTarget = CalamityUtils.MinionHoming(Main.MouseWorld, 1000f, player);
                    if (unluckyTarget != null)
                    {
                        int pointyThingyAmount = count;
                        float angleVariance = MathHelper.TwoPi / pointyThingyAmount;
                        float angle = 0f;

                        var source = player.GetSource_ItemUse(player.ActiveItem());
                        for (int i = 0; i < pointyThingyAmount; i++)
                        {
                            if (Main.projectile.Length == Main.maxProjectiles)
                                break;
                            int coldDivinityDamage = (int)player.GetTotalDamage<SummonDamageClass>().ApplyTo(80);
                            int projj = Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<ColdDivinityPointyThing>(), coldDivinityDamage, 1f, player.whoAmI, angle, 2f);
                            Main.projectile[projj].originalDamage = 80;

                            angle += angleVariance;
                            for (int j = 0; j < 22; j++)
                            {
                                Dust dust = Dust.NewDustDirect(Main.projectile[projj].position, Main.projectile[projj].width, Main.projectile[projj].height, DustID.Ice);
                                dust.velocity = Vector2.UnitY * Main.rand.NextFloat(3f, 5.5f) * Main.rand.NextBool(2).ToDirectionInt();
                                dust.noGravity = true;
                            }
                        }
                    }
                }
                return false;
            }
            return base.AltFunctionUse(item, player);
        }

        public override bool CanUseItem(Item item, Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            CalamityGlobalItem modItem = item.Calamity();

            // Restrict behavior when reading Dreadon's Log.
            if (PopupGUIManager.AnyGUIsActive)
                return false;

            if (player.ownedProjectileCounts[ModContent.ProjectileType<RelicOfDeliveranceSpear>()] > 0 &&
                (item.damage > 0 || item.ammo != AmmoID.None))
            {
                return false; // Don't use weapons if you're charging with a spear
            }

            // Conversion for Andromeda
            if (player.ownedProjectileCounts[ModContent.ProjectileType<GiantIbanRobotOfDoom>()] > 0)
            {
                if (item.type == ItemID.WireKite)
                    return false;
                if (item.pick > 0 || item.axe > 0 || item.hammer > 0 || item.fishingPole > 0)
                    return false;
                // compiler optimization: && short-circuits, so if altFunctionUse != 0, Andromeda code is never called.
                if (item.CountsAsClass<ThrowingDamageClass>() || item.CountsAsClass<MagicDamageClass>() || item.CountsAsClass<RangedDamageClass>() || item.CountsAsClass<MeleeDamageClass>())
                    return player.altFunctionUse == 0 && FlamsteedRing.TransformItemUsage(item, player);
            }

            // Conversion for Profaned Soul Crystal
            ProfanedSoulCrystal.DetermineTransformationEligibility(player);
            if (modPlayer.profanedCrystalBuffs && item.pick == 0 && item.axe == 0 && item.hammer == 0 && item.autoReuse && (item.CountsAsClass<ThrowingDamageClass>() || item.CountsAsClass<MagicDamageClass>() || item.CountsAsClass<RangedDamageClass>() || item.CountsAsClass<MeleeDamageClass>()))
                return player.altFunctionUse == 0 ? ProfanedSoulCrystal.TransformItemUsage(item, player) : AltFunctionUse(item, player);


            //TODO - This souldn't be here!
            if (!item.IsAir)
            {
                // Exhaust the weapon if it has the necessary enchant.
                if (modPlayer.dischargingItemEnchant)
                {
                    float exhaustionCost = item.useTime * 2.25f;
                    if (exhaustionCost < 10f)
                        exhaustionCost = 10f;
                    DischargeEnchantExhaustion = MathHelper.Clamp(DischargeEnchantExhaustion - exhaustionCost, 0.001f, DischargeEnchantExhaustionCap);
                }

                // Otherwise, if it doesn't, clear exhaustion.
                else
                    DischargeEnchantExhaustion = 0;
            }

            // Check for sufficient charge if this item uses charge.
            if (item.type >= ItemID.Count && modItem.UsesCharge)
            {
                // If attempting to use alt fire, and alt fire charge is defined, require that charge. Otherwise require normal charge per use.
                float chargeNeeded = (player.altFunctionUse == 2 && modItem.ChargePerAltUse != -1f) ? modItem.ChargePerAltUse : modItem.ChargePerUse;

                // If the amount of charge needed is zero or less, ignore the charge requirement entirely (e.g. summon staff right click).
                if (chargeNeeded > 0f)
                {
                    if (modItem.Charge < chargeNeeded)
                        return false;

                    // If you have enough charge, decrement charge on the spot because this hook runs exactly once every time you use an item.
                    // Mana has to be checked separately or you'll fail to use the weapon on a mana check later and still have consumed charge.
                    if (player.CheckMana(item) && item.ModItem.CanUseItem(player))
                        Charge -= chargeNeeded;
                }
            }

            // Handle general use-item effects for the Gem Tech Armor.
            player.Calamity().GemTechState.OnItemUseEffects(item);

            if (item.type == ItemID.MonkStaffT1 || CalamityLists.spearAutoreuseList.Contains(item.type))
            {
                return player.ownedProjectileCounts[item.shoot] <= 0;
            }
            if (item.type == ItemID.InvisibilityPotion && player.FindBuffIndex(ModContent.BuffType<ShadowBuff>()) > -1)
            {
                return false;
            }
            if ((item.type == ItemID.RegenerationPotion || item.type == ItemID.LifeforcePotion) && player.FindBuffIndex(ModContent.BuffType<CadancesGrace>()) > -1)
            {
                return false;
            }
            if (item.type == ItemID.WrathPotion && player.FindBuffIndex(ModContent.BuffType<HolyWrathBuff>()) > -1)
            {
                return false;
            }
            if (item.type == ItemID.RagePotion && player.FindBuffIndex(ModContent.BuffType<ProfanedRageBuff>()) > -1)
            {
                return false;
            }
            if ((item.type == ItemID.SuperAbsorbantSponge || item.type == ItemID.EmptyBucket) && modPlayer.ZoneAbyss)
            {
                return false;
            }
            if (item.type == ItemID.RodofDiscord)
            {
                if (player.chaosState)
                    return false;

                Vector2 teleportLocation;
                teleportLocation.X = (float)Main.mouseX + Main.screenPosition.X;
                if (player.gravDir == 1f)
                {
                    teleportLocation.Y = (float)Main.mouseY + Main.screenPosition.Y - (float)player.height;
                }
                else
                {
                    teleportLocation.Y = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY;
                }
                teleportLocation.X -= (float)(player.width / 2);
                if (teleportLocation.X > 50f && teleportLocation.X < (float)(Main.maxTilesX * 16 - 50) && teleportLocation.Y > 50f && teleportLocation.Y < (float)(Main.maxTilesY * 16 - 50))
                {
                    int x = (int)teleportLocation.X / 16;
                    int y = (int)teleportLocation.Y / 16;
                    bool templeCheck = Main.tile[x, y].WallType != WallID.LihzahrdBrickUnsafe || y <= Main.worldSurface || NPC.downedPlantBoss;
                    if (templeCheck && !Collision.SolidCollision(teleportLocation, player.width, player.height))
                    {
                        int duration = CalamityPlayer.chaosStateDuration;
                        player.AddBuff(BuffID.ChaosState, duration, true);
                    }
                }
            }
            if (item.type == ItemID.SuspiciousLookingEye || item.type == ItemID.WormFood || item.type == ItemID.BloodySpine || item.type == ItemID.SlimeCrown || item.type == ItemID.BloodMoonStarter || item.type == ItemID.Abeemination || item.type == ItemID.DeerThing || item.type == ItemID.QueenSlimeCrystal || item.type == ItemID.MechanicalEye || item.type == ItemID.MechanicalWorm || item.type == ItemID.MechanicalSkull || item.type == ItemID.CelestialSigil)
            {
                return !BossRushEvent.BossRushActive;
            }
            return true;
        }
        #endregion

        #region Modify Weapon Damage
        public override void ModifyWeaponDamage(Item item, Player player, ref StatModifier damage)
        {
            // Nerf yoyo glove and bag because it's bad and stupid and dumb and bad.
            if (player.yoyoGlove && ItemID.Sets.Yoyo[item.type])
                damage *= 0.66f;

            if (item.type < ItemID.Count)
                return;

            // Summon weapons specifically do not have their damage affected by charge. They still require charge to function however.
            CalamityGlobalItem modItem = item.Calamity();

            if (!item.CountsAsClass<SummonDamageClass>() && modItem.DischargeEnchantExhaustion > 0f)
                damage *= DischargeEnchantmentDamageFormula();

            if (!item.CountsAsClass<SummonDamageClass>() && (modItem?.UsesCharge ?? false))
            {
                // At exactly zero charge, do not perform any multiplication.
                // This makes charge-using weapons show up at full damage when previewed in crafting, Recipe Browser, etc.
                if (Charge == 0f)
                    return;
                damage *= ChargeDamageFormula();
            }
        }

        internal float DischargeEnchantmentDamageFormula()
        {
            // This exponential has the properties of beginning at 0 and ending at 1, yet also has their signature rising curve.
            // It is therefore perfect for a potential interpolant.
            float interpolant = (float)Math.Pow(2D, DischargeExhaustionRatio) - 1f;

            // No further smoothening is required in the form of a Smoothstep remap.
            // A linear interpolation works fine; the exponential already has the desired curve shape.
            return MathHelper.Lerp(DischargeEnchantMinDamageFactor, DischargeEnchantMaxDamageFactor, interpolant);
        }

        // This formula gives a slightly higher value than 1.0 above 85% charge, and a slightly lower value than 0.0 at 0% charge.
        // Specifically, it gives 0.0 or less at 0.36% charge or lower. This is fine because the result is immediately clamped.
        internal float ChargeDamageFormula()
        {
            float x = MathHelper.Clamp(ChargeRatio, 0f, 1f);
            float y = 1.087f - 0.08f / (x + 0.07f);
            return MathHelper.Clamp(y, 0f, 1f);
        }
        #endregion

        #region Armor Set Changes
        public override string IsArmorSet(Item head, Item body, Item legs)
        {
            string managedArmorSetName = VanillaArmorChangeManager.GetSetBonusName(Main.player[head.playerIndexTheItemIsReservedFor]);
            if (!string.IsNullOrEmpty(managedArmorSetName))
                return managedArmorSetName;

            if (head.type == ItemID.CrystalNinjaHelmet && body.type == ItemID.CrystalNinjaChestplate && legs.type == ItemID.CrystalNinjaLeggings)
                return "CrystalAssassin";
            if (head.type == ItemID.SquireGreatHelm && body.type == ItemID.SquirePlating && legs.type == ItemID.SquireGreaves)
                return "SquireTier2";
            if (head.type == ItemID.HuntressWig && body.type == ItemID.HuntressJerkin && legs.type == ItemID.HuntressPants)
                return "HuntressTier2";
            if (head.type == ItemID.ApprenticeHat && body.type == ItemID.ApprenticeRobe && legs.type == ItemID.ApprenticeTrousers)
                return "ApprenticeTier2";
            if (head.type == ItemID.MonkAltHead && body.type == ItemID.MonkAltShirt && legs.type == ItemID.MonkAltPants)
                return "MonkTier3";
            if (head.type == ItemID.SquireAltHead && body.type == ItemID.SquireAltShirt && legs.type == ItemID.SquireAltPants)
                return "SquireTier3";
            if (head.type == ItemID.HuntressAltHead && body.type == ItemID.HuntressAltShirt && legs.type == ItemID.HuntressAltPants)
                return "HuntressTier3";
            if (head.type == ItemID.ApprenticeAltHead && body.type == ItemID.ApprenticeAltShirt && legs.type == ItemID.ApprenticeAltPants)
                return "ApprenticeTier3";
            if (head.type == ItemID.SpectreHood && body.type == ItemID.SpectreRobe && legs.type == ItemID.SpectrePants)
                return "SpectreHealing";
            if (head.type == ItemID.SolarFlareHelmet && body.type == ItemID.SolarFlareBreastplate && legs.type == ItemID.SolarFlareLeggings)
                return "SolarFlare";
            return "";
        }

        public override void UpdateArmorSet(Player player, string set)
        {
            CalamityPlayer modPlayer = player.Calamity();
            VanillaArmorChangeManager.CreateTooltipManuallyAsNecessary(player);
            VanillaArmorChangeManager.ApplyPotentialEffectsTo(player);

            if (set == "CrystalAssassin")
            {
                player.GetDamage<GenericDamageClass>() -= 0.1f;
                player.GetCritChance<GenericDamageClass>() -= 10;
                player.setBonus = "Allows the ability to dash";
                modPlayer.DashID = string.Empty;
            }
            else if (set == "SquireTier2")
            {
                player.lifeRegen += 3;
                player.GetDamage<SummonDamageClass>() += 0.15f;
                player.GetCritChance<MeleeDamageClass>() += 10;
                player.setBonus += "\nIncreases your life regeneration\n" +
                            "15% increased minion damage and 10% increased melee critical strike chance";
            }
            else if (set == "HuntressTier2")
            {
                player.GetDamage<SummonDamageClass>() += 0.1f;
                player.GetDamage<RangedDamageClass>() += 0.1f;
                player.setBonus += "\n10% increased minion and ranged damage";
            }
            else if (set == "ApprenticeTier2")
            {
                player.GetDamage<SummonDamageClass>() += 0.05f;
                player.GetCritChance<MagicDamageClass>() += 15;
                player.setBonus += "\n5% increased minion damage and 15% increased magic critical strike chance";
            }
            else if (set == "MonkTier3")
            {
                player.GetDamage<SummonDamageClass>() += 0.3f;
                player.GetAttackSpeed<MeleeDamageClass>() += 0.1f;
                player.GetDamage<MeleeDamageClass>() += 0.1f;
                player.GetCritChance<MeleeDamageClass>() += 10;
                player.setBonus += "\n10% increased melee damage, melee critical strike chance and melee speed\n" +
                            "30% increased minion damage";
            }
            else if (set == "SquireTier3")
            {
                player.lifeRegen += 6;
                player.GetDamage<SummonDamageClass>() += 0.1f;
                player.GetCritChance<MeleeDamageClass>() += 10;
                player.setBonus += "\nMassively increased life regeneration\n" +
                            "10% increased minion damage and melee critical strike chance";
            }
            else if (set == "HuntressTier3")
            {
                player.GetDamage<SummonDamageClass>() += 0.1f;
                player.GetDamage<RangedDamageClass>() += 0.1f;
                player.setBonus += "\n10% increased minion and ranged damage";
            }
            else if (set == "ApprenticeTier3")
            {
                player.GetDamage<SummonDamageClass>() += 0.1f;
                player.GetCritChance<MagicDamageClass>() += 15;
                player.setBonus += "\n10% increased minion damage and 15% increased magic critical strike chance";
            }
            else if (set == "SpectreHealing")
            {
                player.GetDamage<MagicDamageClass>() += 0.2f;
                player.setBonus = "Reduces Magic damage by 20% and converts it to healing force\n" +
                    "Magic damage done to enemies heals the player with lowest health";
            }
            else if (set == "SolarFlare")
            {
                if (player.solarShields > 0)
                    modPlayer.DashID = string.Empty;
            }
        }
        #endregion

        #region Equip Changes
        public override void UpdateEquip(Item item, Player player)
        {
            switch (item.type)
            {
                case ItemID.MagicHat:
                    player.GetDamage<MagicDamageClass>() -= 0.01f;
                    player.GetCritChance<MagicDamageClass>() -= 1;
                    break;

                case ItemID.SquireGreatHelm:
                    player.lifeRegen -= 3;
                    break;
                case ItemID.SquirePlating:
                    player.GetDamage<SummonDamageClass>() -= 0.05f;
                    player.GetDamage<MeleeDamageClass>() -= 0.05f;
                    break;
                case ItemID.SquireGreaves:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetCritChance<MeleeDamageClass>() -= 10;
                    break;

                case ItemID.HuntressJerkin:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetDamage<RangedDamageClass>() -= 0.1f;
                    break;

                case ItemID.ApprenticeTrousers:
                    player.GetDamage<SummonDamageClass>() -= 0.05f;
                    player.GetCritChance<MagicDamageClass>() -= 15;
                    break;

                case ItemID.SquireAltShirt:
                    player.lifeRegen -= 6;
                    break;
                case ItemID.SquireAltPants:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetCritChance<MeleeDamageClass>() -= 10;
                    break;

                case ItemID.MonkAltHead:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetDamage<MeleeDamageClass>() -= 0.1f;
                    break;
                case ItemID.MonkAltShirt:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetAttackSpeed<MeleeDamageClass>() -= 0.1f;
                    break;
                case ItemID.MonkAltPants:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetCritChance<MeleeDamageClass>() -= 10;
                    break;

                case ItemID.HuntressAltShirt:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetDamage<RangedDamageClass>() -= 0.1f;
                    break;

                case ItemID.ApprenticeAltPants:
                    player.GetDamage<SummonDamageClass>() -= 0.1f;
                    player.GetCritChance<MagicDamageClass>() -= 15;
                    break;
            }
        }
        #endregion

        #region Accessory Changes
        public override void UpdateAccessory(Item item, Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();

            if (item.prefix > 0)
            {
                float stealthGenBoost = item.Calamity().StealthGenBonus - 1f;
                if (stealthGenBoost > 0)
                {
                    modPlayer.accStealthGenBoost += stealthGenBoost;
                }
            }

            // Obsidian Skull and its upgrades make you immune to On Fire!
            if (item.type == ItemID.ObsidianSkull || item.type == ItemID.ObsidianHorseshoe || item.type == ItemID.ObsidianShield || item.type == ItemID.ObsidianWaterWalkingBoots || item.type == ItemID.LavaWaders || item.type == ItemID.ObsidianSkullRose || item.type == ItemID.MoltenCharm || item.type == ItemID.LavaSkull || item.type == ItemID.MoltenSkullRose || item.type == ItemID.AnkhShield)
                player.buffImmune[BuffID.OnFire] = true;

            // Ankh Shield Mighty Wind immunity.
            if (item.type == ItemID.AnkhShield)
                player.buffImmune[BuffID.WindPushed] = true;

            if (item.type == ItemID.HellfireTreads)
            {
                modPlayer.hellfireTreads = true;
                player.buffImmune[BuffID.OnFire] = true;
            }

            // Nightwither immunity pre-Moon Lord and Holy Flames immunity pre-Profaned Guardians.
            if (item.type == ItemID.MoonStone)
                player.buffImmune[ModContent.BuffType<Nightwither>()] = true;
            if (item.type == ItemID.SunStone)
                player.buffImmune[ModContent.BuffType<HolyFlames>()] = true;
            if (item.type == ItemID.CelestialStone || item.type == ItemID.CelestialShell)
            {
                player.buffImmune[ModContent.BuffType<Nightwither>()] = true;
                player.buffImmune[ModContent.BuffType<HolyFlames>()] = true;
            }

            if (item.type == ItemID.FairyBoots)
                modPlayer.fairyBoots = true;

            // Arcane and Magnet Flower buffs
            if (item.type == ItemID.ArcaneFlower || item.type == ItemID.MagnetFlower)
                player.manaCost -= 0.04f;

            if (item.type == ItemID.SniperScope)
            {
                player.GetDamage<RangedDamageClass>() -= 0.03f;
                player.GetCritChance<RangedDamageClass>() -= 0.03f;
            }

            if (item.type == ItemID.MagicQuiver)
                player.arrowDamage -= 0.05f;

            if (item.type == ItemID.MoltenQuiver)
                player.arrowDamage -= 0.03f;

            if (item.type == ItemID.FireGauntlet)
            {
                player.GetDamage<MeleeDamageClass>() += 0.02f;
                player.GetAttackSpeed<MeleeDamageClass>() += 0.02f;
            }

            if (item.type == ItemID.TerrasparkBoots)
                player.buffImmune[BuffID.OnFire] = true;

            if (item.type == ItemID.AngelWings) // Boost to max life, defense, and life regen
            {
                player.statLifeMax2 += 20;
                player.statDefense += 10;
                player.lifeRegen += 2;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.DemonWings) // Boost to all damage and crit
            {
                player.GetDamage<GenericDamageClass>() += 0.05f;
                player.GetCritChance<GenericDamageClass>() += 5;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.FinWings) // Boosted water abilities, faster fall in water
            {
                player.gills = true;
                player.ignoreWater = true;
                player.noFallDmg = true;
                if (!player.mount.Active)
                {
                    if (Collision.DrownCollision(player.position, player.width, player.height, player.gravDir))
                        player.maxFallSpeed = 12f;
                }
            }
            else if (item.type == ItemID.BeeWings) // Honey buff
            {
                player.AddBuff(BuffID.Honey, 2);
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.ButterflyWings) // Boost to magic stats
            {
                player.statManaMax2 += 20;
                player.GetDamage<MagicDamageClass>() += 0.05f;
                player.manaCost *= 0.95f;
                player.GetCritChance<MagicDamageClass>() += 5;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.FairyWings) // Boost to max life
            {
                player.statLifeMax2 += 60;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.BatWings) // Stronger at night
            {
                player.noFallDmg = true;
                if (!Main.dayTime || Main.eclipse)
                {
                    player.GetDamage<GenericDamageClass>() += 0.07f;
                    player.GetCritChance<GenericDamageClass>() += 3;
                }
            }
            else if (item.type == ItemID.HarpyWings)
            {
                modPlayer.harpyWingBoost = true;
                player.moveSpeed += 0.2f;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.BoneWings) // Bonus to ranged and defense stats while wearing necro armor
            {
                player.noFallDmg = true;
                if ((player.head == ArmorIDs.Head.NecroHelmet || player.head == ArmorIDs.Head.AncientNecroHelmet) &&
                    player.body == ArmorIDs.Body.NecroBreastplate && player.legs == ArmorIDs.Legs.NecroGreaves)
                {
                    player.moveSpeed += 0.1f;
                    player.GetDamage<RangedDamageClass>() += 0.1f;
                    player.GetCritChance<RangedDamageClass>() += 10;
                    player.statDefense += 30;
                }
            }
            else if (item.type == ItemID.MothronWings) // Spawn baby mothrons over time to attack enemies, max of 3
            {
                player.statDefense += 5;
                player.GetDamage<GenericDamageClass>() += 0.05f;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.FrozenWings) // Bonus to melee and ranged stats while wearing frost armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.FrostHelmet && player.body == ArmorIDs.Body.FrostBreastplate && player.legs == ArmorIDs.Legs.FrostLeggings)
                {
                    player.GetDamage<MeleeDamageClass>() += 0.02f;
                    player.GetDamage<RangedDamageClass>() += 0.02f;
                    player.GetCritChance<MeleeDamageClass>() += 1;
                    player.GetCritChance<RangedDamageClass>() += 1;
                }
            }
            else if (item.type == ItemID.FlameWings) // Bonus to melee stats
            {
                player.GetDamage<MeleeDamageClass>() += 0.05f;
                player.GetCritChance<MeleeDamageClass>() += 5;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.GhostWings) // Bonus to mage stats while wearing spectre armor
            {
                player.noFallDmg = true;
                if (player.body == ArmorIDs.Body.SpectreRobe && player.legs == ArmorIDs.Legs.SpectrePants)
                {
                    if (player.head == ArmorIDs.Head.SpectreHood)
                    {
                        player.statDefense += 10;
                        player.endurance += 0.05f;
                    }
                    else if (player.head == ArmorIDs.Head.SpectreMask)
                    {
                        player.GetDamage<MagicDamageClass>() += 0.05f;
                        player.GetCritChance<MagicDamageClass>() += 5;
                    }
                }
            }
            else if (item.type == ItemID.BeetleWings) // Boosted defense and melee stats while wearing beetle armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.BeetleHelmet && player.legs == ArmorIDs.Legs.BeetleLeggings)
                {
                    if (player.body == ArmorIDs.Body.BeetleShell)
                    {
                        player.statDefense += 10;
                        player.endurance += 0.05f;
                    }
                    else if (player.body == ArmorIDs.Body.BeetleScaleMail)
                    {
                        player.GetDamage<MeleeDamageClass>() += 0.05f;
                        player.GetCritChance<MeleeDamageClass>() += 5;
                    }
                }
            }
            else if (item.type == ItemID.Hoverboard) // Boosted ranged stats while wearing shroomite armor
            {
                player.noFallDmg = true;
                if (player.body == ArmorIDs.Body.ShroomiteBreastplate && player.legs == ArmorIDs.Legs.ShroomiteLeggings)
                {
                    if (player.head == ArmorIDs.Head.ShroomiteHeadgear) //arrows
                    {
                        player.arrowDamage += 0.05f;
                    }
                    else if (player.head == ArmorIDs.Head.ShroomiteMask) //bullets
                    {
                        player.bulletDamage += 0.05f;
                    }
                    else if (player.head == ArmorIDs.Head.ShroomiteHelmet) //rockets
                    {
                        player.rocketDamage += 0.05f;
                    }
                    else if (player.Calamity().flamethrowerBoost) //flamethrowers
                    {
                        player.Calamity().hoverboardBoost = true;
                    }
                }
            }
            else if (item.type == ItemID.LeafWings) // Bonus to defensive stats while wearing tiki armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.TikiMask && player.body == ArmorIDs.Body.TikiShirt && player.legs == ArmorIDs.Legs.TikiPants)
                {
                    player.statDefense += 5;
                    player.endurance += 0.05f;
                    player.AddBuff(BuffID.DryadsWard, 5, true); // Dryad's Blessing
                }
            }
            else if (item.type == ItemID.FestiveWings) // Drop powerful homing christmas tree bulbs while in flight
            {
                player.noFallDmg = true;
                player.statLifeMax2 += 40;
                if (modPlayer.icicleCooldown <= 0)
                {
                    var source = player.GetSource_Accessory(item);
                    if (player.controlJump && !player.canJumpAgain_Cloud && player.jump == 0 && player.velocity.Y != 0f && !player.mount.Active && !player.mount.Cart)
                    {
                        int ornamentDamage = (int)player.GetBestClassDamage().ApplyTo(100);
                        int p = Projectile.NewProjectile(source, player.Center, Vector2.UnitY * 2f, ProjectileID.OrnamentFriendly, ornamentDamage, 5f, player.whoAmI);
                        if (p.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[p].DamageType = DamageClass.Generic;
                            Main.projectile[p].Calamity().lineColor = 1;
                            modPlayer.icicleCooldown = 10;
                        }
                    }
                }
            }
            else if (item.type == ItemID.SpookyWings) // Bonus to summon stats while wearing spooky armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.SpookyHelmet && player.body == ArmorIDs.Body.SpookyBreastplate && player.legs == ArmorIDs.Legs.SpookyLeggings)
                {
                    player.GetKnockback(DamageClass.Summon) += 2f;
                    player.GetDamage<SummonDamageClass>() += 0.05f;
                }
            }
            else if (item.type == ItemID.TatteredFairyWings)
            {
                player.GetDamage<GenericDamageClass>() += 0.05f;
                player.GetCritChance<GenericDamageClass>() += 5;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.SteampunkWings)
            {
                player.statDefense += 8;
                player.GetDamage<GenericDamageClass>() += 0.04f;
                player.GetCritChance<GenericDamageClass>() += 2;
                player.moveSpeed += 0.1f;
                player.noFallDmg = true;
            }
            else if (item.type == ItemID.WingsSolar) // Bonus to melee stats while wearing solar flare armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.SolarFlareHelmet && player.body == ArmorIDs.Body.SolarFlareBreastplate && player.legs == ArmorIDs.Legs.SolarFlareLeggings)
                {
                    player.GetDamage<MeleeDamageClass>() += 0.07f;
                    player.GetCritChance<MeleeDamageClass>() += 3;
                }
            }
            else if (item.type == ItemID.WingsVortex) // Bonus to ranged stats while wearing vortex armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.VortexHelmet && player.body == ArmorIDs.Body.VortexBreastplate && player.legs == ArmorIDs.Legs.VortexLeggings)
                {
                    player.GetDamage<RangedDamageClass>() += 0.03f;
                    player.GetCritChance<RangedDamageClass>() += 7;
                }
            }
            else if (item.type == ItemID.WingsNebula) // Bonus to magic stats while wearing nebula armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.NebulaHelmet && player.body == ArmorIDs.Body.NebulaBreastplate && player.legs == ArmorIDs.Legs.NebulaLeggings)
                {
                    player.GetDamage<MagicDamageClass>() += 0.05f;
                    player.GetCritChance<MagicDamageClass>() += 5;
                    player.statManaMax2 += 20;
                    player.manaCost *= 0.95f;
                }
            }
            else if (item.type == ItemID.WingsStardust) // Bonus to summon stats while wearing stardust armor
            {
                player.noFallDmg = true;
                if (player.head == ArmorIDs.Head.StardustHelmet && player.body == ArmorIDs.Body.StardustPlate && player.legs == ArmorIDs.Legs.StardustLeggings)
                    player.GetDamage<SummonDamageClass>() += 0.1f;
            }
            else if (item.type == ItemID.FishronWings || item.type == ItemID.BetsyWings || item.type == ItemID.Yoraiz0rWings ||
                item.type == ItemID.JimsWings || item.type == ItemID.SkiphsWings || item.type == ItemID.LokisWings ||
                item.type == ItemID.ArkhalisWings || item.type == ItemID.LeinforsWings || item.type == ItemID.BejeweledValkyrieWing ||
                item.type == ItemID.RedsWings || item.type == ItemID.DTownsWings || item.type == ItemID.WillsWings ||
                item.type == ItemID.CrownosWings || item.type == ItemID.CenxsWings || item.type == ItemID.CreativeWings ||
                item.type == ItemID.FoodBarbarianWings || item.type == ItemID.GroxTheGreatWings || item.type == ItemID.GhostarsWings ||
                item.type == ItemID.SafemanWings || item.type == ItemID.RainbowWings || item.type == ItemID.LongRainbowTrailWings)
            {
                player.noFallDmg = true;
            }

            if (item.type == ItemID.JellyfishNecklace || item.type == ItemID.JellyfishDivingGear || item.type == ItemID.ArcticDivingGear)
                modPlayer.jellyfishNecklace = true;

            if (item.type == ItemID.FleshKnuckles || item.type == ItemID.BerserkerGlove || item.type == ItemID.HeroShield)
                modPlayer.fleshKnuckles = true;

            if (item.type == ItemID.WormScarf)
                player.endurance -= 0.07f;

            if (item.type == ItemID.RoyalGel)
                modPlayer.royalGel = true;

            if (item.type == ItemID.HandWarmer)
                modPlayer.handWarmer = true;

            if (item.type == ItemID.EoCShield || item.type == ItemID.Tabi || item.type == ItemID.MasterNinjaGear)
                modPlayer.DashID = string.Empty;

            // Hard / Guarding / Armored / Warding give 0.25% / 0.5% / 0.75% / 1% DR
            if (item.prefix == PrefixID.Hard)
            {
                /* Prehardmode = 1
                 * Hardmode = 2
                 * Post-Golem = 2
                 * Post-Moon Lord = 2
                 * Post-Provi = 2
                 * Post-Polter = 3
                 * Post-DoG = 3
                 * Post-Yharon = 4
                 */

                if (DownedBossSystem.downedYharon)
                    player.statDefense += 3;
                else if (DownedBossSystem.downedPolterghast || DownedBossSystem.downedDoG)
                    player.statDefense += 2;
                else if (Main.hardMode || NPC.downedGolemBoss || NPC.downedMoonlord || DownedBossSystem.downedProvidence)
                    player.statDefense += 1;

                player.endurance += 0.0025f;
            }
            if (item.prefix == PrefixID.Guarding)
            {
                /* Prehardmode = 2
                 * Hardmode = 3
                 * Post-Golem = 4
                 * Post-Moon Lord = 4
                 * Post-Provi = 5
                 * Post-Polter = 5
                 * Post-DoG = 6
                 * Post-Yharon = 7
                 */

                if (DownedBossSystem.downedYharon)
                    player.statDefense += 5;
                else if (DownedBossSystem.downedDoG)
                    player.statDefense += 4;
                else if (DownedBossSystem.downedProvidence || DownedBossSystem.downedPolterghast)
                    player.statDefense += 3;
                else if (NPC.downedGolemBoss || NPC.downedMoonlord)
                    player.statDefense += 2;
                else if (Main.hardMode)
                    player.statDefense += 1;

                player.endurance += 0.005f;
            }
            if (item.prefix == PrefixID.Armored)
            {
                /* Prehardmode = 3
                 * Hardmode = 5
                 * Post-Golem = 5
                 * Post-Moon Lord = 6
                 * Post-Provi = 7
                 * Post-Polter = 8
                 * Post-DoG = 9
                 * Post-Yharon = 10
                 */

                if (DownedBossSystem.downedYharon)
                    player.statDefense += 7;
                else if (DownedBossSystem.downedDoG)
                    player.statDefense += 6;
                else if (DownedBossSystem.downedPolterghast)
                    player.statDefense += 5;
                else if (DownedBossSystem.downedProvidence)
                    player.statDefense += 4;
                else if (NPC.downedMoonlord)
                    player.statDefense += 3;
                else if (Main.hardMode || NPC.downedGolemBoss)
                    player.statDefense += 2;

                player.endurance += 0.0075f;
            }
            if (item.prefix == PrefixID.Warding)
            {
                /* Prehardmode = 4
                 * Hardmode = 6
                 * Post-Golem = 7
                 * Post-Moon Lord = 8
                 * Post-Provi = 9
                 * Post-Polter = 10
                 * Post-DoG = 11
                 * Post-Yharon = 12
                 */

                if (DownedBossSystem.downedYharon)
                    player.statDefense += 8;
                else if (DownedBossSystem.downedDoG)
                    player.statDefense += 7;
                else if (DownedBossSystem.downedPolterghast)
                    player.statDefense += 6;
                else if (DownedBossSystem.downedProvidence)
                    player.statDefense += 5;
                else if (NPC.downedMoonlord)
                    player.statDefense += 4;
                else if (NPC.downedGolemBoss)
                    player.statDefense += 3;
                else if (Main.hardMode)
                    player.statDefense += 2;

                player.endurance += 0.01f;
            }
        }
        #endregion

        #region WingChanges
        public override void HorizontalWingSpeeds(Item item, Player player, ref float speed, ref float acceleration)
        {
            CalamityPlayer modPlayer = player.Calamity();
            float moveSpeedBoost = modPlayer.moveSpeedBonus * 0.1f;

            float flightSpeedMult = 1f +
                (modPlayer.soaring ? 0.1f : 0f) +
                (modPlayer.draconicSurge ? 0.1f : 0f) +
                (modPlayer.reaverSpeed ? 0.1f : 0f) +
                moveSpeedBoost;

            float flightAccMult = 1f +
                (modPlayer.draconicSurge ? 0.1f : 0f) +
                moveSpeedBoost;

            flightSpeedMult = MathHelper.Clamp(flightSpeedMult, 0.5f, 1.5f);
            speed *= flightSpeedMult;

            flightAccMult = MathHelper.Clamp(flightAccMult, 0.5f, 1.5f);
            acceleration *= flightAccMult;
        }
        #endregion

        #region GrabChanges
        public override void GrabRange(Item item, Player player, ref int grabRange)
        {
            CalamityPlayer modPlayer = player.Calamity();
            int itemGrabRangeBoost = 0 +
                (modPlayer.reaverExplore ? 20 : 0);

            grabRange += itemGrabRangeBoost;
        }
        #endregion

        #region Ammo
        public override bool CanConsumeAmmo(Item weapon, Item ammo, Player player) => Main.rand.NextFloat() <= player.Calamity().rangedAmmoCost;

        public static bool HasEnoughAmmo(Player player, Item item, int ammoConsumed)
        {
            bool flag = false;
            bool canShoot = false;

            for (int i = 54; i < Main.InventorySlotsTotal; i++)
            {
                if (player.inventory[i].ammo == item.useAmmo && (player.inventory[i].stack >= ammoConsumed || !player.inventory[i].consumable))
                {
                    canShoot = true;
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                for (int j = 0; j < 54; j++)
                {
                    if (player.inventory[j].ammo == item.useAmmo && (player.inventory[j].stack >= ammoConsumed || !player.inventory[j].consumable))
                    {
                        canShoot = true;
                        break;
                    }
                }
            }
            return canShoot;
        }

        public static void ConsumeAdditionalAmmo(Player player, Item item, int ammoConsumed)
        {
            Item itemAmmo = new Item();
            bool flag = false;
            bool dontConsumeAmmo = false;

            for (int i = 54; i < Main.InventorySlotsTotal; i++)
            {
                if (player.inventory[i].ammo == item.useAmmo && (player.inventory[i].stack >= ammoConsumed || !player.inventory[i].consumable))
                {
                    itemAmmo = player.inventory[i];
                    flag = true;
                    break;
                }
            }

            if (!flag)
            {
                for (int j = 0; j < 54; j++)
                {
                    if (player.inventory[j].ammo == item.useAmmo && (player.inventory[j].stack >= ammoConsumed || !player.inventory[j].consumable))
                    {
                        itemAmmo = player.inventory[j];
                        break;
                    }
                }
            }

            if (player.magicQuiver && (item.useAmmo == AmmoID.Arrow || item.useAmmo == AmmoID.Stake) && Main.rand.NextBool(5))
                dontConsumeAmmo = true;
            if (player.huntressAmmoCost90 && Main.rand.NextBool(10))
                dontConsumeAmmo = true;
            if (player.ammoBox && Main.rand.NextBool(5))
                dontConsumeAmmo = true;
            if (player.ammoPotion && Main.rand.NextBool(5))
                dontConsumeAmmo = true;
            if (player.ammoCost80 && Main.rand.NextBool(5))
                dontConsumeAmmo = true;
            if (player.chloroAmmoCost80 && Main.rand.NextBool(5))
                dontConsumeAmmo = true;
            if (player.ammoCost75 && Main.rand.NextBool(4))
                dontConsumeAmmo = true;
            if (Main.rand.NextFloat() > player.Calamity().rangedAmmoCost)
                dontConsumeAmmo = true;

            if (!dontConsumeAmmo && itemAmmo.consumable)
            {
                itemAmmo.stack -= ammoConsumed;
                if (itemAmmo.stack <= 0)
                {
                    itemAmmo.active = false;
                    itemAmmo.TurnToAir();
                }
            }
        }
        #endregion

        #region PostUpdate
        public override void PostUpdate(Item item)
        {
            if (CalamityLists.forceItemList?.Contains(item.type) ?? false)
                CalamityUtils.ForceItemIntoWorld(item);
        }
        #endregion

        #region Inventory Drawing
        internal static ChargingEnergyParticleSet EnchantmentEnergyParticles = new ChargingEnergyParticleSet(-1, 2, Color.DarkViolet, Color.White, 0.04f, 24f);

        internal static void UpdateAllParticleSets()
        {
            EnchantmentEnergyParticles.Update();
        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            // I want to strangle somebody.
            Texture2D itemTexture = TextureAssets.Item[item.type].Value;
            Rectangle itemFrame = (Main.itemAnimations[item.type] == null) ? itemTexture.Frame() : Main.itemAnimations[item.type].GetFrame(itemTexture);

            if (!EnchantmentManager.ItemUpgradeRelationship.ContainsKey(item.type) || !Main.LocalPlayer.InventoryHas(ModContent.ItemType<BrimstoneLocus>()))
                return true;

            // Draw all particles.
            float currentPower = 0f;
            int calamitasNPCIndex = NPC.FindFirstNPC(ModContent.NPCType<WITCH>());
            if (calamitasNPCIndex != -1)
                currentPower = Utils.GetLerpValue(11750f, 1000f, Main.LocalPlayer.Distance(Main.npc[calamitasNPCIndex].Center), true);

            Vector2 particleDrawCenter = position + new Vector2(12f, 16f) * Main.inventoryScale;

            EnchantmentEnergyParticles.InterpolationSpeed = MathHelper.Lerp(0.035f, 0.1f, currentPower);
            EnchantmentEnergyParticles.DrawSet(particleDrawCenter + Main.screenPosition);
            spriteBatch.Draw(itemTexture, position, itemFrame, drawColor, 0f, origin, scale, SpriteEffects.None, 0f);

            return false;
        }
        #endregion

        #region On Create
        public override void OnCreate(Item item, ItemCreationContext context)
        {
			// ChoosePrefix also happens on craft so go reset it here too
			storedPrefix = -1;
        }
        #endregion

        #region Reforge Mechanic Rework
        private static int storedPrefix = -1;
        public override bool PreReforge(Item item)
        {
            StealthGenBonus = 1f;
            StealthStrikePrefixBonus = 0f;
            storedPrefix = item.prefix;
            return true;
        }

        // Ozzatron 31AUG2022: total rework to the reforge rework for mod compatibility
        // you can now disable the rework with config in case it isn't enough to solve your conflicts
        // removed data saved on items; reforging is now a coalescing flowchart that has no RNG
        public override int ChoosePrefix(Item item, UnifiedRandom rand)
        {
			if (storedPrefix == -1 && item.CountsAsClass<RogueDamageClass>() && item.IsCandidateForReforge)
			{
				// Crafting (or first reforge of) a rogue weapon has a 75% chance for a random modifier, this check is done by vanilla
				// Negative modifiers have a 66.66% chance of being voided, Annoying modifier is intentionally ignored by vanilla
				int prefix = CalamityUtils.RandomRoguePrefix();
				bool keepPrefix = !CalamityUtils.NegativeRoguePrefix(prefix) || Main.rand.NextBool(3);
				return keepPrefix ? prefix : 0;
			}

            if (!CalamityConfig.Instance.RemoveReforgeRNG || Main.gameMenu || storedPrefix == -1)
                return -1;

            // Pick a prefix using the new system.
            return CalamityUtils.GetReworkedReforge(item, rand, storedPrefix);
        }

        public override void PostReforge(Item item)
        {
            storedPrefix = -1;
            // Bandit steals 20% of the total price of the reforge if she's around.
            if (NPC.AnyNPCs(ModContent.NPCType<THIEF>()))
            {
                // Calculate the item's reforge cost.
                int value = item.value;
                Player p = Main.LocalPlayer;
                ItemLoader.ReforgePrice(item, ref value, ref p.discount);

                // Steal 20% of that money.
                CalamityWorld.MoneyStolenByBandit += value / 5;

                // Increment the reforge counter to allow the Bandit to refund
                // Also triggers Tinkerer dialogue that hints to the player that money is being stolen
                CalamityWorld.Reforges++;
            }
        }
        #endregion

        #region Money From Rarity
        public static readonly int Rarity0BuyPrice = Item.buyPrice(0, 0, 50, 0);
        public static readonly int Rarity1BuyPrice = Item.buyPrice(0, 1, 0, 0);
        public static readonly int Rarity2BuyPrice = Item.buyPrice(0, 2, 0, 0);
        public static readonly int Rarity3BuyPrice = Item.buyPrice(0, 4, 0, 0);
        public static readonly int Rarity4BuyPrice = Item.buyPrice(0, 12, 0, 0);
        public static readonly int Rarity5BuyPrice = Item.buyPrice(0, 24, 0, 0);
        public static readonly int Rarity6BuyPrice = Item.buyPrice(0, 36, 0, 0);
        public static readonly int Rarity7BuyPrice = Item.buyPrice(0, 48, 0, 0);
        public static readonly int Rarity8BuyPrice = Item.buyPrice(0, 60, 0, 0);
        public static readonly int Rarity9BuyPrice = Item.buyPrice(0, 80, 0, 0);
        public static readonly int Rarity10BuyPrice = Item.buyPrice(1, 0, 0, 0);
        public static readonly int Rarity11BuyPrice = Item.buyPrice(1, 10, 0, 0);
        public static readonly int Rarity12BuyPrice = Item.buyPrice(1, 20, 0, 0);
        public static readonly int Rarity13BuyPrice = Item.buyPrice(1, 30, 0, 0);
        public static readonly int Rarity14BuyPrice = Item.buyPrice(1, 40, 0, 0);
        public static readonly int Rarity15BuyPrice = Item.buyPrice(1, 50, 0, 0);
        public static readonly int Rarity16BuyPrice = Item.buyPrice(2, 0, 0, 0);

        public static readonly int RarityWhiteBuyPrice = Item.buyPrice(0, 0, 50, 0);
        public static readonly int RarityBlueBuyPrice = Item.buyPrice(0, 1, 0, 0);
        public static readonly int RarityGreenBuyPrice = Item.buyPrice(0, 2, 0, 0);
        public static readonly int RarityOrangeBuyPrice = Item.buyPrice(0, 4, 0, 0);
        public static readonly int RarityLightRedBuyPrice = Item.buyPrice(0, 12, 0, 0);
        public static readonly int RarityPinkBuyPrice = Item.buyPrice(0, 24, 0, 0);
        public static readonly int RarityLightPurpleBuyPrice = Item.buyPrice(0, 36, 0, 0);
        public static readonly int RarityLimeBuyPrice = Item.buyPrice(0, 48, 0, 0);
        public static readonly int RarityYellowBuyPrice = Item.buyPrice(0, 60, 0, 0);
        public static readonly int RarityCyanBuyPrice = Item.buyPrice(0, 80, 0, 0);
        public static readonly int RarityRedBuyPrice = Item.buyPrice(1, 0, 0, 0);
        public static readonly int RarityPurpleBuyPrice = Item.buyPrice(1, 10, 0, 0);
        public static readonly int RarityTurquoiseBuyPrice = Item.buyPrice(1, 20, 0, 0);
        public static readonly int RarityPureGreenBuyPrice = Item.buyPrice(1, 30, 0, 0);
        public static readonly int RarityDarkBlueBuyPrice = Item.buyPrice(1, 40, 0, 0);
        public static readonly int RarityVioletBuyPrice = Item.buyPrice(1, 50, 0, 0);
        public static readonly int RarityHotPinkBuyPrice = Item.buyPrice(2, 0, 0, 0);

        public static int GetBuyPrice(int rarity)
        {
            switch (rarity)
            {
                case 0:
					return Rarity0BuyPrice;
                case 1:
					return Rarity1BuyPrice;
                case 2:
					return Rarity2BuyPrice;
                case 3:
					return Rarity3BuyPrice;
                case 4:
					return Rarity4BuyPrice;
                case 5:
					return Rarity5BuyPrice;
                case 6:
					return Rarity6BuyPrice;
                case 7:
					return Rarity7BuyPrice;
                case 8:
					return Rarity8BuyPrice;
                case 9:
					return Rarity9BuyPrice;
                case 10:
					return Rarity10BuyPrice;
                case 11:
					return Rarity11BuyPrice;
            }
			if (rarity == ModContent.RarityType<Turquoise>())
				return RarityTurquoiseBuyPrice;
			if (rarity == ModContent.RarityType<PureGreen>())
				return RarityPureGreenBuyPrice;
			if (rarity == ModContent.RarityType<DarkBlue>())
				return RarityDarkBlueBuyPrice;
			if (rarity == ModContent.RarityType<Violet>())
				return RarityVioletBuyPrice;
			if (rarity == ModContent.RarityType<HotPink>())
				return RarityHotPinkBuyPrice;

			// Return 0 if it's not a progression based or other mod's rarity
			return 0;
        }

        public static int GetBuyPrice(Item item)
        {
            return GetBuyPrice(item.rare);
        }
        #endregion
    }
}
