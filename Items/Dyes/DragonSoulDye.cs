using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework;

namespace CalamityMod.Items.Dyes
{
    public class DragonSoulDye : BaseDye
    {
        public override ArmorShaderData ShaderDataToBind => new ArmorShaderData(new Ref<Effect>(mod.GetEffect("Effects/Dyes/DragonSoulDyeShader")), "DyePass");
        public override void SafeSetStaticDefaults()
        {
            DisplayName.SetDefault("Dragon Soul Dye");
        }

		public override void SafeSetDefaults()
		{
            item.rare = 10;
            item.Calamity().customRarity = CalamityRarity.Violet;
            item.value = Item.sellPrice(0, 8, 0, 0);
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.BottledWater);
            recipe.AddIngredient(ModContent.ItemType<HellcasterFragment>());
            recipe.AddTile(TileID.DyeVat);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
    }
}