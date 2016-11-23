// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Mabi;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

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
		/// Sets up necessarily subscriptions.
		/// </summary>
		public void Initialize()
		{
			ChannelServer.Instance.Events.MinutesTimeTick += this.OnMinutesTimeTick;
		}

		/// <summary>
		/// Called once a minute to check which events need to be started
		/// or stopped.
		/// </summary>
		/// <param name="now"></param>
		private void OnMinutesTimeTick(ErinnTime now)
		{
			IEnumerable<GameEventScript> toStart, toEnd;
			lock (_gameEvents)
			{
				if (_gameEvents.Count == 0)
					return;

				toStart = _gameEvents.Values.Where(a => a.State == GameEventState.Inactive && a.IsActiveTime(now.DateTime));
				toEnd = _gameEvents.Values.Where(a => a.State == GameEventState.Active && !a.IsActiveTime(now.DateTime));
			}

			foreach (var gameEvent in toStart)
				gameEvent.Start();

			foreach (var gameEvent in toEnd)
				gameEvent.End();
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

			return (gameEvent.State == GameEventState.Active);
		}
	}
}
