﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace CalamityMod.Projectiles.Ranged
{
    public class FallenStarProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Fallen Star");
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.alpha = 50;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            if (Projectile.soundDelay == 0)
            {
                Projectile.soundDelay = 20 + Main.rand.Next(40);
                SoundEngine.PlaySound(SoundID.Item9, Projectile.Center);
            }
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f;
            }
            Projectile.alpha += (int)(25f * Projectile.localAI[0]);
            if (Projectile.alpha > 200)
            {
                Projectile.alpha = 200;
                Projectile.localAI[0] = -1f;
            }
            if (Projectile.alpha < 50)
            {
                Projectile.alpha = 50;
                Projectile.localAI[0] = 1f;
            }
            Projectile.rotation += (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * 0.01f * Projectile.direction;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            float newdamage = Projectile.damage * 0.9375f;
            Projectile.damage = (int)newdamage;
            if (Projectile.damage < 1)
                Projectile.damage = 1;
        }

        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
            int dustAmt = 10;
            int goreAmt = 6;
            for (int i = 0; i < dustAmt; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 58, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.2f);
            }
            for (int i = 0; i < dustAmt; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 57, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 150, default, 1.2f);
            }
            if (Main.netMode != NetmodeID.Server)
            {
                for (int i = 0; i < goreAmt; i++)
                {
                    Gore.NewGore(Projectile.GetSource_Death(), Projectile.position, Projectile.velocity * 0.05f, Main.rand.Next(16, 18), 1f);
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => new Color(200, 200, 200, lightColor.A - Projectile.alpha);

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.DrawStarTrail(Color.Blue, Color.White);

            //Draw the actual projectile
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 offsets = new Vector2(0f, Projectile.gfxOffY) - Main.screenPosition;
            Color alpha = Projectile.GetAlpha(lightColor);
            Rectangle spriteRec = new Microsoft.Xna.Framework.Rectangle(0, 0, tex.Width, tex.Height);
            Vector2 spriteOrigin = spriteRec.Size() / 2f;
            SpriteEffects spriteEffects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Main.EntitySpriteDraw(tex, Projectile.Center + offsets, spriteRec, alpha, Projectile.rotation, spriteOrigin, Projectile.scale + 0.1f, spriteEffects, 0);
            return false;
        }
    }
}
