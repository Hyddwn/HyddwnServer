// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Channel.Network.Sending;
using Aura.Mabi.Const;
using Aura.Data;
using Aura.Data.Database;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
	public class Cutscene
	{
		private Action<Cutscene> _callback;
		private Creature[] _viewers;

		/// <summary>
		/// Name of the cutscene file.
		/// </summary>
		public string Name { get; protected set; }

		/// <summary>
		/// Creature that created the cutscene.
		/// </summary>
		public Creature Leader { get; protected set; }

		/// <summary>
		/// Data associated with this cutscene.
		/// </summary>
		public CutsceneData Data { get; protected set; }

		/// <summary>
		/// Actors of the cutscene.
		/// </summary>
		public Dictionary<string, Creature> Actors { get; protected set; }

		/// <summary>
		/// Creates new cutscene.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="leader"></param>
		public Cutscene(string name, Creature leader)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");

			if (leader == null)
				throw new ArgumentNullException("leader");

			if ((this.Data = AuraData.CutscenesDb.Find(name)) == null)
				throw new ArgumentException("Unknown cutscene '" + name + "'.");

			this.Name = name;
			this.Leader = leader;

			this.Actors = new Dictionary<string, Creature>();
		}

		/// <summary>
		/// Creates cutscene and fills actor list as specified in the data.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="creature"></param>
		/// <returns></returns>
		public static Cutscene FromData(string name, Creature creature)
		{
			var result = new Cutscene(name, creature);

			var partyMembers = creature.Party.GetSortedMembers();
			var dummy = new NPC();

			foreach (var actorName in result.Data.Actors)
			{
				Creature actor = null;

				// Retrieve actor
				if (actorName.StartsWith("#"))
				{
					var actorData = AuraData.ActorDb.Find(actorName);
					if (actorData == null)
						Log.Warning("Unknown actor '{0}'.", actorName);
					else
						actor = new NPC(actorData);
				}
				else if (actorName == "me")
				{
					actor = creature;
				}
				else if (actorName == "leader")
				{
					actor = creature.Party.Leader;
				}
				else if (actorName.StartsWith("player"))
				{
					int idx;
					if (!int.TryParse(actorName.Substring("player".Length), out idx))
						Log.Warning("Cutscene.FromData: Invalid party member actor name '{0}'.", actorName);
					else if (idx > partyMembers.Length - 1)
						Log.Warning("Cutscene.FromData: Index out of party member range '{0}/{1}'.", idx, partyMembers.Length);
					else
						actor = partyMembers[idx];
				}
				else
					Log.Warning("Cutscene.FromData: Unknown kind of actor ({0}).", actorName);

				if (actor == null)
					actor = dummy;

				result.AddActor(actorName, actor);
			}

			return result;
		}

		/// <summary>
		/// Adds creature as actor.
		/// </summary>
		/// <remarks>
		/// Officials apparently create copies of the creatures, getting rid
		/// of the name (replaced by the actor name), stand styles, etc.
		/// </remarks>
		/// <param name="name"></param>
		/// <param name="creature"></param>
		public void AddActor(string name, Creature creature)
		{
			if (creature == null)
			{
				creature = new NPC();
				creature.Name = name;
			}

			this.Actors[name] = creature;
		}

		/// <summary>
		/// Adds new creature of race as actor.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="raceId"></param>
		public void AddActor(string name, int raceId)
		{
			var creature = new NPC(raceId);
			this.Actors[name] = creature;
		}

		/// <summary>
		/// Plays cutscene for everybody.
		/// </summary>
		public void Play()
		{
			_viewers = this.Leader.Party.GetMembers();

			foreach (var member in _viewers)
			{
				member.Temp.CurrentCutscene = this;
				member.Lock(Locks.Default, true);
				Send.PlayCutscene(member, this);
			}
		}

		/// <summary>
		/// Plays cutscene for everybody.
		/// </summary>
		public void Play(Action<Cutscene> onFinish)
		{
			this.Play();
			_callback = onFinish;
		}

		/// <summary>
		/// Loads cutscene from data and plays it.
		/// </summary>
		public static void Play(string name, Creature creature)
		{
			var cutscene = FromData(name, creature);
			cutscene.Play();
		}

		/// <summary>
		/// Loads cutscene from data and plays it.
		/// </summary>
		public static void Play(string name, Creature creature, Action<Cutscene> onFinish)
		{
			var cutscene = FromData(name, creature);
			cutscene.Play(onFinish);
		}

		/// <summary>
		/// Ends cutscene for everybody.
		/// </summary>
		public void Finish()
		{
			foreach (var member in _viewers)
			{
				Send.CutsceneEnd(member);
				member.Unlock(Locks.Default, true);
				Send.CutsceneUnk(member);
			}

			// Call callback before setting cutscene to null so it can
			// be referenced from the core during the callback.
			if (_callback != null)
				_callback(this);

			foreach (var member in _viewers)
				member.Temp.CurrentCutscene = null;
		}
	}
}
