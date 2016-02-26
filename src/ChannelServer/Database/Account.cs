// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Channel.Scripting;
using Aura.Channel.World.Inventory;
using Aura.Shared.Util;
using Aura.Shared.Database;

namespace Aura.Channel.Database
{
	public class Account
	{
		private int _autobanScore;
		private int _autobanCount;

		/// <summary>
		/// The account name, which acts as id.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// The current session key.
		/// </summary>
		public long SessionKey { get; set; }

		/// <summary>
		/// The account's authority level, mainly used for GM commands.
		/// </summary>
		public int Authority { get; set; }

		/// <summary>
		/// The time of the last login.
		/// </summary>
		public DateTime LastLogin { get; set; }

		/// <summary>
		/// The reason the account was banned.
		/// </summary>
		public string BanReason { get; set; }

		/// <summary>
		/// The time at which the ban is lifted.
		/// </summary>
		public DateTime BanExpiration { get; set; }

		/// <summary>
		/// List of characters on the account.
		/// </summary>
		public List<Character> Characters { get; set; }

		/// <summary>
		/// List of pets/partners on the account.
		/// </summary>
		public List<Pet> Pets { get; set; }

		/// <summary>
		/// Account specific variables.
		/// </summary>
		/// <remarks>
		/// Permanent variables are saved across relogs.
		/// </remarks>
		public ScriptVariables Vars { get; protected set; }

		/// <summary>
		/// Account wide bank.
		/// </summary>
		public BankInventory Bank { get; protected set; }

		/// <summary>
		/// Account's in-game cash points (Pon).
		/// </summary>
		public int Points
		{
			get { return _points; }
			set { _points = (int)Math2.Clamp((long)0, int.MaxValue, value); }
		}
		private int _points;

		/// <summary>
		/// Manager for Account's premium services.
		/// </summary>
		public PremiumServices PremiumServices { get; private set; }

		/// <summary>
		/// Account's current Autoban score. Don't set this directly
		/// as Autoban takes care of it.
		/// </summary>
		public int AutobanScore
		{
			get
			{
				return _autobanScore;
			}
			internal set
			{
				if (value < 0)
					value = 0;

				_autobanScore = value;
			}
		}

		/// <summary>
		/// Account's current Autoban count. Don't set this directly
		/// as Autoban takes care of it.
		/// </summary>
		public int AutobanCount
		{
			get
			{
				return _autobanCount;
			}
			internal set
			{
				if (value < 0)
					value = 0;

				_autobanCount = value;
			}
		}

		/// <summary>
		/// Last time this account had its autoban score reduced.
		/// Don't set this directly, as Autoban takes care of it.
		/// </summary>
		public DateTime LastAutobanReduction { get; internal set; }

		/// <summary>
		/// Creates new account instance.
		/// </summary>
		public Account()
		{
			this.Characters = new List<Character>();
			this.Pets = new List<Pet>();
			this.Vars = new ScriptVariables();
			this.Bank = new BankInventory();
			this.PremiumServices = new PremiumServices();

			this.LastLogin = DateTime.Now;
		}

		/// <summary>
		/// Returns character or pet with the given entity id,
		/// or null, if entity wasn't found.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public PlayerCreature GetCharacterOrPet(long entityId)
		{
			PlayerCreature result = this.Characters.FirstOrDefault(a => a.EntityId == entityId);
			if (result == null)
				result = this.Pets.FirstOrDefault(a => a.EntityId == entityId);
			return result;
		}

		/// <summary>
		/// Returns character or pet with the given entity id,
		/// or throws, if entity wasn't found.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		/// <exception cref="SevereViolation">When entity doesn't exist on account.</exception>
		public PlayerCreature GetCharacterOrPetSafe(long entityId)
		{
			var r = this.GetCharacterOrPet(entityId);
			if (r == null)
				throw new SevereViolation("Account doesn't contain character 0x{0:X}", entityId);

			return r;
		}

		/// <summary>
		/// Returns pet with the given entity id,
		/// or null, if entity wasn't found.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Pet GetPet(long entityId)
		{
			return this.Pets.FirstOrDefault(a => a.EntityId == entityId);
		}

		/// <summary>
		/// Returns pet with the given entity id,
		/// or throws, if entity wasn't found.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		/// <exception cref="SevereViolation">When entity doesn't exist on account.</exception>
		public Pet GetPetSafe(long entityId)
		{
			var r = this.GetPet(entityId);
			if (r == null)
				throw new SevereViolation("Account doesn't contain pet 0x{0:X}", entityId);

			return r;
		}
	}
}
