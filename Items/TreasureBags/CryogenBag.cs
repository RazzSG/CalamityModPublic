
using CalamityMod.World;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Items.Armor.Vanity;

namespace CalamityMod.Items.TreasureBags
{
    public class CryogenBag : ModItem
    {
        public override int BossBagNPC => ModContent.NPCType<Cryogen>();

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Treasure Bag");
            Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}");
        }

        public override void SetDefaults()
        {
            item.maxStack = 999;
            item.consumable = true;
            item.width = 24;
            item.height = 24;
            item.rare = 9;
            item.expert = true;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void OpenBossBag(Player player)
        {
            player.TryGettingDevArmor();

            // Materials
            DropHelper.DropItem(player, ItemID.SoulofMight, 25, 40);
            DropHelper.DropItem(player, ModContent.ItemType<CryoBar>(), 20, 40);
            DropHelper.DropItem(player, ModContent.ItemType<EssenceofEleum>(), 5, 9);
            DropHelper.DropItem(player, ItemID.FrostCore);

            // Weapons
            DropHelper.DropItemChance(player, ModContent.ItemType<Avalanche>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<GlacialCrusher>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<EffluviumBow>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<BittercoldStaff>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<SnowstormStaff>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<Icebreaker>(), 3);
            DropHelper.DropItemChance(player, ModContent.ItemType<IceStar>(), 3, 150, 200);

            // Equipment
            DropHelper.DropItem(player, ModContent.ItemType<SoulofCryogen>());
            DropHelper.DropItemCondition(player, ModContent.ItemType<FrostFlare>(), CalamityWorld.revenge);
            DropHelper.DropItemChance(player, ModContent.ItemType<CryoStone>(), 10);
            DropHelper.DropItemChance(player, ModContent.ItemType<Regenator>(), DropHelper.RareVariantDropRateInt);

            // Vanity
            DropHelper.DropItemChance(player, ModContent.ItemType<CryogenMask>(), 7);

            // Other
            DropHelper.DropItemChance(player, ItemID.FrozenKey, 5);
        }
    }
}
