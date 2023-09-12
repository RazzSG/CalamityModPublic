using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Tiles;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Placeables.Banners
{
    public class SeaMinnowBanner : BaseBanner
    {
        public override int BannerTileID => TileType<MonsterBanner>();
        public override int BannerTileStyle => 105;
        public override int BonusNPCID => NPCType<SeaMinnow>();
    }
}
