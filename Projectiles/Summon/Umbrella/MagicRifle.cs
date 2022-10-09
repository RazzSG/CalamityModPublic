﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon.Umbrella
{
    public class MagicRifle : ModProjectile
    {
        public VertexStrip TrailDrawer;
		public bool drawTrail = false;
		public ref float SwapSides => ref Projectile.localAI[0];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Rifle");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 40;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 0f;
            Projectile.timeLeft = 180;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
        }

        public override void AI()
        {
            //Set player namespace
            Player player = Main.player[Projectile.owner];

			if (player.Calamity().magicHat)
			{
				Projectile.timeLeft = 2;
			}

			Projectile.ai[0] -= MathHelper.ToRadians(4f);

            float homingRange = MagicHat.Range;
            Vector2 targetVec = Projectile.position;
            int targetIndex = -1;
            //If targeting something, prioritize that enemy
            if (player.HasMinionAttackTargetNPC)
            {
                NPC npc = Main.npc[player.MinionAttackTargetNPC];
                if (npc.CanBeChasedBy(Projectile, false))
                {
                    float extraDist = (npc.width / 2) + (npc.height / 2);
                    //Calculate distance between target and the projectile to know if it's too far or not
                    float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                    if (targetDist < (homingRange + extraDist))
                    {
                        homingRange = targetDist;
                        targetVec = npc.Center;
                        targetIndex = npc.whoAmI;
                    }
                }
            }
            if (targetIndex == -1)
            {
                for (int npcIndex = 0; npcIndex < Main.maxNPCs; npcIndex++)
                {
                    NPC npc = Main.npc[npcIndex];
                    if (npc.CanBeChasedBy(Projectile, false))
                    {
                        float extraDist = (npc.width / 2) + (npc.height / 2);
                        //Calculate distance between target and the projectile to know if it's too far or not
                        float targetDist = Vector2.Distance(npc.Center, Projectile.Center);
                        if (targetDist < (homingRange + extraDist))
                        {
                            homingRange = targetDist;
                            targetVec = npc.Center;
							targetIndex = npc.whoAmI;
                        }
                    }
                }
            }

            //Update rotation and position
            if (targetIndex == -1)
            {
				IdleAI();
            }
			else
			{
				AttackMovement(targetIndex);
			}

            //Increment attack cooldown
            if (Projectile.ai[1] > 0f)
            {
                Projectile.ai[1] += 1f;
            }
            //Set the minion to be ready for attack
            if (Projectile.ai[1] > 45f)
            {
                Projectile.ai[1] = 0f;
                Projectile.netUpdate = true;
            }

            //Return if on attack cooldown, has no target
            if (Projectile.ai[1] != 0f || targetIndex == -1)
                return;

            //Shoot a bullet
            if (Main.myPlayer == Projectile.owner)
            {
                float projSpeed = 6f;
                int projType = ModContent.ProjectileType<MagicBullet>();
                if (Main.rand.NextBool(6))
                {
                    SoundEngine.PlaySound(SoundID.Item20 with { Volume = SoundID.Item20.Volume * 0.1f}, Projectile.position);
                }
                Projectile.ai[1] += 1f;
                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 velocity = targetVec - Projectile.Center;
                    velocity.Normalize();
                    velocity *= projSpeed;
                    SoundEngine.PlaySound(SoundID.Item40, Projectile.position);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, projType, Projectile.damage, 0f, Projectile.owner);
                    Projectile.netUpdate = true;
                }
            }
        }

		private void AttackMovement(int targetIndex)
		{
			Player player = Main.player[Projectile.owner];
			NPC target = Main.npc[targetIndex];

			SwapSides++;

			Vector2 returnPos = Vector2.Zero;
			Vector2 returnPos1 = target.Right + new Vector2(300f, 0f);
			Vector2 returnPos2 = target.Left - new Vector2(300f, 0f);

			returnPos = (SwapSides % 600f < 300f) ? returnPos1 : returnPos2;
			if (player.Center.X - target.Center.X < 0)
				returnPos = (SwapSides % 600f < 300f) ? returnPos2 : returnPos1;

			// Target distance calculations
			Vector2 targetVec = returnPos - Projectile.Center;
			float targetDist = targetVec.Length();

			float targetHomeSpeed = 20f;
			// If more than 60 pixels away, move toward the target
			if (targetDist > 60f)
			{
				targetVec.Normalize();
				targetVec *= targetHomeSpeed;
				Projectile.velocity = (Projectile.velocity * 10f + targetVec) / 11f;
                Projectile.spriteDirection = Projectile.direction = ((returnPos.X - Projectile.Center.X) > 0).ToDirectionInt();
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f) * Projectile.spriteDirection;
				Projectile.ai[1] = 40f;
			}
			else
			{
                Projectile.spriteDirection = Projectile.direction = ((target.Center.X - Projectile.Center.X) > 0).ToDirectionInt();
				float angle = Projectile.AngleTo(target.Center) + (Projectile.spriteDirection == 1 ? MathHelper.ToRadians(-135f) : MathHelper.ToRadians(-45f));
                Projectile.rotation = Projectile.rotation.AngleTowards(angle, 0.1f);
				Projectile.Center = returnPos + new Vector2(0f, ((float)Math.Sin(MathHelper.TwoPi * 0.5f + SwapSides / 50f) * 0.5f + 0.5f) * 40f);
			}
			drawTrail = true;
		}

		private void IdleAI()
		{
			Player player = Main.player[Projectile.owner];

			const float outwardPosition = 180f;
			Vector2 returnPos = player.Center + Projectile.ai[0].ToRotationVector2() * outwardPosition;

			// Player distance calculations
			Vector2 playerVec = returnPos - Projectile.Center;
			float playerDist = playerVec.Length();

			float playerHomeSpeed = 40f;
			// Teleport to the player if abnormally far
			if (playerDist > 2000f)
			{
				Projectile.Center = returnPos;
				Projectile.netUpdate = true;
			}
			// If more than 60 pixels away, move toward the player
			if (playerDist > 60f)
			{
				playerVec.Normalize();
				playerVec *= playerHomeSpeed;
				Projectile.velocity = (Projectile.velocity * 10f + playerVec) / 11f;
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
			}
			else
			{
				Projectile.spriteDirection = Projectile.direction = 0;
				Projectile.rotation = Projectile.ai[0] + MathHelper.PiOver4;
				drawTrail = false;
				Projectile.Center = returnPos;
			}
			SwapSides = 0f;
		}

        public override bool? CanDamage() => false;

        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 10; i++)
            {
                Vector2 dspeed = new Vector2(Main.rand.NextFloat(-7f, 7f), Main.rand.NextFloat(-7f, 7f));
                int dust = Dust.NewDust(Projectile.Center, 1, 1, 66, dspeed.X, dspeed.Y, 160, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 0.75f);
                Main.dust[dust].noGravity = true;
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public Color TrailColorFunction(float completionRatio)
        {
            float opacity = (float)Math.Pow(Utils.GetLerpValue(1f, 0.45f, completionRatio, true), 4D) * Projectile.Opacity * 0.48f;
            return new Color(148, 0, 211) * opacity;
        }

        public float TrailWidthFunction(float completionRatio) => 2f;

        public override bool PreDraw(ref Color lightColor)
        {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Rectangle frame = texture.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
			Vector2 origin = frame.Size() * 0.5f;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
			SpriteEffects direction = Projectile.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			if (drawTrail)
			{
				// Draw the afterimage trail.
				TrailDrawer ??= new();
				GameShaders.Misc["EmpressBlade"].UseShaderSpecificData(new Vector4(1f, 0f, 0f, 0.6f));
				GameShaders.Misc["EmpressBlade"].Apply(null);
				TrailDrawer.PrepareStrip(Projectile.oldPos, Projectile.oldRot, TrailColorFunction, TrailWidthFunction, Projectile.Size * 0.5f - Main.screenPosition, Projectile.oldPos.Length, true);
				TrailDrawer.DrawTrail();
				Main.pixelShader.CurrentTechnique.Passes[0].Apply();

				direction |= SpriteEffects.FlipVertically;
			}

            // Draw the rifle.
			Main.spriteBatch.Draw(texture, drawPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, direction, 0);
            return false;
        }
    }
}
