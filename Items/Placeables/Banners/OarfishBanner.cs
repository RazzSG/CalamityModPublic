using CalamityMod.NPCs.Abyss;
using CalamityMod.Tiles;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Placeables.Banners
{
    public class OarfishBanner : BaseBanner
    {
        public override int BannerTileID => TileType<MonsterBanner>();
        public override int BannerTileStyle => 22;
        public override int BonusNPCID => NPCType<OarfishHead>();
    }
}
