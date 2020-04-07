using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.StatDebuffs
{
    public class DeathModeHot : ModBuff
    {
        public override void SetDefaults()
        {
            DisplayName.SetDefault("Heat Exhaustion");
            Description.SetDefault("The overwhelming heat weakens your bodily functions. You need to look for equipment to protect you from the heat.");
            Main.buffNoTimeDisplay[Type] = true;
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
            longerExpertDebuff = false;
        }
    }
}
