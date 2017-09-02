// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Text;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Dungeons;
using Aura.Data;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
    public class CreatureDeadMenu
    {
        public CreatureDeadMenu(Creature creature)
        {
            Creature = creature;
        }

        public Creature Creature { get; }

        public ReviveOptions Options { get; set; }

        public void Add(ReviveOptions option)
        {
            Options |= option;
        }

        public bool Has(ReviveOptions option)
        {
            return (Options & option) != 0;
        }

        public void Clear()
        {
            Options = ReviveOptions.None;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if (Has(ReviveOptions.ArenaLobby))
                sb.Append("arena_lobby;");
            if (Has(ReviveOptions.ArenaSide))
                sb.Append("arena_side;");
            if (Has(ReviveOptions.ArenaWaitingRoom))
                sb.Append("arena_waiting;");
            if (Has(ReviveOptions.TirNaNog))
                sb.Append("tirnanog;");
            if (Has(ReviveOptions.BarriLobby))
                sb.Append("barri_lobby;");
            if (Has(ReviveOptions.NaoStone))
                sb.Append("naocoupon;");
            if (Has(ReviveOptions.DungeonEntrance))
                sb.Append("dungeon_lobby;");
            if (Has(ReviveOptions.Here))
                sb.Append("here;");
            if (Has(ReviveOptions.HereNoPenalty))
                sb.Append("trnsfrm_pvp_here;");
            if (Has(ReviveOptions.HerePvP))
                sb.Append("showdown_pvp_here;");
            if (Has(ReviveOptions.InCamp))
                sb.Append("camp;");
            if (Has(ReviveOptions.StatueOfGoddess))
                sb.Append("dungeon_statue;");
            if (Has(ReviveOptions.TirChonaill))
                sb.Append("tirchonaill;");
            if (Has(ReviveOptions.Town))
                sb.Append("town;");
            if (Has(ReviveOptions.WaitForRescue))
                sb.Append("stay;");

            return sb.ToString().Trim(';');
        }

        /// <summary>
        ///     Updates menu, based on where its creature is and updates
        ///     nearby clients.
        /// </summary>
        public void Update()
        {
            var before = Options;

            Clear();

            if (Creature.IsDead)
            {
                var isInTirNaNog = Creature.IsInTirNaNog;

                // Defaults
                Add(ReviveOptions.WaitForRescue);
                if (Creature.IsPet)
                    Add(ReviveOptions.PhoenixFeather);

                // Town
                if (!isInTirNaNog)
                {
                    Add(ReviveOptions.Town);
                }
                // Tir Na Nog
                else
                {
                    // Use TNN or Barri, depending on whether the bind quest
                    // was done.
                    if (Creature.Keywords.Has("g1_bind"))
                        Add(ReviveOptions.TirNaNog);
                    else
                        Add(ReviveOptions.BarriLobby);
                }

                // Nao Stone option if it's enabled, and creature is not
                // in Tir Na Nog.
                if (AuraData.FeaturesDb.IsEnabled("NaoCoupon") && !isInTirNaNog)
                    Add(ReviveOptions.NaoStone);

                // Dungeons
                if (Creature.Region is DungeonRegion)
                {
                    Add(ReviveOptions.DungeonEntrance);

                    // Show statue option only if there is a statue on this floor
                    var floorRegion = Creature.Region as DungeonFloorRegion;
                    if (floorRegion == null || floorRegion.Floor.Statue)
                        Add(ReviveOptions.StatueOfGoddess);
                }
                // Fields
                else
                {
                    if (!isInTirNaNog)
                        Add(ReviveOptions.Here);
                }

                // Special
                if (Creature.Titles.SelectedTitle == TitleId.devCAT)
                    Add(ReviveOptions.HereNoPenalty);
            }

            if (Options != before || Creature.IsDead)
                Send.DeadFeather(Creature);
        }

        /// <summary>
        ///     Returns exp penalty for given level and option.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="option"></param>
        /// <returns></returns>
        public int GetExpPenalty(int level, ReviveOptions option)
        {
            float optionMultiplicator;
            switch (option)
            {
                // Check both Nao Stone options for exp, as the exp aren't
                // reduced until the second option gets in.
                case ReviveOptions.NaoStone:
                    optionMultiplicator = 0.001f;
                    break;
                case ReviveOptions.NaoStoneRevive:
                    optionMultiplicator = 0.001f;
                    break;

                case ReviveOptions.PhoenixFeather:
                    optionMultiplicator = 0.0025f;
                    break;
                case ReviveOptions.DungeonEntrance:
                    optionMultiplicator = 0.005f;
                    break;
                case ReviveOptions.StatueOfGoddess:
                    optionMultiplicator = 0.0075f;
                    break;
                case ReviveOptions.Here:
                    optionMultiplicator = 0.01f;
                    break;

                case ReviveOptions.Town:
                    var premium = true;
                    if (premium)
                        goto default;
                    optionMultiplicator = 0.005f;
                    break;

                default: return 0;
            }

            float levelMultiplicator;
            if (level < 10) levelMultiplicator = 1;
            else if (level < 30) levelMultiplicator = 3;
            else if (level < 50) levelMultiplicator = 4;
            else if (level < 100) levelMultiplicator = 5;
            else levelMultiplicator = 6;

            var exp = AuraData.ExpDb.GetForLevel(level);

            return (int) (exp * optionMultiplicator * levelMultiplicator);
        }
    }
}