// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World;
using Aura.Channel.World.Dungeons;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Scripts
{
	public class DungeonScript : GeneralScript
	{
		/// <summary>
		/// Name of the dungeon
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Called when the script is initially created.
		/// </summary>
		/// <returns></returns>
		public override bool Init()
		{
			var attr = this.GetType().GetCustomAttribute<DungeonScriptAttribute>();
			if (attr == null)
			{
				Log.Error("DungeonScript.Init: Missing DungeonScript attribute.");
				return false;
			}

			this.Name = attr.Name;

			this.Load();

			ChannelServer.Instance.ScriptManager.DungeonScripts.Add(this.Name, this);

			return true;
		}

		/// <summary>
		/// Changes dungeonName depending on the item, returns true if routing
		/// was successful, if not, the dungeon was invalid.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="dungeonName"></param>
		/// <returns></returns>
		public virtual bool Route(Creature creature, Item item, ref string dungeonName)
		{
			return true;
		}

		/// <summary>
		/// Called when the dungeon was just created.
		/// </summary>
		public virtual void OnCreation(Dungeon dungeon)
		{
		}

		/// <summary>
		/// Called when the boss door opens.
		/// </summary>
		/// <param name="dungeon"></param>
		public virtual void OnBoss(Dungeon dungeon)
		{
		}

		/// <summary>
		/// Called when one of the boss monsters dies.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="deadBoss"></param>
		/// <param name="killer"></param>
		public virtual void OnBossDeath(Dungeon dungeon, Creature deadBoss, Creature killer)
		{
		}

		/// <summary>
		/// Called when the boss was killed.
		/// </summary>
		/// <param name="dungeon"></param>
		public virtual void OnCleared(Dungeon dungeon)
		{
		}

		/// <summary>
		/// Called when a player leaves a dungeon via the first statue,
		/// logging out, or similar.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="creature"></param>
		public virtual void OnLeftEarly(Dungeon dungeon, Creature creature)
		{
		}

		/// <summary>
		/// Called whenever a player enters the dungeon's lobby.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="creature">The creature that entered.</param>
		public virtual void OnPlayerEntered(Dungeon dungeon, Creature creature)
		{
		}

		/// <summary>
		/// Called once, when the entire initial party has entered the
		/// dungeon's lobby.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="creature">The last creature that entered.</param>
		public virtual void OnPartyEntered(Dungeon dungeon, Creature creature)
		{
		}

		/// <summary>
		/// Called when all puzzles of a section have been solved.
		/// </summary>
		/// <param name="dungeon"></param>
		/// <param name="floor">The floor, starting at 1 (lobby is ignored).</param>
		/// <param name="section">The section, starting at 1.</param>
		public virtual void OnSectionCleared(Dungeon dungeon, int floor, int section)
		{
		}
	}

	public class DungeonScriptAttribute : Attribute
	{
		public string Name { get; private set; }

		public DungeonScriptAttribute(string name)
		{
			this.Name = name.ToLower();
		}
	}
}
