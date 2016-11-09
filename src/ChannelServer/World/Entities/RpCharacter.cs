// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;

namespace Aura.Channel.World.Entities
{
	/// <summary>
	/// Role-playing character, controlled by players.
	/// </summary>
	public class RpCharacter : PlayerCreature
	{
		/// <summary>
		/// Always returns false (RP characters don't save.)
		/// </summary>
		/// <remarks>
		/// TODO: Replace property with a list in channel client?
		/// </remarks>
		public override bool Save { get { return false; } }

		/// <summary>
		/// Reference the actor data used to create this character.
		/// </summary>
		public ActorData ActorData { get; private set; }

		/// <summary>
		/// Reference to the creature playing this role.
		/// </summary>
		public Creature Actor { get; private set; }

		/// <summary>
		/// Returns false, since RP characters aren't allowed to
		/// move equipment.
		/// </summary>
		public override bool CanMoveEquip { get { return false; } }

		/// <summary>
		/// Creates new RP character, based on actor data.
		/// </summary>
		/// <param name="actorData">The data to base this character on.</param>
		/// <param name="actor">Character of the player who gets control over this RP character.</param>
		/// <param name="name">The RP character's full name, use null for default.</param>
		/// <example>
		/// var character = new RpCharacter(AuraData.ActorDb.Find("#tarlach"), "Tarlach (FooBar)", fooBar);
		/// </example>
		public RpCharacter(ActorData actorData, Creature actor, string name)
		{
			if (actorData == null) throw new ArgumentNullException("actorData");
			if (actor == null) throw new ArgumentNullException("actor");

			if (actor.Client is DummyClient)
				throw new InvalidOperationException("Actor must be a player with a valid client.");

			var rnd = RandomProvider.Get();

			this.ActorData = actorData;
			this.Actor = actor;
			this.EntityId = NPC.GetNewNpcEntityId();
			this.RaceId = actorData.RaceId;
			this.LoadDefault();

			// Name
			if (!string.IsNullOrWhiteSpace(name))
			{
				this.Name = name;
			}
			else
			{
				// Use actor data's local name or fall back to race's name.
				var characterName = actorData.LocalName ?? this.RaceData.Name;
				var actorName = this.Actor.Name;

				this.Name = string.Format("{0} ({1})", Localization.Get(characterName), actorName);
			}

			// State
			this.State |= CreatureStates.InstantNpc;
			this.State |= CreatureStates.EnableCommonPvp;

			// Default location
			this.SetLocation(this.Actor.GetLocation());

			// Color
			if (actorData.HasColors)
			{
				this.Color1 = actorData.Color1;
				this.Color2 = actorData.Color2;
				this.Color3 = actorData.Color3;
			}

			// Body
			this.Height = actorData.Height;
			this.Weight = actorData.Weight;
			this.Upper = actorData.Upper;
			this.Lower = actorData.Lower;
			this.EyeColor = (byte)actorData.EyeColor;
			this.EyeType = (short)actorData.EyeType;
			this.MouthType = (byte)actorData.MouthType;
			this.SkinColor = (byte)actorData.SkinColor;

			// Titles
			this.Titles.SelectedTitle = (ushort)actorData.Title;

			// Stats
			this.Age = (short)actorData.Age;
			this.Level = (short)actorData.Level;
			this.Exp = AuraData.ExpDb.GetTotalForNextLevel(this.Level - 1);
			this.AbilityPoints = (short)actorData.AP;
			this.LifeMaxBase = actorData.Life;
			this.ManaMaxBase = actorData.Mana;
			this.StaminaMaxBase = actorData.Stamina;
			this.StrBase = actorData.Str;
			this.IntBase = actorData.Int;
			this.DexBase = actorData.Dex;
			this.WillBase = actorData.Will;
			this.LuckBase = actorData.Luck;

			// Hair and Face
			if (actorData.FaceItemId != 0)
			{
				var face = new Item(actorData.FaceItemId);
				face.Info.Color1 = (byte)actorData.SkinColor;
				this.Inventory.Add(face, Pocket.Face);
			}

			if (actorData.HairItemId != 0)
			{
				var hair = new Item(actorData.HairItemId);
				hair.Info.Color1 = actorData.HairColor;
				this.Inventory.Add(hair, Pocket.Hair);
			}

			// Items
			foreach (var itemData in actorData.Items)
			{
				var item = new Item(itemData.ItemId);
				item.Info.State = (byte)itemData.State;

				item.Info.Amount = (ushort)itemData.Amount;
				if (item.Data.StackType != StackType.Sac && item.Info.Amount < 1)
					item.Info.Amount = 1;

				if (itemData.HasColors)
				{
					item.Info.Color1 = itemData.Color1;
					item.Info.Color2 = itemData.Color2;
					item.Info.Color3 = itemData.Color3;
				}

				var pocket = (Pocket)itemData.Pocket;
				if (pocket != Pocket.None)
					this.Inventory.Add(item, pocket);
			}

			// Skills
			this.Skills.Add(SkillId.CombatMastery, SkillRank.Novice, this.RaceId);

			foreach (var skillData in this.RaceData.Skills)
				this.Skills.Add((SkillId)skillData.SkillId, (SkillRank)skillData.Rank, this.RaceId);

			foreach (var skillData in actorData.Skills)
				this.Skills.Add(skillData.SkillId, skillData.Rank, this.RaceId);

			// Max stats out after skills and items were added (bonuses).
			this.Life = this.LifeMax;
			this.Mana = this.ManaMax;
			this.Stamina = this.StaminaMax;

			this.Client = actor.Client;
		}

		/// <summary>
		/// Starts RP session, removing actor from world and adding the
		/// RP character.
		/// </summary>
		public void Start()
		{
			var actor = this.Actor;
			var rpCharacter = this;
			var client = actor.Client;

			actor.Region.RemoveCreature(actor);
			actor.Lock(Locks.Default, true);

			var playerCreature = actor as PlayerCreature;
			playerCreature.StopLookAround();

			// Don't remove the actor from the controlled creatures, as the
			// client will still send packets with its id, which triggers
			// our safety checks.

			client.Creatures.Add(rpCharacter.EntityId, rpCharacter);
			client.Controlling = rpCharacter;

			var channelHost = ChannelServer.Instance.Conf.Channel.ChannelHost;
			var channelPort = ChannelServer.Instance.Conf.Channel.ChannelPort;

			Send.RequestSecondaryLogin(actor, rpCharacter.EntityId, channelHost, channelPort);
			Send.PetRegister(actor, rpCharacter, SubordinateType.RpCharacter);
			Send.StartRP(actor, rpCharacter.EntityId);
		}

		/// <summary>
		/// Ends RP session.
		/// </summary>
		public void End()
		{
			var actor = this.Actor;
			var rpCharacter = this;
			var client = rpCharacter.Client;

			Send.EndRP(actor, actor.RegionId);
			rpCharacter.Region.RemoveCreature(rpCharacter);

			// Set Controlling before adding the actor to the region again,
			// otherwise the client isn't added to the region's client list
			// used for broadcasting.

			client.Controlling = actor;
			client.Creatures.Remove(rpCharacter.EntityId);

			var actorRegion = ChannelServer.Instance.World.GetRegion(actor.RegionId);
			actorRegion.AddCreature(actor);

			actor.Unlock(Locks.Default, true);

			Send.PetUnregister(actor, rpCharacter);
			Send.Disappear(rpCharacter);

			var playerCreature = actor as PlayerCreature;
			playerCreature.StartLookAround();
		}
	}
}
