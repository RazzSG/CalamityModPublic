using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles
{
    public class AerialiteBarTile : ModTile
    {
        public override void SetDefaults()
        {
            Main.tileShine[Type] = 1000;
            Main.tileSolid[Type] = true;
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);

            dustType = 187;
            drop = ModContent.ItemType<AerialiteBar>();

            AddMapEntry(new Color(141, 232, 246));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            if (Main.rand.NextBool(2))
            {
                type = 187;
            }
            else
            {
                type = 16;
            }
            return true;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            WorldGen.Check1x1(i, j, Type);
            return true;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Main.tile[i, j].frameX = 0;
            Main.tile[i, j].frameY = 0;
        }
    }
}
