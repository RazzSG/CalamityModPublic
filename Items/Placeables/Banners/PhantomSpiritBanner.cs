using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Tiles;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Items.Placeables.Banners
{
    public class PhantomSpiritBanner : BaseBanner
    {
        public override int BannerTileID => TileType<MonsterBanner>();
        public override int BannerTileStyle => 84;
        public override int BonusNPCID => NPCType<PhantomSpirit>();
    }
}
