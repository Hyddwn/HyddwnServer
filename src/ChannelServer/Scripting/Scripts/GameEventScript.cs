// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Script for in-game events, like Double Rainbow.
	/// </summary>
	public class GameEventScript : GeneralScript
	{
		private List<ActivationSpan> _activationSpans = new List<ActivationSpan>();

		/// <summary>
		/// The event's unique id.
		/// </summary>
		/// <remarks>
		/// Sent to client, some ids activate special client behavior.
		/// </remarks>
		public string Id { get; private set; }

		/// <summary>
		/// The event's name, which is used in notices and broadcasts.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// Returns the current state of the event.
		/// </summary>
		public bool IsActive { get; private set; }

		/// <summary>
		/// Loads and sets up event.
		/// </summary>
		/// <returns></returns>
		public override bool Init()
		{
			this.Load();

			if (string.IsNullOrWhiteSpace(this.Id) || string.IsNullOrWhiteSpace(this.Name))
			{
				Log.Error("Id or name not set for event script '{0}'.", this.GetType().Name);
				return false;
			}

			ChannelServer.Instance.GameEventManager.Register(this);

			this.AfterLoad();

			return true;
		}

		/// <summary>
		/// Called after script was registered, so it can schedule itself.
		/// </summary>
		public virtual void AfterLoad()
		{
		}

		/// <summary>
		/// Sets event's id.
		/// </summary>
		/// <param name="id"></param>
		public void SetId(string id)
		{
			this.Id = id;
		}

		/// <summary>
		/// Sets event's name, which is used for notices and broadcasts.
		/// </summary>
		/// <param name="name"></param>
		public void SetName(string name)
		{
			this.Name = name;
		}

		/// <summary>
		/// Starts event if it's not active yet.
		/// </summary>
		public void Start()
		{
			if (this.IsActive)
				return;

			this.IsActive = true;
			this.OnStart();
		}

		/// <summary>
		/// Stops event if it's active.
		/// </summary>
		public void End()
		{
			if (!this.IsActive)
				return;

			this.IsActive = false;
			this.OnEnd();
		}

		/// <summary>
		/// Called when the event is activated.
		/// </summary>
		protected virtual void OnStart()
		{
		}

		/// <summary>
		/// Called when the event is deactivated.
		/// </summary>
		protected virtual void OnEnd()
		{
		}

		/// <summary>
		/// Adds the given activation span to the event, in which it's
		/// supposed to be active.
		/// </summary>
		/// <param name="span"></param>
		public void AddActivationSpan(ActivationSpan span)
		{
			lock (_activationSpans)
				_activationSpans.Add(span);

			var now = DateTime.Now;

			// Active time
			if (now >= span.Start && now < span.End)
			{
				this.Start();
			}
			// Inactive time
			else
			{
				this.End();
			}
		}

		/// <summary>
		/// Returns true if the event is supposed to be active at the given
		/// time, based on its activation spans.
		/// </summary>
		/// <param name="time"></param>
		/// <returns></returns>
		public bool IsActiveTime(DateTime time)
		{
			lock (_activationSpans)
				return _activationSpans.Any(a => time >= a.Start && time < a.End);
		}
	}

	public class ActivationSpan
	{
		public string Id { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
