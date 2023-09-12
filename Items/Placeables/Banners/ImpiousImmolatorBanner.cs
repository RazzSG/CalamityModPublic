using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Tiles;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Placeables.Banners
{
    public class ImpiousImmolatorBanner : BaseBanner
    {
        public override int BannerTileID => TileType<MonsterBanner>();
        public override int BannerTileStyle => 48;
        public override int BonusNPCID => NPCType<ImpiousImmolator>();
    }
}
