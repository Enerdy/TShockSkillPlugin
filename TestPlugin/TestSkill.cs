﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria;
using TShockAPI;
using WhiteXHazSkillz;
using WhiteXHazSkillz.Events;

namespace TestPlugin
{
	public class TestSkill : WhiteXHazSkillz.Plugin
	{
		public EventRegister register;

		public override string Name { get { return "Test Skill"; }}

		public TestSkill(EventRegister register) : base(register)
		{
			this.register = register;
		}

		public override void Initialize()
		{
			register.RegisterSkillHandler<PlayerDamageEvent>(this, OnPlayerTakesDamage, EventType.PlayerTakesDamage);
			register.RegisterSkillHandler<PlayerDamageEvent>(this, OnPlayerDoesDamage, EventType.PlayerDoesDamage);
			register.RegisterSkillHandler<NpcKilledEvent>(this, OnPlayerKillsNPC, EventType.NpcIsKilled);
			register.RegisterSkillHandler<NpcDamageEvent>(this, OnPlayerDamagesNPC, EventType.NpcTakesDamage);
			TShockAPI.Commands.ChatCommands.Add(new Command("", AddSkillPoint, "ts"));
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
				register.DeregisterSkillHandler(this, EventType.PlayerTakesDamage);
		}

		private void AddSkillPoint(CommandArgs args)
		{
			try
			{
				SkillPlayer ply = PlayerManager.GetPlayer(args.Player.Index);
				SkillInformation info = ply.GetSkillInformation(Name);
				SkillInformation newInfo;
				if (info == null)
					newInfo = new SkillInformation() {Name = this.Name, Value = 0};
				else
					newInfo = new SkillInformation() {Name = this.Name, Value = info.Value + 1};

				ply.SetSkillInformation(newInfo);
			}
			catch (Exception e)
			{
				//player is not logged in
				Console.WriteLine(e.Message);
			}
		}

		Random rand = new Random();
		private void OnPlayerTakesDamage(PlayerDamageEvent args)
		{
			try
			{
				SkillPlayer player = PlayerManager.GetPlayer(args.HurtPlayerIndex);
				SkillInformation info = player.GetSkillInformation(Name);

				if (info == null)
				{
					info = new SkillInformation(){Name = this.Name, Value = 0};
					player.SetSkillInformation(info);
					return;
				}

				if(info.Value > 0 && !args.PVP)
					FireRocket(player, args.Damage, 1);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
			
		}

		private void OnPlayerDoesDamage(PlayerDamageEvent args)
		{
			//hurt another player
		}

		private void OnPlayerKillsNPC(NpcKilledEvent args)
		{
			try
			{
				SkillPlayer player = PlayerManager.GetPlayer(args.PlayerIndex);
				SkillInformation info = player.GetSkillInformation(Name);

				if (info == null)
				{
					info = new SkillInformation() { Name = this.Name, Value = 0 };
					player.SetSkillInformation(info);
					return;
				}

				if (info.Value > 0)
				{
					FireRocket(player, args.Damage, info.Value);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void OnPlayerDamagesNPC(NpcDamageEvent args)
		{
			try
			{
				SkillPlayer player = PlayerManager.GetPlayer(args.PlayerIndex);
				SkillInformation info = player.GetSkillInformation(Name);

				if (info == null)
				{
					info = new SkillInformation() { Name = this.Name, Value = 0 };
					player.SetSkillInformation(info);
					return;
				}

				if (info.Value > 0)
				{
					double ratio = Math.Min(info.Value / 20.0, 1.0);
					int health = (int)Math.Ceiling(ratio * args.Damage);
					player.Player.Heal(health);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private void FireRocket(SkillPlayer origin, int damage, int strikes)
		{
			for (int i = 0; i < strikes; i++)
			{
				int xOffset = rand.Next(-10 * strikes, 10 * strikes);
				int projectileIndex = Projectile.NewProjectile(origin.Player.TPlayer.position.X + xOffset,
					origin.Player.TPlayer.position.Y - 300, 0, 3, 207, damage, 50, origin.Player.Index, 0, 0);
				NetMessage.SendData(27, -1, -1, "", projectileIndex);
			}
		}
	}
}
