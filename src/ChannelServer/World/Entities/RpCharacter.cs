// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Network;
using Aura.Channel.Network.Sending;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    /// <summary>
    ///     Role-playing character, controlled by players.
    /// </summary>
    public class RpCharacter : PlayerCreature
    {
        /// <summary>
        ///     Creates new RP character, based on actor data.
        /// </summary>
        /// <param name="actorData">The data to base this character on.</param>
        /// <param name="actor">Character of the player who gets control over this RP character.</param>
        /// <param name="name">The RP character's full name, use null for default.</param>
        /// <example>
        ///     var character = new RpCharacter(AuraData.ActorDb.Find("#tarlach"), "Tarlach (FooBar)", fooBar);
        /// </example>
        public RpCharacter(ActorData actorData, Creature actor, string name)
        {
            if (actorData == null) throw new ArgumentNullException("actorData");
            if (actor == null) throw new ArgumentNullException("actor");

            if (actor.Client is DummyClient)
                throw new InvalidOperationException("Actor must be a player with a valid client.");

            var rnd = RandomProvider.Get();

            ActorData = actorData;
            Actor = actor;
            EntityId = NPC.GetNewNpcEntityId();
            RaceId = actorData.RaceId;
            LoadDefault();

            // Name
            if (!string.IsNullOrWhiteSpace(name))
            {
                Name = name;
            }
            else
            {
                // Use actor data's local name or fall back to race's name.
                var characterName = actorData.LocalName ?? RaceData.Name;
                var actorName = Actor.Name;

                Name = string.Format("{0} ({1})", Localization.Get(characterName), actorName);
            }

            // State
            State |= CreatureStates.InstantNpc;
            State |= CreatureStates.EnableCommonPvp;

            // Default location
            SetLocation(Actor.GetLocation());

            // Color
            if (actorData.HasColors)
            {
                Color1 = actorData.Color1;
                Color2 = actorData.Color2;
                Color3 = actorData.Color3;
            }

            // Body
            Height = actorData.Height;
            Weight = actorData.Weight;
            Upper = actorData.Upper;
            Lower = actorData.Lower;
            EyeColor = (byte) actorData.EyeColor;
            EyeType = (short) actorData.EyeType;
            MouthType = (byte) actorData.MouthType;
            SkinColor = (byte) actorData.SkinColor;

            // Titles
            Titles.SelectedTitle = (ushort) actorData.Title;

            // Stats
            Age = (short) actorData.Age;
            Level = (short) actorData.Level;
            Exp = AuraData.ExpDb.GetTotalForNextLevel(Level - 1);
            AbilityPoints = (short) actorData.AP;
            LifeMaxBase = actorData.Life;
            ManaMaxBase = actorData.Mana;
            StaminaMaxBase = actorData.Stamina;
            StrBase = actorData.Str;
            IntBase = actorData.Int;
            DexBase = actorData.Dex;
            WillBase = actorData.Will;
            LuckBase = actorData.Luck;

            // Hair and Face
            if (actorData.FaceItemId != 0)
            {
                var face = new Item(actorData.FaceItemId);
                face.Info.Color1 = (byte) actorData.SkinColor;
                Inventory.Add(face, Pocket.Face);
            }

            if (actorData.HairItemId != 0)
            {
                var hair = new Item(actorData.HairItemId);
                hair.Info.Color1 = actorData.HairColor;
                Inventory.Add(hair, Pocket.Hair);
            }

            // Items
            foreach (var itemData in actorData.Items)
            {
                var item = new Item(itemData.ItemId);
                item.Info.State = (byte) itemData.State;

                item.Info.Amount = (ushort) itemData.Amount;
                if (item.Data.StackType != StackType.Sac && item.Info.Amount < 1)
                    item.Info.Amount = 1;

                if (itemData.HasColors)
                {
                    item.Info.Color1 = itemData.Color1;
                    item.Info.Color2 = itemData.Color2;
                    item.Info.Color3 = itemData.Color3;
                }

                var pocket = itemData.Pocket;
                if (pocket != Pocket.None)
                    Inventory.Add(item, pocket);
            }

            // Skills
            Skills.Add(SkillId.CombatMastery, SkillRank.Novice, RaceId);

            foreach (var skillData in RaceData.Skills)
                Skills.Add((SkillId) skillData.SkillId, (SkillRank) skillData.Rank, RaceId);

            foreach (var skillData in actorData.Skills)
                Skills.Add(skillData.SkillId, skillData.Rank, RaceId);

            // Max stats out after skills and items were added (bonuses).
            Life = LifeMax;
            Mana = ManaMax;
            Stamina = StaminaMax;

            Client = actor.Client;
        }

        /// <summary>
        ///     Always returns false (RP characters don't save.)
        /// </summary>
        /// <remarks>
        ///     TODO: Replace property with a list in channel client?
        /// </remarks>
        public override bool Save => false;

        /// <summary>
        ///     Reference the actor data used to create this character.
        /// </summary>
        public ActorData ActorData { get; }

        /// <summary>
        ///     Reference to the creature playing this role.
        /// </summary>
        public Creature Actor { get; }

        /// <summary>
        ///     Returns false, since RP characters aren't allowed to
        ///     move equipment.
        /// </summary>
        public override bool CanMoveEquip => false;

        /// <summary>
        ///     Starts RP session, removing actor from world and adding the
        ///     RP character.
        /// </summary>
        public void Start()
        {
            var actor = Actor;
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
        ///     Ends RP session.
        /// </summary>
        public void End()
        {
            var actor = Actor;
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