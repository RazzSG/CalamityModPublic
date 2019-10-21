
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Tiles.FurnitureEutrophic
{
    public class SmoothNavystone : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            TileMerge.MergeGeneralTiles(Type);
            TileMerge.MergeSmoothTiles(Type);
            TileMerge.MergeDecorativeTiles(Type);
            TileMerge.MergeDesertTiles(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            soundType = 21;
            minPick = 55;
            drop = ModContent.ItemType<Items.Placeables.FurnitureEutrophic.SmoothNavystone>();
            AddMapEntry(new Color(39, 48, 53));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, 51, 0f, 0f, 1, new Color(54, 69, 72), 1f);
            return false;
        }
    }
}
