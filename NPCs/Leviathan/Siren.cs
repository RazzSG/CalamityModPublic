﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.World;

namespace CalamityMod.NPCs.Leviathan
{
	[AutoloadBossHead]
	public class Siren : ModNPC
	{
		private bool spawnedLevi = false;
		private bool secondClone = false;
		private int phaseSwitch = 0;
		private int chargeSwitch = 0;
		private int frameUsed = 0;
		private double anotherFloat = 0.0;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("The Siren");
			Main.npcFrameCount[npc.type] = 6;
		}

		public override void SetDefaults()
		{
			npc.damage = 70; //150
			npc.npcSlots = 16f;
			npc.width = 100; //324
			npc.height = 100; //216
			npc.defense = 25;
			npc.lifeMax = CalamityWorld.revenge ? 41600 : 27400;
			if (CalamityWorld.death)
			{
				npc.lifeMax = 58650;
			}
			if (CalamityWorld.bossRushActive)
			{
				npc.lifeMax = CalamityWorld.death ? 2800000 : 2600000;
			}
			double HPBoost = (double)Config.BossHealthPercentageBoost * 0.01;
			npc.lifeMax += (int)((double)npc.lifeMax * HPBoost);
			npc.knockBackResist = 0f;
			npc.aiStyle = -1; //new
			aiType = -1; //new
			npc.boss = true;
			npc.value = Item.buyPrice(0, 15, 0, 0);
			for (int k = 0; k < npc.buffImmune.Length; k++)
			{
				npc.buffImmune[k] = true;
			}
			npc.buffImmune[BuffID.Ichor] = false;
			npc.buffImmune[mod.BuffType("MarkedforDeath")] = false;
			npc.buffImmune[BuffID.CursedInferno] = false;
			npc.buffImmune[BuffID.Daybreak] = false;
			npc.buffImmune[mod.BuffType("AbyssalFlames")] = false;
			npc.buffImmune[mod.BuffType("ArmorCrunch")] = false;
			npc.buffImmune[mod.BuffType("DemonFlames")] = false;
			npc.buffImmune[mod.BuffType("GodSlayerInferno")] = false;
			npc.buffImmune[mod.BuffType("HolyLight")] = false;
			npc.buffImmune[mod.BuffType("Nightwither")] = false;
			npc.buffImmune[mod.BuffType("Plague")] = false;
			npc.buffImmune[mod.BuffType("Shred")] = false;
			npc.buffImmune[mod.BuffType("WhisperingDeath")] = false;
			npc.buffImmune[mod.BuffType("SilvaStun")] = false;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.HitSound = SoundID.NPCHit1;
			npc.DeathSound = SoundID.NPCDeath1;
			Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
			if (calamityModMusic != null)
				music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/Siren");
			else
				music = MusicID.Boss3;
			bossBag = mod.ItemType("LeviathanBag");
		}

		public override void SendExtraAI(BinaryWriter writer)
		{
			writer.Write(spawnedLevi);
			writer.Write(secondClone);
			writer.Write(phaseSwitch);
			writer.Write(chargeSwitch);
			writer.Write(anotherFloat);
			writer.Write(frameUsed);
			writer.Write(npc.dontTakeDamage);
		}

		public override void ReceiveExtraAI(BinaryReader reader)
		{
			spawnedLevi = reader.ReadBoolean();
			secondClone = reader.ReadBoolean();
			phaseSwitch = reader.ReadInt32();
			chargeSwitch = reader.ReadInt32();
			anotherFloat = reader.ReadDouble();
			frameUsed = reader.ReadInt32();
			npc.dontTakeDamage = reader.ReadBoolean();
		}

