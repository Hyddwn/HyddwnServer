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
		/// <param name="viewers"></param>
		public Cutscene(string name, Creature leader, params Creature[] viewers)
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

			// Create list of viewers, with the leader being the first one
			// (index 0), followed by the leader's party members and then
			// the other viewers.
			// Using List and Distinct to maintain the order, while getting
			// each viewer only once. This is never gonna be a performance
			// problem for us, but if it were we'd need a custom type.
			var viewersList = new List<Creature>();

			viewersList.Add(leader);
			viewersList.AddRange(leader.Party.GetSortedMembers(a => a.Region == leader.Region));

			if (viewers != null)
				viewersList.AddRange(viewers);

			_viewers = viewersList.Distinct().ToArray();
		}

		/// <summary>
		/// Loads actors from the cutscene's data.
		/// </summary>
		public void LoadActorsFromData()
		{
			var leader = this.Leader;

			foreach (var cutsceneActorData in this.Data.Actors)
			{
				Creature actor = null;

				var actorName = cutsceneActorData.Name;
				var defaultActorName = cutsceneActorData.Default;

				// Retrieve actor
				if (actorName.StartsWith("#"))
				{
					var actorData = AuraData.ActorDb.Find(actorName);
					if (actorData == null)
						Log.Warning("Cutscene.FromData: Unknown actor '{0}'.", actorData);
					else
						actor = new NPC(actorData);
				}
				else if (actorName == "me")
				{
					actor = leader;
				}
				else if (actorName == "leader")
				{
					actor = _viewers[0];
				}
				else if (actorName.StartsWith("player"))
				{
					int idx;
					if (!int.TryParse(actorName.Substring("player".Length), out idx))
					{
						Log.Warning("Cutscene.FromData: Invalid party member actor name '{0}'.", actorName);
					}
					else if (idx > _viewers.Length - 1)
					{
						if (!string.IsNullOrWhiteSpace(defaultActorName))
						{
							var actorData = AuraData.ActorDb.Find(defaultActorName);
							if (actorData == null)
								Log.Warning("Cutscene.FromData: Unknown default actor '{0}'.", defaultActorName);
							else
								actor = new NPC(actorData);
						}
						else
							Log.Warning("Cutscene.FromData: Index out of party member range '{0}/{1}'.", idx, _viewers.Length);
					}
					else
					{
						actor = _viewers[idx];
					}
				}
				else
				{
					Log.Warning("Cutscene.FromData: Unknown kind of actor ({0}).", actorName);
				}

				if (actor == null)
				{
					var dummy = new NPC();
					actor = dummy;
				}

				this.AddActor(actorName, actor);
			}
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
		/// <param name="onFinish"></param>
		public void Play(Action<Cutscene> onFinish)
		{
			this.Play();
			_callback = onFinish;
		}

		/// <summary>
		/// Loads cutscene from data and plays it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="leader"></param>
		/// <param name="viewers"></param>
		public static void Play(string name, Creature leader, params Creature[] viewers)
		{
			Play(name, leader, null, viewers);
		}

		/// <summary>
		/// Loads cutscene from data and plays it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="leader"></param>
		/// <param name="onFinish"></param>
		/// <param name="viewers"></param>
		public static void Play(string name, Creature leader, Action<Cutscene> onFinish, params Creature[] viewers)
		{
			var cutscene = new Cutscene(name, leader, viewers);
			cutscene.LoadActorsFromData();
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
			{
				// Only set cutscene to null if callback didn't start
				// another one, otherwise players wouldn't be able to
				// finish the new one, getting stuck.
				if (member.Temp.CurrentCutscene == this)
					member.Temp.CurrentCutscene = null;
			}
		}
	}
}
