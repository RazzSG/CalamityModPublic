using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
namespace CalamityMod.Items.Placeables.Furniture.CraftingStations
{
    public class EutrophicCrafting : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
            Tooltip.SetDefault("Used for special crafting");
        }

        public override void SetDefaults()
        {
            Item.SetNameOverride("Eutrophic Shelf");
            Item.width = 28;
            Item.height = 20;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.Furniture.CraftingStations.EutrophicCrafting>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(1).AddIngredient(ModContent.ItemType<Navystone>(), 10).AddIngredient(ModContent.ItemType<SeaPrism>(), 5).AddIngredient(ModContent.ItemType<PrismShard>(), 5).AddTile(TileID.WorkBenches).Register();
        }
    }
}
