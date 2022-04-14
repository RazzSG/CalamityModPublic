using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
namespace CalamityMod.Items.Placeables.FurnitureCosmilite
{
    public class CosmilitePiano : ModItem
    {
        public override void SetStaticDefaults()
        {
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.FurnitureCosmilite.CosmilitePiano>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(1).AddIngredient(ItemID.Bone, 4).AddIngredient(ModContent.ItemType<CosmiliteBrick>(), 15).AddIngredient(ItemID.Book).AddTile(ModContent.TileType<CosmicAnvil>()).Register();
        }
    }
}
