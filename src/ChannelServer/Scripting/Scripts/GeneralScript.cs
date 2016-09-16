// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Linq;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Threading;
using System.Collections.Generic;
using Aura.Shared.Util.Commands;
using Aura.Shared.Scripting.Scripts;
using Aura.Data;
using Aura.Mabi;
using System.Reflection;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// General purpose script with various helpful methods and the
	/// auto subscription feature.
	/// </summary>
	public abstract class GeneralScript : IDisposable, IScript, IAutoLoader
	{
		/// <summary>
		/// Global scripting variables.
		/// </summary>
		protected ScriptVariables GlobalVars { get { return ChannelServer.Instance.ScriptManager.GlobalVars; } }

		/// <summary>
		/// Initalizes script, calling Load.
		/// </summary>
		/// <returns></returns>
		public virtual bool Init()
		{
			this.Load();
			return true;
		}

		/// <summary>
		/// Use initial setup of the script.
		/// </summary>
		/// <remarks>
		/// The reason Init calls Load is backwards compatibility,
		/// prior to IScript the initial method was called Load.
		/// </remarks>
		public virtual void Load()
		{
		}

		/// <summary>
		/// Adds subscriptions based on "On" attribute on methods.
		/// </summary>
		public void AutoLoad()
		{
			var type = this.GetType();
			var methods = this.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes(typeof(OnAttribute), false);
				if (attrs.Length == 0)
					continue;

				var attr = attrs[0] as OnAttribute;

				var eventHandlerInfo = ChannelServer.Instance.Events.GetType().GetEvent(attr.Event);
				if (eventHandlerInfo == null)
				{
					Log.Error("AutoLoadEvents: Unknown event '{0}' on '{1}.{2}'.", attr.Event, type.Name, method.Name);
					continue;
				}

				try
				{
					eventHandlerInfo.AddEventHandler(ChannelServer.Instance.Events, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, this, method));
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "AutoLoadEvents: Failed to subscribe '{1}.{2}' to '{0}'.", attr.Event, type.Name, method.Name);
				}
			}
		}

		/// <summary>
		/// Unsubscribes from all auto subscribed events.
		/// </summary>
		public virtual void Dispose()
		{
			var type = this.GetType();
			var methods = this.GetType().GetMethods();
			foreach (var method in methods)
			{
				var attrs = method.GetCustomAttributes(typeof(OnAttribute), false);
				if (attrs.Length == 0)
					continue;

				var attr = attrs[0] as OnAttribute;

				var eventHandlerInfo = ChannelServer.Instance.Events.GetType().GetEvent(attr.Event);
				if (eventHandlerInfo == null)
				{
					// Erroring on load should be enough.
					//Log.Error("Dispose: Unknown event '{0}' on '{1}.{2}'.", attr.Event, type.Name, method.Name);
					continue;
				}

				try
				{
					eventHandlerInfo.RemoveEventHandler(ChannelServer.Instance.Events, Delegate.CreateDelegate(eventHandlerInfo.EventHandlerType, this, method));
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Dispose: Failed to unsubscribe '{1}.{2}' from '{0}'.", attr.Event, type.Name, method.Name);
				}
			}

			this.CleanUp();
		}

		/// <summary>
		/// Called from Dispose, use for cleaning up before reload.
		/// </summary>
		protected virtual void CleanUp()
		{
		}

		#region General functions

		/// <summary>
		/// Returns random number between 0.0 and 100.0.
		/// </summary>
		/// <returns></returns>
		protected double Random()
		{
			var rnd = RandomProvider.Get();
			return (100 * rnd.NextDouble());
		}

		/// <summary>
		/// Returns random number between 0 and max-1.
		/// </summary>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(max);
		}

		/// <summary>
		/// Returns random number between min and max-1.
		/// </summary>
		/// <param name="min">Inclusive lower bound</param>
		/// <param name="max">Exclusive upper bound</param>
		/// <returns></returns>
		protected int Random(int min, int max)
		{
			var rnd = RandomProvider.Get();
			return rnd.Next(min, max);
		}

		/// <summary>
		/// Returns a random value from the given ones.
		/// </summary>
		/// <param name="values"></param>
		protected T Rnd<T>(params T[] values)
		{
			var rnd = RandomProvider.Get();
			return rnd.Rnd(values);
		}

		/// <summary>
		/// Returns a unique number of random parameters,
		/// useful when you need unique random numbers for example.
		/// </summary>
		/// <example>
		/// var n = UniqueRnd(3, 1,2,3,4,5); // n = int[] { 3, 1, 5 }
		/// var s = UniqueRnd(2, "test", "foo", "bar"); // s = string[] { "bar", "foo" }
		/// </example>
		/// <typeparam name="T"></typeparam>
		/// <param name="amount"></param>
		/// <param name="values"></param>
		/// <returns></returns>
		protected T[] UniqueRnd<T>(int amount, params T[] values)
		{
			if (values == null || values.Length == 0 || values.Length < amount)
				throw new ArgumentException("Values may not be null, empty, or smaller than amount.");

			var rnd = RandomProvider.Get();
			return values.OrderBy(a => rnd.Next()).Take(amount).ToArray();
		}

		/// <summary>
		/// Returns the params as array (syntactic sugar).
		/// </summary>
		/// <param name="coordinates"></param>
		protected T[] A<T>(params T[] coordinates)
		{
			return coordinates;
		}

		/// <summary>
		/// Proxy for Localization.Get.
		/// </summary>
		/// <param name="phrase"></param>
		protected string L(string phrase)
		{
			return Localization.Get(phrase);
		}

		/// <summary>
		/// Proxy for Localization.GetParticular.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="phrase"></param>
		protected string LX(string context, string phrase)
		{
			return Localization.GetParticular(context, phrase);
		}

		/// <summary>
		/// Proxy for Localization.GetPlural.
		/// </summary>
		/// <param name="phrase"></param>
		/// <param name="phrasePlural"></param>
		/// <param name="count"></param>
		protected string LN(string phrase, string phrasePlural, int count)
		{
			return Localization.GetPlural(phrase, phrasePlural, count);
		}

		/// <summary>
		/// Proxy for Localization.GetParticularPlural.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="phrase"></param>
		/// <param name="phrasePlural"></param>
		/// <param name="count"></param>
		protected string LXN(string context, string phrase, string phrasePlural, int count)
		{
			return Localization.GetParticularPlural(context, phrase, phrasePlural, count);
		}

		/// <summary>
		/// Returns true if feature is enabled.
		/// </summary>
		/// <param name="featureName"></param>
		/// <returns></returns>
		protected bool IsEnabled(string featureName)
		{
			return AuraData.FeaturesDb.IsEnabled(featureName);
		}

		/// <summary>
		/// Returns Mabi-style time span string.
		/// </summary>
		/// <example>
		/// If same day: Today night
		/// If tomorrow: Tomorrow dawn
		/// Some day: 3 days morning
		/// </example>
		/// <param name="now"></param>
		/// <param name="future"></param>
		/// <returns></returns>
		public string GetTimeSpanString(ErinnTime now, ErinnTime future)
		{
			// Calculate days
			var m1 = (now.Year * 7 * 40 * 24 * 60) + (now.Month * 40 * 24 * 60) + (now.Day * 24 * 60) + (now.Hour * 60) + now.Minute;
			var m2 = (future.Year * 7 * 40 * 24 * 60) + (future.Month * 40 * 24 * 60) + (future.Day * 24 * 60) + (future.Hour * 60) + future.Minute;
			var days = (m2 - m1) / 60.0 / 24.0;

			if (future.Hour * 100 + future.Minute > now.Hour * 100 + now.Minute)
				days = Math.Floor(days);
			else
				days = Math.Ceiling(days);

			// Get days part
			var time = "";
			if (future.DateTimeStamp == now.DateTimeStamp)
				time = L("Today");
			else if (future.DateTimeStamp == now.DateTimeStamp + 1)
				time = L("Tomorrow");
			else if (future.DateTimeStamp == now.DateTimeStamp + 2)
				time = L("the day after tomorrow");
			else
				time = string.Format(LN("{0} day", "{0} days", (int)days), (int)days);

			// Get time of day part
			var hour = future.Hour;
			if (hour >= 20)
				time += L(" night");
			else if (hour >= 12)
				time += L(" afternoon");
			else if (hour >= 6)
				time += L(" morning");
			else if (hour >= 0)
				time += L(" dawn");

			return time;
		}

		#endregion

		#region Extension

		/// <summary>
		/// Adds hook for NPC.
		/// </summary>
		/// <param name="npcName"></param>
		/// <param name="hookName"></param>
		/// <param name="func"></param>
		protected void AddHook(string npcName, string hookName, NpcScriptHook func)
		{
			ChannelServer.Instance.ScriptManager.NpcScriptHooks.Add(npcName, hookName, func);
		}

		/// <summary>
		/// Adds packet handler.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="handler"></param>
		protected void AddPacketHandler(int op, PacketHandlerManager<ChannelClient>.PacketHandlerFunc handler)
		{
			ChannelServer.Instance.Server.Handlers.Add(op, handler);
		}

		/// <summary>
		/// Adds GM command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="charAuth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="func"></param>
		protected void AddCommand(int auth, int charAuth, string name, string usage, GmCommandFunc func)
		{
			ChannelServer.Instance.CommandProcessor.Add(auth, charAuth, name, usage, func);
		}

		/// <summary>
		/// Adds GM command.
		/// </summary>
		/// <param name="auth"></param>
		/// <param name="charAuth"></param>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="description"></param>
		/// <param name="func"></param>
		protected void AddCommand(int auth, int charAuth, string name, string usage, string description, GmCommandFunc func)
		{
			ChannelServer.Instance.CommandProcessor.Add(auth, charAuth, name, usage, description, func);
		}

		/// <summary>
		/// Adds alias for a GM command.
		/// </summary>
		/// <param name="original"></param>
		/// <param name="alias"></param>
		protected void AddAlias(string original, string alias)
		{
			ChannelServer.Instance.CommandProcessor.AddAlias(original, alias);
		}

		/// <summary>
		/// Adds console command.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="usage"></param>
		/// <param name="description"></param>
		/// <param name="handler"></param>
		protected void AddConsoleCommand(string name, string usage, string description, ConsoleCommandFunc handler)
		{
			ChannelServer.Instance.ConsoleCommands.Add(name, usage, description, handler);
		}

		#endregion Extension

		#region Props

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		/// <returns>Created prop.</returns>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, float scale, PropFunc behavior = null)
		{
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Error("{1}.SpawnProp: Region '{0}' doesn't exist.", regionId, this.GetType().Name);
				return null;
			}

			var prop = new Prop(id, regionId, x, y, direction, scale);
			prop.Behavior = behavior;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Creates prop and spawns it.
		/// </summary>
		/// <returns>Created prop.</returns>
		protected Prop SpawnProp(int id, int regionId, int x, int y, float direction, PropFunc behavior = null)
		{
			return this.SpawnProp(id, regionId, x, y, direction, 1, behavior);
		}

		/// <summary>
		/// Spawns prop
		/// </summary>
		/// <returns>Created prop.</returns>
		protected Prop SpawnProp(Prop prop, PropFunc behavior = null)
		{
			var region = ChannelServer.Instance.World.GetRegion(prop.RegionId);
			if (region == null)
			{
				Log.Error("{1}.SpawnProp: Region '{0}' doesn't exist.", prop.RegionId, this.GetType().Name);
				return null;
			}

			prop.Behavior = behavior;

			region.AddProp(prop);

			return prop;
		}

		/// <summary>
		/// Sets behavior for the prop with entityId.
		/// </summary>
		/// <returns>Prop that the behavior was added for.</returns>
		protected Prop SetPropBehavior(long entityId, PropFunc behavior)
		{
			var prop = ChannelServer.Instance.World.GetProp(entityId);
			if (prop == null)
			{
				Log.Error("{1}.SetPropBehavior: Prop '{0:X16}' doesn't exist.", entityId, this.GetType().Name);
				return null;
			}

			prop.Behavior = behavior;

			return prop;
		}

		/// <summary>
		/// Returns a drop behavior.
		/// </summary>
		/// <param name="dropType"></param>
		/// <returns></returns>
		protected PropFunc PropDrop(int dropType)
		{
			return Prop.GetDropBehavior(dropType);
		}

		/// <summary>
		/// Returns a warp behavior.
		/// </summary>
		/// <remarks>
		/// The source location is ignored, it's only there for clarity.
		/// </remarks>
		/// <param name="sourceRegion"></param>
		/// <param name="sourceX"></param>
		/// <param name="sourceY"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected PropFunc PropWarp(int sourceRegion, int sourceX, int sourceY, int region, int x, int y)
		{
			return this.PropWarp(region, x, y);
		}

		/// <summary>
		/// Returns a warp behavior.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected PropFunc PropWarp(int region, int x, int y)
		{
			return Prop.GetWarpBehavior(region, x, y);
		}

		#endregion Props

		#region Spawn

		/// <summary>
		/// Creates creature spawn area.
		/// </summary>
		/// <remarks>
		/// Creates a spawner, that spawns a certain amount of monsters
		/// and respawns them after they died. The monsters can have random
		/// titles, and respawn delays, specifying how much time should be
		/// between death and respawn.
		/// </remarks>
		/// <param name="race">Race to spawn</param>
		/// <param name="amount">Maximum amount to spawn</param>
		/// <param name="regionId">Region to spawn in</param>
		/// <param name="delay">Initial spawn delay in seconds</param>
		/// <param name="delayMin">Minimum respawn delay in seconds</param>
		/// <param name="delayMax">Maximum respawn delay in seconds</param>
		/// <param name="titles">List of random titles to apply to creatures</param>
		/// <param name="coordinates">Even number of coordinates, specifying the spawn area</param>
		protected void CreateSpawner(int race, int amount, int region, int delay = 0, int delayMin = 10, int delayMax = 20, int[] titles = null, int[] coordinates = null)
		{
			ChannelServer.Instance.World.SpawnManager.Add(new CreatureSpawner(race, amount, region, delay, delayMin, delayMax, titles, coordinates));
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y)
		{
			return this.Spawn(raceId, amount, regionId, x, y, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius)
		{
			return this.Spawn(raceId, amount, regionId, x, y, radius, true, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, bool effect)
		{
			return this.Spawn(raceId, amount, regionId, x, y, radius, effect, null);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, x, y, 0, true, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, x, y, 0, true, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="x">X-coordinate to spawn creatures at.</param>
		/// <param name="y">Y-coordinate to spawn creatures at.</param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, int x, int y, int radius, bool effect, Action<Creature, Creature> onDeath)
		{
			return this.Spawn(raceId, amount, regionId, new Position(x, y), radius, effect, onDeath);
		}

		/// <summary>
		/// Spawns creature(s)
		/// </summary>
		/// <param name="raceId">Race to spawn.</param>
		/// <param name="amount">Amount of creatures to spawn.</param>
		/// <param name="regionId">Region to spawn creatures in.</param>
		/// <param name="pos">Position to spawn creatures at.</param>
		/// <param name="radius">Radius around position for random spawn</param>
		/// <param name="effect">Whether to display spawn effect</param>
		/// <param name="onDeath">Runs when one of the creatures dies</param>
		/// <returns>List of creatures spawned by this call.</returns>
		protected List<Creature> Spawn(int raceId, int amount, int regionId, Position pos, int radius, bool effect, Action<Creature, Creature> onDeath)
		{
			var result = new List<Creature>();

			amount = Math2.Clamp(1, 100, amount);

			var rnd = RandomProvider.Get();

			for (int i = 0; i < amount; ++i)
			{
				if (radius > 0)
					pos = pos.GetRandomInRange(radius, rnd);

				var creature = ChannelServer.Instance.World.SpawnManager.Spawn(raceId, regionId, pos.X, pos.Y, true, effect);

				if (onDeath != null)
					creature.Death += onDeath;

				result.Add(creature);
			}

			return result;
		}

		#endregion Spawn

		#region Client Events

		/// <summary>
		/// Adds handler for event.
		/// </summary>
		/// <remarks>
		/// Reads the region id from the event id, the region must exist first,
		/// and it must already contain the event.
		/// </remarks>
		/// <param name="eventId"></param>
		/// <param name="signal"></param>
		/// <param name="onTriggered"></param>
		public void OnClientEvent(long eventId, SignalType signal, Action<Creature, EventData> onTriggered)
		{
			// Get event
			var clientEvent = ChannelServer.Instance.World.GetClientEvent(eventId);
			if (clientEvent == null)
			{
				Log.Error("OnClientEvent: Client event '{0}' doesn't exist.", eventId);
				return;
			}

			clientEvent.Handlers.Add(signal, onTriggered);
		}

		/// <summary>
		/// Adds handler for event.
		/// </summary>
		/// <remarks>
		/// The region and the event must exist first, so the client event
		/// can be found, to add the handler.
		/// </remarks>
		/// <param name="fullName"></param>
		/// <param name="signal"></param>
		/// <param name="onTriggered"></param>
		public void OnClientEvent(string fullName, SignalType signal, Action<Creature, EventData> onTriggered)
		{
			// Get event
			var clientEvent = ChannelServer.Instance.World.GetClientEvent(fullName);
			if (clientEvent == null)
			{
				Log.Error("OnClientEvent: Client event '{0}' doesn't exist.", fullName);
				return;
			}

			clientEvent.Handlers.Add(signal, onTriggered);
		}

		#endregion Client Events

		#region Timers

		/// <summary>
		/// Creates timer that runs action once, after x milliseconds.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected Timer SetTimeout(int ms, Action action)
		{
			Timer timer = null;
			timer = new Timer(_ =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Exception during SetInterval callback in {0}.", this.GetType().Name);
				}
				GC.KeepAlive(timer);
			}
			, null, ms, Timeout.Infinite);

			return timer;
		}

		/// <summary>
		/// Creates timer that runs action repeatedly, every x milliseconds.
		/// </summary>
		/// <param name="ms"></param>
		/// <param name="action"></param>
		/// <returns></returns>
		protected Timer SetInterval(int ms, Action action)
		{
			Timer timer = null;
			timer = new Timer(_ =>
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					Log.Exception(ex, "Exception during SetInterval callback in {0}.", this.GetType().Name);
				}
				GC.KeepAlive(timer);
			}
			, null, ms, ms);

			return timer;
		}

		/// <summary>
		/// Stops timer by changing due time and period.
		/// </summary>
		/// <param name="timer"></param>
		protected void StopTimer(Timer timer)
		{
			timer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		#endregion Timers
	}

	/// <summary>
	/// Attribute for methods in GeneralScript, to mark them as subscribers
	/// for events in the EventManager.
	/// </summary>
	public class OnAttribute : Attribute
	{
		/// <summary>
		/// Event to subscribe to (*without* "On" prefix).
		/// </summary>
		public string Event { get; protected set; }

		/// <summary>
		/// Creates new attribute.
		/// </summary>
		/// <param name="evnt"></param>
		public OnAttribute(string evnt)
		{
			this.Event = evnt;
		}
	}
}