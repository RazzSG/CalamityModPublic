using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Tiles;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Placeables.Banners
{
    public class AeroSlimeBanner : BaseBanner
    {
        public override int BannerTileID => TileType<MonsterBanner>();
        public override int BannerTileStyle => 90;
        public override int BonusNPCID => NPCType<AeroSlime>();
    }
}