		public override void AI()
		{
			CalamityGlobalNPC.siren = npc.whoAmI;
			Lighting.AddLight((int)((npc.position.X + (float)(npc.width / 2)) / 16f), (int)((npc.position.Y + (float)(npc.height / 2)) / 16f), 0f, 0.5f, 0.3f);
			Player player = Main.player[npc.target];
			bool revenge = (CalamityWorld.revenge || CalamityWorld.bossRushActive);
			bool expertMode = (Main.expertMode || CalamityWorld.bossRushActive);
			bool playerWet = player.wet;
			float num998 = 8f;
			float scaleFactor3 = 300f;
			float num999 = 800f;
			float num1001 = 5f;
			float scaleFactor4 = 0.8f;
			int num1002 = 0;
			float scaleFactor5 = 10f;
			float num1003 = 30f;
			float num1004 = 150f;
			float num1006 = 0.333333343f;
			float num1007 = 8f;
			Vector2 vector = npc.Center;
			Vector2 spawnAt = npc.Center + new Vector2(0f, (float)npc.height / 2f);
			bool isNotOcean = player.position.Y < 800f || (double)player.position.Y > Main.worldSurface * 16.0 || (player.position.X > 6400f && player.position.X < (float)(Main.maxTilesX * 16 - 6400));
			bool halfLife = (double)npc.life <= (double)npc.lifeMax * 0.5;
			bool leviAlive = false;
			if (CalamityGlobalNPC.leviathan != -1)
			{
				leviAlive = Main.npc[CalamityGlobalNPC.leviathan].active;
			}
			float num1000 = leviAlive ? 14f : 16f;
			float num1005 = leviAlive ? 14f : 16f;
			float chargeSpeedDivisor = leviAlive ? 13.85f : 15.85f;
			num1006 *= num1005;
			if ((halfLife || CalamityWorld.death || CalamityWorld.bossRushActive) && Main.netMode != 1)
			{
				if (!spawnedLevi)
				{
					if (revenge)
					{
						NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y - 200, mod.NPCType("SirenClone"));
					}
					Mod calamityModMusic = ModLoader.GetMod("CalamityModMusic");
					if (calamityModMusic != null)
						music = calamityModMusic.GetSoundSlot(SoundType.Music, "Sounds/Music/LeviathanAndSiren");
					else
						music = MusicID.Boss3;
					NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("Leviathan"));
					spawnedLevi = true;
				}
			}
			double defenseMult = halfLife ? 2.0 : 1.5;
			if ((!leviAlive && halfLife) || CalamityWorld.death)
			{
				npc.defense = (int)(25.0 * defenseMult);
			}
			else
			{
				npc.defense = 25;
			}
			if ((double)npc.life <= (double)npc.lifeMax * 0.25 && Main.netMode != 1)
			{
				if (!secondClone)
				{
					if (revenge)
					{
						NPC.NewNPC((int)spawnAt.X, (int)spawnAt.Y - 200, mod.NPCType("SirenClone"));
					}
					secondClone = true;
				}
			}
			if (npc.ai[3] == 0f && npc.localAI[1] == 0f && Main.netMode != 1)
			{
				int num6 = NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y, mod.NPCType("SirenIce"), npc.whoAmI, 0f, 0f, 0f, 0f, 255);
				npc.ai[3] = (float)(num6 + 1);
				npc.localAI[1] = -1f;
				npc.netUpdate = true;
				Main.npc[num6].ai[0] = (float)npc.whoAmI;
				Main.npc[num6].netUpdate = true;
			}
			int num7 = (int)npc.ai[3] - 1;
			if (num7 != -1 && Main.npc[num7].active && Main.npc[num7].type == mod.NPCType("SirenIce"))
			{
				npc.dontTakeDamage = true;
			}
			else
			{
				npc.dontTakeDamage = isNotOcean && !CalamityWorld.bossRushActive;
				npc.ai[3] = 0f;
				if (npc.localAI[1] == -1f)
				{
					npc.localAI[1] = revenge ? 600f : 1200f;
				}
				if (npc.localAI[1] > 0f)
				{
					npc.localAI[1] -= 1f;
				}
			}
			if (isNotOcean)
			{
				npc.alpha += 3;
				if (npc.alpha >= 150)
				{
					npc.alpha = 150;
				}
			}
			else
			{
				npc.alpha -= 5;
				if (npc.alpha <= 0)
				{
					npc.alpha = 0;
				}
			}
			if (Main.rand.Next(300) == 0)
			{
				Main.PlaySound(29, (int)npc.position.X, (int)npc.position.Y, 35);
			}
			int num1038 = 0;
			for (int num1039 = 0; num1039 < 255; num1039++)
			{
				if (Main.player[num1039].active && !Main.player[num1039].dead && (npc.Center - Main.player[num1039].Center).Length() < 1000f)
				{
					num1038++;
				}
			}
			if (npc.timeLeft < 3000)
			{
				npc.timeLeft = 3000;
			}
			if (npc.target < 0 || npc.target == 255 || player.dead || !player.active)
			{
				npc.TargetClosest(true);
			}
			else if (npc.ai[0] == -1f)
			{
				int random = ((double)npc.life <= (double)npc.lifeMax * 0.5) ? 3 : 2;
				int num871 = Main.rand.Next(random);
				if (num871 == 0)
				{
					num871 = 0;
				}
				else if (num871 == 1)
				{
					num871 = 2;
				}
				else
				{
					num871 = 3;
				}
				npc.ai[0] = (float)num871;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;
				return;
			}
			else if (npc.ai[0] == 0f)
			{
				npc.TargetClosest(true);
				npc.rotation = npc.velocity.X * 0.02f;
				npc.spriteDirection = npc.direction;
				float num1053 = 9f;
				float num1054 = 0.15f;
				float num10542 = 0.05f;
				Vector2 vector118 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
				float num1055 = player.position.X + (float)(player.width / 2) - vector118.X;
				float num1056 = player.position.Y + (float)(player.height / 2) - 200f - vector118.Y;
				float num1057 = (float)Math.Sqrt((double)(num1055 * num1055 + num1056 * num1056));
				if (num1057 < 600f)
				{
					npc.ai[0] = 1f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
					return;
				}
				num1057 = num1053 / num1057;
				if (npc.velocity.X < num1055)
				{
					npc.velocity.X = npc.velocity.X + num1054;
					if (npc.velocity.X < 0f && num1055 > 0f)
					{
						npc.velocity.X = npc.velocity.X + num1054;
					}
				}
				else if (npc.velocity.X > num1055)
				{
					npc.velocity.X = npc.velocity.X - num1054;
					if (npc.velocity.X > 0f && num1055 < 0f)
					{
						npc.velocity.X = npc.velocity.X - num1054;
					}
				}
				if (npc.velocity.Y < num1056)
				{
					npc.velocity.Y = npc.velocity.Y + num10542;
					if (npc.velocity.Y < 0f && num1056 > 0f)
					{
						npc.velocity.Y = npc.velocity.Y + num10542;
					}
				}
				else if (npc.velocity.Y > num1056)
				{
					npc.velocity.Y = npc.velocity.Y - num10542;
					if (npc.velocity.Y > 0f && num1056 < 0f)
					{
						npc.velocity.Y = npc.velocity.Y - num10542;
					}
				}
			}
			else if (npc.ai[0] == 1f)
			{
				npc.rotation = npc.velocity.X * 0.02f;
				npc.TargetClosest(true);
				Vector2 vector119 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.8f);
				Vector2 vector120 = new Vector2(npc.position.X + (float)npc.width * 0.5f, npc.position.Y + (float)npc.height * 0.5f);
				float num1058 = player.position.X + (float)(player.width / 2) - vector120.X;
				float num1059 = player.position.Y + (float)(player.height / 2) - vector120.Y;
				float num1060 = (float)Math.Sqrt((double)(num1058 * num1058 + num1059 * num1059));
				npc.ai[1] += 1f;
				npc.ai[1] += (float)(num1038 / 2);
				if ((double)npc.life < (double)npc.lifeMax * 0.5 || CalamityWorld.bossRushActive)
				{
					npc.ai[1] += 0.25f;
				}
				if ((double)npc.life < (double)npc.lifeMax * 0.1 || CalamityWorld.bossRushActive)
				{
					npc.ai[1] += 0.25f;
				}
				bool flag103 = false;
				if (npc.ai[1] > 20f)
				{
					npc.ai[1] = 0f;
					npc.ai[2] += 1f;
					flag103 = true;
				}
				if (Collision.CanHit(vector119, 1, 1, player.position, player.width, player.height) && flag103)
				{
					Main.PlaySound(2, (int)npc.position.X, (int)npc.position.Y, 85);
					if (Main.netMode != 1)
					{
						int num1061 = 371;
						int num1062 = NPC.NewNPC((int)vector119.X, (int)vector119.Y - 30, num1061, 0, 0f, 0f, 0f, 0f, 255);
						Main.npc[num1062].velocity.X = (float)Main.rand.Next(-200, 201) * 0.01f;
						Main.npc[num1062].velocity.Y = (float)Main.rand.Next(-200, 201) * 0.01f;
						Main.npc[num1062].localAI[0] = 60f;
						Main.npc[num1062].netUpdate = true;
						Main.npc[num1062].damage = leviAlive ? 100 : 140;
					}
				}
				if (num1060 > 600f || !Collision.CanHit(new Vector2(vector119.X, vector119.Y - 30f), 1, 1, player.position, player.width, player.height))
				{
					float num1063 = 14f;
					float num1064 = 0.15f;
					float num10642 = 0.05f;
					vector120 = vector119;
					num1058 = player.position.X + (float)(player.width / 2) - vector120.X;
					num1059 = player.position.Y + (float)(player.height / 2) - vector120.Y;
					num1060 = (float)Math.Sqrt((double)(num1058 * num1058 + num1059 * num1059));
					num1060 = num1063 / num1060;
					if (npc.velocity.X < num1058)
					{
						npc.velocity.X = npc.velocity.X + num1064;
						if (npc.velocity.X < 0f && num1058 > 0f)
						{
							npc.velocity.X = npc.velocity.X + num1064;
						}
					}
					else if (npc.velocity.X > num1058)
					{
						npc.velocity.X = npc.velocity.X - num1064;
						if (npc.velocity.X > 0f && num1058 < 0f)
						{
							npc.velocity.X = npc.velocity.X - num1064;
						}
					}
					if (npc.velocity.Y < num1059)
					{
						npc.velocity.Y = npc.velocity.Y + num10642;
						if (npc.velocity.Y < 0f && num1059 > 0f)
						{
							npc.velocity.Y = npc.velocity.Y + num10642;
						}
					}
					else if (npc.velocity.Y > num1059)
					{
						npc.velocity.Y = npc.velocity.Y - num10642;
						if (npc.velocity.Y > 0f && num1059 < 0f)
						{
							npc.velocity.Y = npc.velocity.Y - num10642;
						}
					}
				}
				else
				{
					npc.velocity *= 0.9f;
				}
				npc.spriteDirection = npc.direction;
				if (npc.ai[2] > 4f)
				{
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
				}
			}
			else if (npc.ai[0] == 2f)
			{
				npc.rotation = npc.velocity.X * 0.02f;
				Vector2 vector121 = new Vector2(npc.position.X + (float)(npc.width / 2) + (float)(Main.rand.Next(20) * npc.direction), npc.position.Y + (float)npc.height * 0.4f);
				npc.ai[1] += 1f;
				bool flag104 = false;
				if (npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged || (Config.BossRushXerocCurse && CalamityWorld.bossRushActive))
				{
					if (npc.ai[1] % 10f == 9f)
					{
						flag104 = true;
					}
				}
				else
				{
					if (((double)npc.life < (double)npc.lifeMax * 0.1 || CalamityWorld.death) && !leviAlive)
					{
						if (npc.ai[1] % 20f == 19f)
						{
							flag104 = true;
						}
					}
					else if (halfLife)
					{
						if (npc.ai[1] % 25f == 24f)
						{
							flag104 = true;
						}
					}
					else if (npc.ai[1] % 30f == 29f)
					{
						flag104 = true;
					}
				}
				if (flag104 && (npc.position.Y + (float)npc.height < player.position.Y || (!leviAlive && halfLife)) && Collision.CanHit(vector121, 1, 1, player.position, player.width, player.height))
				{
					if (Main.netMode != 1)
					{
						float num1070 = revenge ? 13f : 11f;
						if (npc.GetGlobalNPC<CalamityGlobalNPC>(mod).enraged || (Config.BossRushXerocCurse && CalamityWorld.bossRushActive))
						{
							num1070 = 24f;
						}
						else if (isNotOcean || (!leviAlive && halfLife) || CalamityWorld.death || CalamityWorld.bossRushActive)
						{
							num1070 = revenge ? 19f : 18f;
						}
						else if (!playerWet)
						{
							num1070 = revenge ? 16f : 15f;
						}
						else
						{
							if ((double)npc.life < (double)npc.lifeMax * 0.1)
							{
								num1070 += 2f;
							}
						}
						float num1071 = player.position.X + (float)player.width * 0.5f - vector121.X + (float)Main.rand.Next(-80, 81);
						float num1072 = player.position.Y + (float)player.height * 0.5f - vector121.Y + (float)Main.rand.Next(-40, 41);
						float num1073 = (float)Math.Sqrt((double)(num1071 * num1071 + num1072 * num1072));
						num1073 = num1070 / num1073;
						num1071 *= num1073;
						num1072 *= num1073;
						int num1074 = expertMode ? 26 : 32;
						int num1075 = mod.ProjectileType("WaterSpear");
						switch (Main.rand.Next(6))
						{
							case 0: num1075 = mod.ProjectileType("SirenSong"); break;
							case 1: num1075 = mod.ProjectileType("FrostMist"); break;
							case 2:
							case 3:
							case 4:
							case 5: num1075 = mod.ProjectileType("WaterSpear"); break;
						}
						if (isNotOcean)
						{
							num1074 *= 2;
						}
						Projectile.NewProjectile(vector121.X, vector121.Y, num1071, num1072, num1075, num1074, 0f, Main.myPlayer, 0f, 0f);
					}
				}
				if (npc.position.Y > player.position.Y - 200f) //200
				{
					if (npc.velocity.Y > 0f)
					{
						npc.velocity.Y = npc.velocity.Y * 0.985f;
					}
					npc.velocity.Y = npc.velocity.Y - 0.1f;
					if (npc.velocity.Y > 2f)
					{
						npc.velocity.Y = 2f;
					}
				}
				else if (npc.position.Y < player.position.Y - 500f) //500
				{
					if (npc.velocity.Y < 0f)
					{
						npc.velocity.Y = npc.velocity.Y * 0.985f;
					}
					npc.velocity.Y = npc.velocity.Y + 0.1f;
					if (npc.velocity.Y < -2f)
					{
						npc.velocity.Y = -2f;
					}
				}
				if (npc.position.X + (float)(npc.width / 2) > player.position.X + (float)(player.width / 2) + 100f)
				{
					if (npc.velocity.X > 0f)
					{
						npc.velocity.X = npc.velocity.X * 0.98f;
					}
					npc.velocity.X = npc.velocity.X - 0.1f;
					if (npc.velocity.X > 8f)
					{
						npc.velocity.X = 8f;
					}
				}
				if (npc.position.X + (float)(npc.width / 2) < player.position.X + (float)(player.width / 2) - 100f)
				{
					if (npc.velocity.X < 0f)
					{
						npc.velocity.X = npc.velocity.X * 0.98f;
					}
					npc.velocity.X = npc.velocity.X + 0.1f;
					if (npc.velocity.X < -8f)
					{
						npc.velocity.X = -8f;
					}
				}
				float playerLocation = npc.Center.X - player.Center.X;
				npc.direction = (playerLocation < 0 ? 1 : -1);
				npc.spriteDirection = npc.direction;
				if (npc.ai[1] > 300f)
				{
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
				}
			}
			else if (npc.ai[0] == 3f)
			{
				npc.TargetClosest(false);
				npc.rotation = npc.velocity.ToRotation();
				if (Math.Sign(npc.velocity.X) != 0)
				{
					npc.spriteDirection = -Math.Sign(npc.velocity.X);
				}
				if (npc.rotation < -1.57079637f)
				{
					npc.rotation += 3.14159274f;
				}
				if (npc.rotation > 1.57079637f)
				{
					npc.rotation -= 3.14159274f;
				}
				npc.spriteDirection = Math.Sign(npc.velocity.X);
				phaseSwitch += 1;
				if (CalamityWorld.death || CalamityWorld.bossRushActive)
				{
					phaseSwitch += 1;
				}
				if (chargeSwitch == 0) //line up the charge
				{
					float scaleFactor6 = num998;
					Vector2 center4 = npc.Center;
					Vector2 center5 = player.Center;
					Vector2 vector126 = center5 - center4;
					Vector2 vector127 = vector126 - Vector2.UnitY * scaleFactor3;
					float num1013 = vector126.Length();
					vector126 = Vector2.Normalize(vector126) * scaleFactor6;
					vector127 = Vector2.Normalize(vector127) * scaleFactor6;
					bool flag64 = Collision.CanHit(npc.Center, 1, 1, player.Center, 1, 1);
					if (anotherFloat >= 120.0)
					{
						flag64 = true;
					}
					float num1014 = 8f;
					flag64 = (flag64 && vector126.ToRotation() > 3.14159274f / num1014 && vector126.ToRotation() < 3.14159274f - 3.14159274f / num1014);
					if (num1013 > num999 || !flag64)
					{
						npc.velocity.X = (npc.velocity.X * (num1000 - 1f) + vector127.X) / chargeSpeedDivisor;
						npc.velocity.Y = (npc.velocity.Y * (num1000 - 1f) + vector127.Y) / chargeSpeedDivisor;
						if (!flag64)
						{
							anotherFloat += 1.0;
							if (anotherFloat == 120.0)
							{
								npc.netUpdate = true;
							}
						}
						else
						{
							anotherFloat = 0.0;
						}
					}
					else
					{
						chargeSwitch = 1;
						npc.ai[2] = vector126.X;
						anotherFloat = (double)vector126.Y;
						npc.netUpdate = true;
					}
				}
				else if (chargeSwitch == 1) //pause before charging
				{
					npc.velocity *= scaleFactor4;
					npc.ai[1] += 1f;
					if (npc.ai[1] >= num1001)
					{
						chargeSwitch = 2;
						npc.ai[1] = 0f;
						npc.netUpdate = true;
						Vector2 velocity = new Vector2(npc.ai[2], (float)anotherFloat) + new Vector2((float)Main.rand.Next(-num1002, num1002 + 1), (float)Main.rand.Next(-num1002, num1002 + 1)) * 0.04f;
						velocity.Normalize();
						velocity *= scaleFactor5;
						npc.velocity = velocity;
					}
				}
				else if (chargeSwitch == 2) //charging
				{
					float num1016 = num1003;
					npc.ai[1] += 1f;
					bool flag65 = Vector2.Distance(npc.Center, player.Center) > num1004 && npc.Center.Y > player.Center.Y;
					if ((npc.ai[1] >= num1016 && flag65) || npc.velocity.Length() < num1007)
					{
						chargeSwitch = 0;
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						anotherFloat = 0.0;
						npc.velocity /= 2f;
						npc.netUpdate = true;
						npc.ai[1] = 45f;
						chargeSwitch = 3;
					}
					else
					{
						Vector2 center6 = npc.Center;
						Vector2 center7 = player.Center;
						Vector2 vec2 = center7 - center6;
						vec2.Normalize();
						if (vec2.HasNaNs())
						{
							vec2 = new Vector2((float)npc.direction, 0f);
						}
						npc.velocity = (npc.velocity * (num1005 - 1f) + vec2 * (npc.velocity.Length() + num1006)) / num1005;
					}
				}
				else if (chargeSwitch == 3) //slow down after charging and reset
				{
					npc.ai[1] -= 1f;
					if (npc.ai[1] <= 0f)
					{
						chargeSwitch = 0;
						npc.ai[1] = 0f;
						npc.netUpdate = true;
					}
					npc.velocity *= 0.98f;
				}
				if (phaseSwitch > 300)
				{
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					anotherFloat = 0.0;
					chargeSwitch = 0;
					phaseSwitch = 0;
					npc.netUpdate = true;
				}
			}
			if (player.dead || Vector2.Distance(player.Center, vector) > 5600f)
			{
				if (npc.localAI[3] < 120f)
				{
					npc.localAI[3] += 1f;
				}
				if (npc.localAI[3] > 60f)
				{
					npc.velocity.Y = npc.velocity.Y + (npc.localAI[3] - 60f) * 0.25f;
					if ((double)npc.position.Y > Main.rockLayer * 16.0)
					{
						for (int x = 0; x < 200; x++)
						{
							if (Main.npc[x].type == mod.NPCType("Leviathan"))
							{
								Main.npc[x].active = false;
								Main.npc[x].netUpdate = true;
							}
						}
						npc.active = false;
						npc.netUpdate = true;
					}
				}
				return;
			}
			if (npc.localAI[3] > 0f)
			{
				npc.localAI[3] -= 1f;
			}
		}

		public override void HitEffect(int hitDirection, double damage)
		{
			for (int k = 0; k < 3; k++)
			{
				Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
			}
			if (npc.life <= 0)
			{
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SirenGores/Siren"), 1f);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SirenGores/Siren2"), 1f);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SirenGores/Siren3"), 1f);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SirenGores/Siren4"), 1f);
				Gore.NewGore(npc.position, npc.velocity, mod.GetGoreSlot("Gores/SirenGores/Siren5"), 1f);
				for (int k = 0; k < 50; k++)
				{
					Dust.NewDust(npc.position, npc.width, npc.height, 5, hitDirection, -1f, 0, default(Color), 1f);
				}
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor) //2 total states (ice shield or no ice shield)
		{
			Mod mod = ModLoader.GetMod("CalamityMod");
			Texture2D texture = Main.npcTexture[npc.type];
			if (npc.dontTakeDamage)
			{
				switch (frameUsed)
				{
					case 0:
						texture = mod.GetTexture("NPCs/Leviathan/SirenAlt");
						break;
					case 1:
						texture = mod.GetTexture("NPCs/Leviathan/SirenAltSinging");
						break;
					case 2:
						texture = mod.GetTexture("NPCs/Leviathan/SirenAltStabbing");
						break;
				}
			}
			else
			{
				switch (frameUsed)
				{
					case 0:
						texture = Main.npcTexture[npc.type];
						break;
					case 1:
						texture = mod.GetTexture("NPCs/Leviathan/SirenSinging");
						break;
					case 2:
						texture = mod.GetTexture("NPCs/Leviathan/SirenStabbing");
						break;
				}
			}
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (npc.spriteDirection == 1)
			{
				spriteEffects = SpriteEffects.FlipHorizontally;
			}
			if (npc.ai[0] == 3f)
			{
				int width = 106;
				Vector2 vector = new Vector2((float)(width / 2), (float)(texture.Height / Main.npcFrameCount[npc.type] / 2));
				Microsoft.Xna.Framework.Rectangle frame = new Rectangle(0, 0, width, texture.Height / Main.npcFrameCount[npc.type]);
				frame.Y = 146 * (int)(npc.frameCounter / 12.0); //1 to 6
				if (frame.Y >= 146 * 6)
				{
					frame.Y = 0;
				}
				Main.spriteBatch.Draw(texture,
					new Vector2(npc.position.X - Main.screenPosition.X + (float)(npc.width / 2) - (float)width * npc.scale / 2f + vector.X * npc.scale,
					npc.position.Y - Main.screenPosition.Y + (float)npc.height - (float)texture.Height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + vector.Y * npc.scale + 0f + npc.gfxOffY),
					new Microsoft.Xna.Framework.Rectangle?(frame),
					npc.GetAlpha(drawColor),
					npc.rotation,
					vector,
					npc.scale,
					spriteEffects,
					0f);
				return false;
			}
			Microsoft.Xna.Framework.Rectangle frame2 = npc.frame;
			Vector2 vector11 = new Vector2((float)(Main.npcTexture[npc.type].Width / 2), (float)(Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type] / 2));
			Main.spriteBatch.Draw(texture,
				new Vector2(npc.position.X - Main.screenPosition.X + (float)(npc.width / 2) - (float)Main.npcTexture[npc.type].Width * npc.scale / 2f + vector11.X * npc.scale,
				npc.position.Y - Main.screenPosition.Y + (float)npc.height - (float)Main.npcTexture[npc.type].Height * npc.scale / (float)Main.npcFrameCount[npc.type] + 4f + vector11.Y * npc.scale + 0f + npc.gfxOffY),
				new Microsoft.Xna.Framework.Rectangle?(frame2),
				npc.GetAlpha(drawColor),
				npc.rotation,
				vector11,
				npc.scale,
				spriteEffects,
				0f);
			return false;
		}

		public override void FindFrame(int frameHeight) //6 total frames, 3 total texture types
		{
			if (npc.ai[0] == 2f)
			{
				frameUsed = 0;
			}
			else if (npc.ai[0] <= 1f)
			{
				frameUsed = 1;
			}
			else
			{
				frameUsed = 2;
			}
			npc.frameCounter += 1.0;
			if (npc.ai[0] == 4f)
			{
				if (npc.frameCounter > 72.0)
				{
					npc.frameCounter = 0.0;
				}
			}
			else
			{
				int frameY = 146;
				if (npc.frameCounter > 72.0)
				{
					npc.frameCounter = 0.0;
				}
				npc.frame.Y = frameY * (int)(npc.frameCounter / 12.0);
				if (npc.frame.Y >= frameHeight * 6)
				{
					npc.frame.Y = 0;
				}
			}
		}

		public override void BossLoot(ref string name, ref int potionType)
		{
			potionType = ItemID.GreaterHealingPotion;
		}

        // Anahita runs the same loot code as the Leviathan, but only if she dies last.
        public override void NPCLoot()
		{
            if (!NPC.AnyNPCs(mod.NPCType("Leviathan")))
                Leviathan.DropSirenLeviLoot(npc);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit)
		{
			target.AddBuff(BuffID.Wet, 120, true);
		}

		public override bool CheckActive()
		{
			return false;
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
		{
			npc.lifeMax = (int)(npc.lifeMax * 0.8f * bossLifeScale);
			npc.damage = (int)(npc.damage * 0.8f);
		}
	}
}
