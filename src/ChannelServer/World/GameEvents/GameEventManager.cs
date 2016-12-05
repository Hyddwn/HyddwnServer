// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.World.GameEvents
{
	/// <summary>
	/// Holds all available game events, starts and stops them, and notifies
	/// players about them.
	/// </summary>
	public class GameEventManager
	{
		private Dictionary<string, GameEventScript> _gameEvents = new Dictionary<string, GameEventScript>();

		/// <summary>
		/// Global bonuses for AP, EXP, etc.
		/// </summary>
		public GlobalBonusManager GlobalBonuses { get; private set; }

		/// <summary>
		/// Creates new instance of manager.
		/// </summary>
		public GameEventManager()
		{
			this.GlobalBonuses = new GlobalBonusManager();
		}

		/// <summary>
		/// Sets up necessarily subscriptions.
		/// </summary>
		public void Initialize()
		{
			ChannelServer.Instance.Events.MinutesTimeTick += this.OnMinutesTimeTick;
			ChannelServer.Instance.Events.CreatureConnected += this.OnCreatureConnected;
		}

		/// <summary>
		/// Called once a minute to check which events need to be started
		/// or stopped.
		/// </summary>
		/// <param name="now"></param>
		private void OnMinutesTimeTick(ErinnTime now)
		{
			var toStart = new List<GameEventScript>();
			var toEnd = new List<GameEventScript>();

			lock (_gameEvents)
			{
				if (_gameEvents.Count == 0)
					return;

				foreach (var gameEvent in _gameEvents.Values)
				{
					var isActive = (gameEvent.IsActive);
					var isActiveTime = gameEvent.IsActiveTime(now.DateTime);

					if (!isActive && isActiveTime)
						toStart.Add(gameEvent);
					else if (isActive && !isActiveTime)
						toEnd.Add(gameEvent);
				}
			}

			foreach (var gameEvent in toStart)
				gameEvent.Start();

			foreach (var gameEvent in toEnd)
				gameEvent.End();

			// Update broadcast
			if (toStart.Any() || toEnd.Any())
			{
				var activeEvents = this.GetActiveEvents();
				var message = GetBroadcastMessage(activeEvents);
				Send.Notice(NoticeType.TopGreen, message);
			}
		}

		/// <summary>
		/// Called when a player logged in, sends notice about active events.
		/// </summary>
		/// <param name="creature"></param>
		private void OnCreatureConnected(Creature creature)
		{
			// RP characters "connect", but they don't need another set of
			// notices and event activations.
			if (creature.IsRpCharacter)
				return;

			var activeEvents = this.GetActiveEvents();
			var message = GetBroadcastMessage(activeEvents);

			if (!string.IsNullOrWhiteSpace(message))
				Send.Notice(creature, NoticeType.TopGreen, message);
			foreach (var gameEvent in activeEvents)
				Send.GameEventStateUpdate(creature, gameEvent.Id, gameEvent.IsActive);
		}

		/// <summary>
		/// Registers an event with the manager.
		/// </summary>
		/// <param name="gameEvent"></param>
		public void Register(GameEventScript gameEvent)
		{
			lock (_gameEvents)
			{
				if (_gameEvents.ContainsKey(gameEvent.Id))
					throw new ArgumentException("An event with the id '" + gameEvent.Id + "' already exists.");

				_gameEvents[gameEvent.Id] = gameEvent;
			}
		}

		/// <summary>
		/// Unregisters an event from the manager.
		/// </summary>
		/// <param name="gameEvent"></param>
		public void Unregister(string gameEventId)
		{
			lock (_gameEvents)
			{
				if (_gameEvents.ContainsKey(gameEventId))
					_gameEvents.Remove(gameEventId);
			}
		}

		/// <summary>
		/// Adds an activation span to the given event.
		/// </summary>
		/// <param name="gameEventId"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		public void AddActivationSpan(string gameEventId, DateTime start, DateTime end)
		{
			GameEventScript gameEvent;
			lock (_gameEvents)
			{
				if (!_gameEvents.TryGetValue(gameEventId, out gameEvent))
					throw new ArgumentException("Unknown even id '" + gameEventId + "'.");
			}

			var span = new ActivationSpan();
			span.Id = gameEventId;
			span.Start = start;
			span.End = end;

			gameEvent.AddActivationSpan(span);
		}

		/// <summary>
		/// Returns true if the given event is currently active.
		/// </summary>
		/// <param name="gameEventId"></param>
		/// <returns></returns>
		public bool IsActive(string gameEventId)
		{
			GameEventScript gameEvent;
			lock (_gameEvents)
				_gameEvents.TryGetValue(gameEventId, out gameEvent);

			if (gameEvent == null)
				return false;

			return gameEvent.IsActive;
		}

		/// <summary>
		/// Returns broadcast message that is used to inform players about
		/// active events.
		/// </summary>
		/// <returns></returns>
		private static string GetBroadcastMessage(IEnumerable<GameEventScript> gameEvents)
		{
			var sb = new StringBuilder();
			var count = gameEvents.Count();

			var i = 0;
			foreach (var gameEvent in gameEvents)
			{
				sb.AppendFormat("The {0} Event is in progress.", gameEvent.Name);
				if (++i < count)
					sb.Append("     ");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Returns list of all events that are currently active.
		/// </summary>
		/// <returns></returns>
		public GameEventScript[] GetActiveEvents()
		{
			lock (_gameEvents)
				return _gameEvents.Values.Where(a => a.IsActive).ToArray();
		}
	}
}
