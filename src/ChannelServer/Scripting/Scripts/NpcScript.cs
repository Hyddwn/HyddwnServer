// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Util;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    public abstract class NpcScript : GeneralScript
    {
        private readonly CancellationTokenSource _cancellation;

        private Creature _player;
        private string _response;
        private readonly SemaphoreSlim _resumeSignal;

        /// <summary>
        ///     Initializes class
        /// </summary>
        protected NpcScript()
        {
            _resumeSignal = new SemaphoreSlim(0);
            _cancellation = new CancellationTokenSource();
        }

        public ConversationState ConversationState { get; private set; }

        /// <summary>
        ///     The NPC associated with this instance of the NPC script.
        /// </summary>
        public NPC NPC { get; set; }

        /// <summary>
        ///     The player associated with this instance of the NPC script.
        /// </summary>
        public Creature Player
        {
            get
            {
                if (_player == null)
                    throw new Exception("NpcScript: Missing player in " + GetType().Name);
                return _player;
            }
            set => _player = value;
        }

        /// <summary>
        ///     Gets or sets how well the NPC remembers the player.
        /// </summary>
        public int Memory
        {
            get => NPC.GetMemory(Player);
            set => NPC.SetMemory(Player, value);
        }

        /// <summary>
        ///     Gets or sets how much the NPC likes the player.
        /// </summary>
        public int Favor
        {
            get => NPC.GetFavor(Player);
            set => NPC.SetFavor(Player, value);
        }

        /// <summary>
        ///     Gets or sets how much the player stresses the NPC.
        /// </summary>
        public int Stress
        {
            get => NPC.GetStress(Player);
            set => NPC.SetStress(Player, value);
        }

        /// <summary>
        ///     Returns the player's current title.
        /// </summary>
        [Obsolete]
        public int Title => Player.Titles.SelectedTitle;

        /// <summary>
        ///     Gets and set the player's amount of gold,
        ///     by modifying the inventory.
        /// </summary>
        [Obsolete]
        public int Gold
        {
            get => Player.Inventory.Gold;
            set => Player.Inventory.Gold = value;
        }

        /// <summary>
        ///     Initiates the NPC script, creating and placing the NPC.
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            // Load first, to get race, location, etc.
            Load();

            if (NPC == null)
            {
                Log.Error("{0}: No race set.", GetType().Name);
                return false;
            }

            NPC.ScriptType = GetType();

            if (NPC.RegionId > 0)
            {
                var region = ChannelServer.Instance.World.GetRegion(NPC.RegionId);
                if (region == null)
                {
                    Log.Error("Failed to spawn '{0}', region '{1}' not found.", GetType().Name, NPC.RegionId);
                    return false;
                }

                // Add creature to region, unless the script already did it
                // for some reason.
                if (!region.CreatureExists(NPC.EntityId))
                    region.AddCreature(NPC);
            }

            NPC.SpawnLocation = new Location(NPC.RegionId, NPC.GetPosition());

            return true;
        }

        // ------------------------------------------------------------------

        /// <summary>
        ///     Called from packet handler when a player starts the conversation.
        /// </summary>
        public virtual async void TalkAsync()
        {
            ConversationState = ConversationState.Ongoing;
            try
            {
                if (!Player.IsPet)
                    await Talk();
                else
                    await TalkPet();
            }
            catch (OperationCanceledException)
            {
                // Thrown to get out of the async chain
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "NpcScript.TalkAsync");
                Close2("(Error)");
            }
            ConversationState = ConversationState.Ended;
        }

        /// <summary>
        ///     Called when a player starts the conversation.
        /// </summary>
        protected virtual async Task Talk()
        {
            await Task.Yield();
        }

        /// <summary>
        ///     Called when a pet starts the conversation.
        /// </summary>
        protected virtual async Task TalkPet()
        {
            // Officials don't use random messages, but one message for every NPC,
            // which is usually the default one below. However, some NPCs have a
            // different message, ones added later in particular, so we'll just
            // RNG it for the default message, less overriding for something
            // nobody cares about.

            switch (Random(3))
            {
                default:
                    if (NPC.IsMale)
                    {
                        Close(Hide.None, Localization.Get("(I don't think he can understand me.)"));
                        break;
                    }
                    else if (NPC.IsFemale)
                    {
                        Close(Hide.None, Localization.Get("(I don't think she can understand me.)"));
                        break;
                    }
                    else if (NPC.HasTag("/prop/"))
                    {
                        Close(Hide.None, Localization.Get("(I don't think I can talk to this.)"));
                        break;
                    }

                    // Go to next case if gender isn't clear
                    goto case 1;

                case 1:
                    Close(Hide.None, Localization.Get("(This conversation doesn't seem to be going anywhere.)"));
                    break;
                case 2:
                    Close(Hide.None, Localization.Get("(I don't think we'll see things eye to eye.)"));
                    break;
            }

            await Task.Yield();
        }

        /// <summary>
        ///     Called from packet handler when a player starts the conversation with a gift.
        /// </summary>
        public virtual async void GiftAsync(Item gift)
        {
            ConversationState = ConversationState.Ongoing;
            try
            {
                var score = GetGiftReaction(gift);

                // Debug output
                if (Player.IsDev)
                    Msg(string.Format("-Debug-<br/>Reaction: {0}<br/>Score: {1}", NPC.GiftWeights.CalculateScore(gift),
                        score));

                await Hook("before_gift", gift, score);

                await Gift(gift, score);
            }
            catch (OperationCanceledException)
            {
                // Thrown to get out of the async chain
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "NpcScript.GiftAsync");
                Close2("(Error)");
            }
            ConversationState = ConversationState.Ended;
        }

        /// <summary>
        ///     Called from Gift, override to react to gifts.
        /// </summary>
        /// <param name="gift">Item gifted to the NPC by the player.</param>
        /// <param name="reaction">NPCs reaction to the gift.</param>
        /// <returns></returns>
        protected virtual async Task Gift(Item gift, GiftReaction reaction)
        {
            Msg(Localization.Get("Thank you."));

            await Task.Yield();
        }

        /// <summary>
        ///     Returns NPCs reaction to gifted item.
        /// </summary>
        /// <param name="gift"></param>
        /// <returns></returns>
        protected virtual GiftReaction GetGiftReaction(Item gift)
        {
            var score = NPC.GiftWeights.CalculateScore(gift);

            if (gift.Info.Id == 51046) // Likeability pot
            {
                score = 10;
                Favor += 10; // Determined through a LOT of pots... RIP my bank :(
                Memory += 4; // Gotta remember who gave you roofies!!
            }
            else
            {
                var delta = score;

                if (gift.Data.StackType == StackType.Stackable)
                {
                    delta *= gift.Amount * gift.Data.StackMax * 3;
                    delta /= 1 + 2 * (Random(4) + 7);
                }
                else
                {
                    delta *= 3;
                    delta /= Random(7) + 6;
                }

                Favor += delta;
            }

            // Reduce stress by 0 ~ score (or at least 4) - 1 for good gifts
            if (score >= 0)
                Stress -= Random(Math.Max(4, score));

            if (score > 6)
                return GiftReaction.Love;
            if (score > 3)
                return GiftReaction.Like;
            if (score > -4)
                return GiftReaction.Neutral;
            return GiftReaction.Dislike;
        }

        /// <summary>
        ///     Sends Close, using either the message or the standard ending phrase.
        /// </summary>
        /// <param name="response"></param>
        public void End(string message = null)
        {
            Close(message ?? "(You ended your conversation with <npcname/>.)");
        }

        /// <summary>
        ///     Sets response and returns from Select.
        /// </summary>
        /// <param name="response"></param>
        public void Resume(string response)
        {
            _response = response;
            _resumeSignal.Release();
        }

        /// <summary>
        ///     Cancels conversation.
        /// </summary>
        public void Cancel()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        ///     Updates relation between NPC and Player based on current
        ///     relation values.
        /// </summary>
        /// <remarks>
        ///     Handles common update of relation values, which almost all NPCs
        ///     do, after greeting the player on the start of a Conversation.
        /// </remarks>
        public void UpdateRelationAfterGreet()
        {
            if (Memory <= 0)
            {
                Memory = 1;
            }
            else if (Memory == 1)
            {
            }
            else if (Memory <= 6 && Stress == 0)
            {
                Memory += 1;
                Stress += 5;
            }
            else if (Stress == 0)
            {
                Memory += 1;
                Stress += 10;
            }

            // Show relation values to devCATs for debugging
            if (Player.Titles.SelectedTitle == TitleId.devCAT)
                Msg(string.Format("-Debug-<br/>Favor: {0}<br/>Memory: {1}<br/>Stress: {2}", Favor, Memory, Stress));
        }

        /// <summary>
        ///     Gets the mood.
        /// </summary>
        /// <returns></returns>
        public virtual NpcMood GetMood()
        {
            var stress = Stress;
            var favor = Favor;
            var memory = Memory;

            if (stress > 12)
                return NpcMood.VeryStressed;
            if (stress > 8)
                return NpcMood.Stressed;
            if (favor > 40)
                return NpcMood.Love;
            if (favor > 30)
                return NpcMood.ReallyLikes;
            if (favor > 10)
                return NpcMood.Likes;
            if (favor < -20)
                return NpcMood.Hates;
            if (favor < -10)
                return NpcMood.ReallyDislikes;
            if (favor < -5)
                return NpcMood.Dislikes;

            if (memory > 15)
                return NpcMood.BestFriends;
            if (memory > 5)
                return NpcMood.Friends;

            return NpcMood.Neutral;
        }

        /// <summary>
        ///     Gets the mood string for the current mood.
        /// </summary>
        /// <returns></returns>
        public string GetMoodString()
        {
            return GetMoodString(GetMood());
        }

        /// <summary>
        ///     Gets the mood string for the given mood.
        /// </summary>
        /// <param name="mood">The mood.</param>
        /// <returns></returns>
        public virtual string GetMoodString(NpcMood mood)
        {
            string moodStr;

            switch (mood)
            {
                case NpcMood.VeryStressed:
                    moodStr = Localization.Get(
                        "(<npcname/> is giving me and impression that I am interruping something.)");
                    break;

                case NpcMood.Stressed:
                    moodStr = Localization.Get(
                        "(<npcname/> is giving me a look that it may be better to stop this conversation.)");
                    break;

                case NpcMood.BestFriends:
                    moodStr = Localization.Get("(<npcname/> is smiling at me as if we've known each other for years.)");
                    break;

                case NpcMood.Friends:
                    moodStr = Localization.Get("(<npcname/> is really giving me a friendly vibe.)");
                    break;

                case NpcMood.Hates:
                    moodStr = Rnd(
                        Localization.Get("(<npcname/> is looking at me like they don't want to see me.)"),
                        Localization.Get("(<npcname/> obviously hates me.)")
                    );
                    break;

                case NpcMood.ReallyDislikes:
                    moodStr = Localization.Get("(<npcname/> is looking at me with obvious disgust.)");
                    break;

                case NpcMood.Dislikes:
                    moodStr = Localization.Get("(<npcname/> looks like it's a bit unpleasent that I'm here.)");
                    break;

                case NpcMood.Likes:
                    moodStr = Localization.Get("(<npcname/> is looking at me with great interest.)");
                    break;

                case NpcMood.ReallyLikes:
                    moodStr = Localization.Get("(<npcname/> is giving me a friendly smile.)");
                    break;

                case NpcMood.Love:
                    moodStr = Localization.Get("(<npcname/> is giving me a welcome look.)");
                    break;

                default:
                    moodStr = Rnd(
                        Localization.Get("(<npcname/> is looking at me.)"),
                        Localization.Get("(<npcname/> is looking in my direction.)"),
                        Localization.Get("(<npcname/> is waiting for me to says something.)"),
                        Localization.Get("(<npcname/> is paying attention to me.)")
                    );
                    break;
            }

            // (<npcname/> is slowly looking me over.)

            return moodStr;
        }

        /// <summary>
        ///     Conversation (keywords) loop with initial mood message.
        /// </summary>
        /// <returns></returns>
        public virtual async Task StartConversation()
        {
            // Show mood once at the start of the conversation
            Msg(Hide.Name, GetMoodString(), FavorExpression());

            await Conversation();
        }

        /// <summary>
        ///     Conversation (keywords) loop.
        /// </summary>
        /// <remarks>
        ///     This is a separate method so it can be called from hooks
        ///     that go into keyword handling after they're done,
        ///     without mood message.
        /// </remarks>
        /// <returns></returns>
        public virtual async Task Conversation()
        {
            // Infinite keyword handling until End is clicked.
            while (true)
            {
                ShowKeywords();
                var keyword = await Select();

                if (keyword == "@end")
                    break;

                // Don't go into normal keyword handling if a hook handled
                // the keyword.
                var hooked = await Hook("before_keywords", keyword);
                if (hooked)
                    continue;

                await Keywords(keyword);
            }
        }

        /// <summary>
        ///     Called from conversation, keyword handling.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        protected virtual async Task Keywords(string keyword)
        {
            await Task.Yield();
        }

        /// <summary>
        ///     Modifies memory, favor, and stress and sends random reaction
        ///     message based on the favor change.
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="favor"></param>
        /// <param name="stress"></param>
        public virtual void ModifyRelation(int memory, int favor, int stress)
        {
            if (memory != 0) Memory += memory;
            if (favor != 0) Favor += favor;
            if (stress != 0) Stress += stress;

            // Seem to be multiple levels? -5, -2, 0, 2, 5?

            var msg = "";
            if (favor >= 0)
                msg = Rnd(
                    Localization.Get("(I think I left a good impression.)"),
                    Localization.Get("(The conversation drew a lot of interest.)"),
                    Localization.Get("(That was a great conversation!)")
                );
            else
                msg = Rnd(
                    Localization.Get("(A bit of frowning is evident.)"),
                    Localization.Get("(Seems like this person did not enjoy the conversation.)"),
                    Localization.Get("(A disapproving look, indeed.)")
                );

            Msg(Hide.Name, FavorExpression(), msg);
        }

        // Setup
        // ------------------------------------------------------------------		

        /// <summary>
        ///     Sets the gift weights.
        /// </summary>
        /// <param name="adult">How much the NPC likes "adult" items.</param>
        /// <param name="anime">How much the NPC likes "anime" items.</param>
        /// <param name="beauty">How much the NPC likes "beauty" items.</param>
        /// <param name="individuality">How much the NPC likes "indiv" items.</param>
        /// <param name="luxury">How much the NPC likes "luxury" items.</param>
        /// <param name="maniac">How much the NPC likes "maniac" items.</param>
        /// <param name="meaning">How much the NPC likes "meaning" items.</param>
        /// <param name="rarity">How much the NPC likes "rarity" items.</param>
        /// <param name="sexy">How much the NPC likes "sexy" items.</param>
        /// <param name="toughness">How much the NPC likes "toughness" items.</param>
        /// <param name="utility">How much the NPC likes "utility" items.</param>
        protected void SetGiftWeights(float adult, float anime, float beauty, float individuality, float luxury,
            float maniac, float meaning, float rarity, float sexy, float toughness, float utility)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.GiftWeights.Adult = adult;
            NPC.GiftWeights.Anime = anime;
            NPC.GiftWeights.Beauty = beauty;
            NPC.GiftWeights.Individuality = individuality;
            NPC.GiftWeights.Luxury = luxury;
            NPC.GiftWeights.Maniac = maniac;
            NPC.GiftWeights.Meaning = meaning;
            NPC.GiftWeights.Rarity = rarity;
            NPC.GiftWeights.Sexy = sexy;
            NPC.GiftWeights.Toughness = toughness;
            NPC.GiftWeights.Utility = utility;
        }

        /// <summary>
        ///     Sets NPC's name.
        /// </summary>
        /// <param name="name"></param>
        protected void SetName(string name)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.Name = name;
        }

        /// <summary>
        ///     Sets NPC's portrait.
        /// </summary>
        /// <param name="name"></param>
        protected void SetPortrait(string name)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.DialogPortrait = name;
        }

        /// <summary>
        ///     Sets NPC's location.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        protected void SetLocation(int regionId, int x, int y, byte direction = 0)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.SetLocation(regionId, x, y);
            NPC.Direction = direction;
        }

        /// <summary>
        ///     Sets NPC's race.
        /// </summary>
        /// <remarks>
        ///     This must be the first setup method called, since it creates the
        ///     NPC with its default settings.
        /// </remarks>
        /// <param name="raceId"></param>
        protected void SetRace(int raceId)
        {
            if (NPC != null)
                throw new InvalidOperationException("The NPC has a race already.");

            NPC = new NPC(raceId);
            NPC.State = CreatureStates.Npc | CreatureStates.NamedNpc | CreatureStates.GoodNpc;

            SetAi("npc_normal");
        }

        /// <summary>
        ///     Sets NPC's body proportions.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="weight"></param>
        /// <param name="upper"></param>
        /// <param name="lower"></param>
        protected void SetBody(float height = 1, float weight = 1, float upper = 1, float lower = 1)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.Height = height;
            NPC.Weight = weight;
            NPC.Upper = upper;
            NPC.Lower = lower;
        }

        /// <summary>
        ///     Sets NPC's face values.
        /// </summary>
        /// <param name="skinColor"></param>
        /// <param name="eyeType"></param>
        /// <param name="eyeColor"></param>
        /// <param name="mouthType"></param>
        protected void SetFace(byte skinColor = 0, short eyeType = 0, byte eyeColor = 0, byte mouthType = 0)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.SkinColor = skinColor;
            NPC.EyeType = eyeType;
            NPC.EyeColor = eyeColor;
            NPC.MouthType = mouthType;
        }

        /// <summary>
        ///     Sets NPC's color values.
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="color3"></param>
        protected void SetColor(uint color1 = 0x808080, uint color2 = 0x808080, uint color3 = 0x808080)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.Color1 = color1;
            NPC.Color2 = color2;
            NPC.Color3 = color3;
        }

        /// <summary>
        ///     Sets NPC's stand style.
        /// </summary>
        /// <param name="stand"></param>
        /// <param name="talkStand"></param>
        protected void SetStand(string stand, string talkStand = null)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.StandStyle = stand;
            NPC.StandStyleTalking = talkStand;
        }

        /// <summary>
        ///     Adds item to NPC's inventory.
        /// </summary>
        /// <param name="pocket"></param>
        /// <param name="itemId"></param>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="color3"></param>
        /// <param name="state">For robes and helmets</param>
        protected void EquipItem(Pocket pocket, int itemId, uint color1 = 0, uint color2 = 0, uint color3 = 0,
            ItemState state = ItemState.Up)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            if (!pocket.IsEquip())
            {
                Log.Error("Pocket '{0}' is not for equipment ({1})", pocket, GetType().Name);
                return;
            }

            if (!AuraData.ItemDb.Exists(itemId))
            {
                Log.Error("Unknown item '{0}' ({1})", itemId, GetType().Name);
                return;
            }

            var item = new Item(itemId);
            item.Info.Pocket = pocket;
            item.Info.Color1 = color1;
            item.Info.Color2 = color2;
            item.Info.Color3 = color3;
            item.Info.State = (byte) state;

            NPC.Inventory.InitAdd(item);
        }

        /// <summary>
        ///     Adds phrase to AI.
        /// </summary>
        /// <param name="phrase"></param>
        protected void AddPhrase(string phrase)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            if (NPC.AI != null)
                NPC.AI.Phrases.Add(phrase);
        }

        /// <summary>
        ///     Sets id of the NPC.
        /// </summary>
        /// <remarks>
        ///     Only required for NPCs like Nao and Tin, avoid if possible!
        /// </remarks>
        /// <param name="entityId"></param>
        protected void SetId(long entityId)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            NPC.EntityId = entityId;
        }

        /// <summary>
        ///     Pulls down the hood of all equipped robes.
        /// </summary>
        public void SetHoodDown()
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            var item = NPC.Inventory.GetItemAt(Pocket.Robe, 0, 0);
            if (item != null)
                item.Info.State = 1;
            item = NPC.Inventory.GetItemAt(Pocket.RobeStyle, 0, 0);
            if (item != null)
                item.Info.State = 1;
            item = NPC.Inventory.GetItemAt(Pocket.Armor, 0, 0);
            if (item != null)
                item.Info.State = 1;
            item = NPC.Inventory.GetItemAt(Pocket.ArmorStyle, 0, 0);
            if (item != null)
                item.Info.State = 1;
        }

        /// <summary>
        ///     Changes the NPC's AI.
        /// </summary>
        /// <param name="name"></param>
        public void SetAi(string name)
        {
            if (NPC == null)
                throw new InvalidOperationException("NPC's race has to be set first.");

            if (NPC.AI != null)
                NPC.AI.Dispose();

            NPC.AI = ChannelServer.Instance.ScriptManager.AiScripts.CreateAi(name, NPC);
            if (NPC.AI == null)
                Log.Error("NpcScript.SetAi: AI '{0}' not found ({1})", name, GetType().Name);
        }

        // Functions
        // ------------------------------------------------------------------

        /// <summary>
        ///     Sends Msg with Bgm element.
        /// </summary>
        /// <param name="fileName"></param>
        protected void SetBgm(string fileName)
        {
            Msg(new DialogBgm(fileName));
        }

        /// <summary>
        ///     Opens shop for player.
        /// </summary>
        /// <param name="typeName"></param>
        protected void OpenShop(string typeName)
        {
            var shop = ChannelServer.Instance.ScriptManager.NpcShopScripts.Get(typeName);
            if (shop == null)
            {
                Log.Unimplemented("Missing shop: {0}", typeName);
                Close("(Missing shop.)");
                return;
            }

            shop.OpenFor(Player, NPC);
        }

        /// <summary>
        ///     Joins lines and sends them as Msg,
        ///     but only once per creature and NPC per session.
        /// </summary>
        /// <param name="lines"></param>
        protected async Task Intro(params object[] lines)
        {
            if (Player.Vars.Temp["npc_intro:" + NPC.Name] == null)
            {
                // Explicit button and Select, so we don't get into the hooks
                // (that might do more than sending msgs) without clicking.
                Msg(Hide.Both, string.Join("<br/>", lines), Button("Continue"));
                await Select();
                Player.Vars.Temp["npc_intro:" + NPC.Name] = true;
            }

            await Hook("after_intro");
        }

        /// <summary>
        ///     Execute Hook! Harhar.
        /// </summary>
        /// <remarks>
        ///     Runs all hook funcs, one by one.
        /// </remarks>
        /// <param name="hookName"></param>
        /// <param name="args"></param>
        /// <returns>Whether a hook was executed and broke execution.</returns>
        protected async Task<bool> Hook(string hookName, params object[] args)
        {
            // Not hooked if no hooks found
            var hooks = ChannelServer.Instance.ScriptManager.NpcScriptHooks.Get(NPC.Name, hookName);
            if (hooks == null)
                return false;

            foreach (var hook in hooks)
            {
                var result = await hook(this, args);
                switch (result)
                {
                    case HookResult.Continue: continue; // Run next hook
                    case HookResult.Break: return true; // Stop and go back into the NPC
                    case HookResult.End:
                        Exit();
                        return true; // Exit script
                }
            }

            // Not hooked if no break or end.
            // XXX: Technically a script could do something and return
            //   Continue, which would make it hooked without break,
            //   but you really shouldn't continue on hook, it would lead
            //   to confusing dialogues... Maybe add a second Continue type,
            //   in case we actually need it.
            return false;
        }

        /// <summary>
        ///     Redeems code if found.
        /// </summary>
        /// <param name="code"></param>
        public bool RedeemCoupon(string code)
        {
            var script = ChannelServer.Instance.Database.GetCouponScript(code);
            if (script == null) return false;

            if (string.IsNullOrWhiteSpace(script))
            {
                Log.Error("CheckCouponCode: Empty script in '{0}'", code);
                return false;
            }

            var splitted = script.Split(':');
            if (splitted.Length < 2)
            {
                Log.Error("CheckCouponCode: Invalid script '{0}' in '{1}'", script, code);
                return false;
            }

            switch (splitted[0])
            {
                case "item":
                    int itemId;
                    if (!int.TryParse(splitted[1], out itemId))
                        return false;

                    var item = new Item(itemId);
                    Player.Inventory.Add(item, true);
                    Send.AcquireItemInfo(Player, item.EntityId);

                    break;

                case "title":
                    ushort titleId;
                    if (!ushort.TryParse(splitted[1], out titleId))
                        return false;
                    Player.Titles.Enable(titleId);
                    break;

                case "card":
                    int cardId;
                    if (!int.TryParse(splitted[1], out cardId))
                        return false;
                    ChannelServer.Instance.Database.AddCard(Player.Client.Account.Id, cardId, 0);
                    break;

                case "petcard":
                    int raceId;
                    if (!int.TryParse(splitted[1], out raceId))
                        return false;
                    ChannelServer.Instance.Database.AddCard(Player.Client.Account.Id, MabiId.PetCardType, raceId);
                    break;

                default:
                    Log.Error("CheckCouponCode: Unknown script type '{0}' in '{1}'", splitted[0], code);
                    return false;
            }

            ChannelServer.Instance.Database.UseCoupon(code);

            return true;
        }

        /// <summary>
        ///     Opens bank window.
        /// </summary>
        /// <param name="bankId">The unique identifier for the bank to open.</param>
        /// <param name="bankTitle">The title of the bank to open.</param>
        public void OpenBank(string bankId)
        {
            // Previously we used these two for id and title, which allowed
            // access to anything from anywhere. Make this an option?
            //packet.PutString("Global");
            //packet.PutString("Bank");

            if (!AuraData.BankDb.Exists(bankId))
            {
                Log.Warning("OpenBank: Unknown bank '{0}'", bankId);
                Msg(string.Format("(Error: Unknown bank '{0}')", bankId));
                return;
            }

            var bankTitle = BankInventory.GetName(bankId);

            // Override bank if global bank is activated
            if (ChannelServer.Instance.Conf.World.GlobalBank)
            {
                bankId = "Global";
                bankTitle = L("Global Bank");
            }

            Player.Temp.CurrentBankId = bankId;
            Player.Temp.CurrentBankTitle = bankTitle;

            Send.OpenBank(Player, Player.Client.Account.Bank, BankTabRace.Human, bankId, bankTitle);
        }

        /// <summary>
        ///     Tries to repair item specified in the repair reply.
        /// </summary>
        /// <param name="repairReply"></param>
        /// <param name="rate"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public RepairResult Repair(string repairReply, int rate, params string[] tags)
        {
            var result = new RepairResult();

            // Response ends with one '#' if the player selected 100%
            // success rate.
            if (repairReply.EndsWith("#"))
            {
                if (!IsEnabled("RepairRenewal"))
                {
                    Send.MsgBox(Player, Localization.Get("The 100% repair feature has not been enabled yet."));
                    return result;
                }

                repairReply = repairReply.TrimEnd('#');
                rate = 100;
                result.Used100Rate = true;
            }

            // Repair everything
            if (repairReply == "@repair_everything")
            {
                Send.MsgBox(Player, Localization.Get("Repairing everything at once is not supported yet."));
                return result;
            }

            // Get item id: @repair(_all):123456789
            var pos = -1;
            if ((pos = repairReply.IndexOf(':')) == -1 ||
                !long.TryParse(repairReply.Substring(pos + 1), out result.ItemEntityId))
            {
                Log.Warning("NpcScript.Repair: Player '{0:X16}' (Account: {1}) sent invalid repair reply.",
                    Player.EntityId, Player.Client.Account.Id);
                return result;
            }

            // Perfect repair?
            var all = repairReply.StartsWith("@repair_all");

            // Get item
            result.Item = Player.Inventory.GetItem(result.ItemEntityId);
            if (result.Item == null || !tags.Any(a => result.Item.Data.HasTag(a)))
            {
                Log.Warning("NpcScript.Repair: Player '{0:X16}' (Account: {1}) tried to repair invalid item.",
                    Player.EntityId, Player.Client.Account.Id);
                return result;
            }

            // Calculate points to repair
            result.Points = !all ? 1000 : result.Item.OptionInfo.DurabilityMax - result.Item.OptionInfo.Durability;
            result.Points = (int) Math.Floor(result.Points / 1000f);

            // Check gold
            var cost = result.Item.GetRepairCost(rate, 1);
            if (Player.Inventory.Gold < cost * result.Points)
            {
                result.HadGold = false;
                return result;
            }

            result.HadGold = true;

            // TODO: Luck?

            // Repair x times
            for (var i = 0; i < result.Points; ++i)
            {
                var useRate = rate;

                // Holy Water
                if (result.Item.IsBlessed)
                    useRate = 100 - (100 - useRate) / 2;

                // 100% Repair Event
                if (IsEventActive("all_repairrate_100"))
                    useRate = 100;

                // Success
                if (Random(100) < useRate)
                {
                    result.Item.Durability += 1000;
                    result.Successes++;
                }
                // Fail
                else
                {
                    // Remove blessing 
                    if (result.Item.IsBlessed)
                    {
                        result.Item.OptionInfo.Flags &= ~ItemFlags.Blessed;
                        Send.ItemBlessed(Player, result.Item);
                    }

                    result.Item.OptionInfo.DurabilityMax = Math.Max(1000, result.Item.OptionInfo.DurabilityMax - 1000);
                    if (result.Item.OptionInfo.DurabilityMax < result.Item.OptionInfo.Durability)
                        result.Item.Durability -= 1000;
                    result.Fails++;
                }
            }

            // Reduce gold, but only for successes
            Player.Inventory.Gold -= cost * result.Successes;

            // Update max dura
            if (result.Fails != 0)
                Send.ItemMaxDurabilityUpdate(Player, result.Item);

            // Update  dura
            if (result.Successes != 0)
                Send.ItemDurabilityUpdate(Player, result.Item);

            // Send result
            Send.ItemRepairResult(Player, result.Item, result.Successes);

            Player.Keywords.Give("ExperienceRepair");

            return result;
        }

        /// <summary>
        ///     Tries to upgrade item specified in the reply.
        /// </summary>
        /// <param name="upgradeReply"></param>
        /// <returns></returns>
        /// <remarks>
        ///     Only warn when something goes wrong, because problems can be caused
        ///     by replies unknown to us or an outdated database.
        ///     The NPCs don't have replies for failed upgrades, because the client
        ///     disables invalid upgrades, you shouldn't be able to get a fail,
        ///     unless you "hacked", modified client files, or Aura is outdated.
        /// </remarks>
        public UpgradeResult Upgrade(string upgradeReply)
        {
            var result = new UpgradeResult();

            // Example: @upgrade:22518872341757176:broad_sword_balance1
            var args = upgradeReply.Split(new[] {':'}, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 3 || !long.TryParse(args[1], out result.ItemEntityId))
            {
                Log.Warning("NpcScript.Upgrade: Player '{0:X16}' (Account: {1}) sent invalid reply.", Player.EntityId,
                    Player.Client.Account.Id);
                return result;
            }

            // Get item
            result.Item = Player.Inventory.GetItem(result.ItemEntityId);
            if (result.Item == null || result.Item.OptionInfo.Upgraded == result.Item.OptionInfo.UpgradeMax)
            {
                Log.Warning("NpcScript.Upgrade: Player '{0:X16}' (Account: {1}) tried to upgrade invalid item.",
                    Player.EntityId, Player.Client.Account.Id);
                return result;
            }

            // Get upgrade and check item and NPCs
            result.Upgrade = AuraData.ItemUpgradesDb.Find(args[2]);
            if (result.Upgrade == null)
            {
                Log.Warning(
                    "NpcScript.Upgrade: Player '{0:X16}' (Account: {1}) tried to apply an unknown upgrade ({2}).",
                    Player.EntityId, Player.Client.Account.Id, args[2]);
                return result;
            }

            // Check upgrade and item
            if (!result.Item.Data.HasTag(result.Upgrade.Filter) || result.Item.Proficiency < result.Upgrade.Exp ||
                !Math2.Between(result.Item.OptionInfo.Upgraded, result.Upgrade.UpgradeMin, result.Upgrade.UpgradeMax))
            {
                Log.Warning(
                    "NpcScript.Upgrade: Player '{0:X16}' (Account: {1}) tried to apply upgrade to invalid item.",
                    Player.EntityId, Player.Client.Account.Id);
                return result;
            }
            if (!result.Upgrade.Npcs.Contains(NPC.Name.TrimStart('_').ToLower()))
            {
                Log.Warning(
                    "NpcScript.Upgrade: Player '{0:X16}' (Account: {1}) tried to apply upgrade '{2}' at an invalid NPC ({3}).",
                    Player.EntityId, Player.Client.Account.Id, result.Upgrade.Ident, NPC.Name.TrimStart('_').ToLower());
                return result;
            }

            // Check for disabled Artisan
            if (result.Upgrade.Effects.Any(a => a.Key == "Artisan") && !IsEnabled("ArtisanUpgrade"))
            {
                Send.MsgBox(Player, Localization.Get("Artisan upgrades are unavailable."));
                return result;
            }

            // Check gold
            if (Player.Inventory.Gold < result.Upgrade.Gold)
                return result;

            // Take gold and exp
            result.Item.Proficiency -= result.Upgrade.Exp;
            Player.Inventory.Gold -= result.Upgrade.Gold;

            // Increase upgrade count
            result.Item.OptionInfo.Upgraded++;
            if (ChannelServer.Instance.Conf.World.UnlimitedUpgrades &&
                result.Item.OptionInfo.Upgraded == result.Item.OptionInfo.UpgradeMax)
                result.Item.OptionInfo.Upgraded = 0;

            // Upgrade
            foreach (var effect in result.Upgrade.Effects)
                switch (effect.Key)
                {
                    case "MinAttack":
                        result.Item.OptionInfo.AttackMin = (ushort) Math2.Clamp(1, result.Item.OptionInfo.AttackMax,
                            result.Item.OptionInfo.AttackMin + effect.Value[0]);
                        break;
                    case "MaxAttack":
                        result.Item.OptionInfo.AttackMax = (ushort) Math2.Clamp(1, ushort.MaxValue,
                            result.Item.OptionInfo.AttackMax + effect.Value[0]);
                        if (result.Item.OptionInfo.AttackMax < result.Item.OptionInfo.AttackMin)
                            result.Item.OptionInfo.AttackMin = result.Item.OptionInfo.AttackMax;
                        break;

                    case "MinInjury":
                        result.Item.OptionInfo.InjuryMin = (ushort) Math2.Clamp(0, result.Item.OptionInfo.InjuryMax,
                            result.Item.OptionInfo.InjuryMin + effect.Value[0]);
                        break;
                    case "MaxInjury":
                        result.Item.OptionInfo.InjuryMax = (ushort) Math2.Clamp(0, ushort.MaxValue,
                            result.Item.OptionInfo.InjuryMax + effect.Value[0]);
                        if (result.Item.OptionInfo.InjuryMax < result.Item.OptionInfo.InjuryMin)
                            result.Item.OptionInfo.InjuryMin = result.Item.OptionInfo.InjuryMax;
                        break;

                    case "Balance":
                        result.Item.OptionInfo.Balance = (byte) Math2.Clamp(0, byte.MaxValue,
                            result.Item.OptionInfo.Balance + effect.Value[0]);
                        break;
                    case "Critical":
                        result.Item.OptionInfo.Critical = (sbyte) Math2.Clamp(0, sbyte.MaxValue,
                            result.Item.OptionInfo.Critical + effect.Value[0]);
                        break;
                    case "Defense":
                        result.Item.OptionInfo.Defense = (int) Math2.Clamp(0, int.MaxValue,
                            result.Item.OptionInfo.Defense + (long) effect.Value[0]);
                        break;
                    case "Protection":
                        result.Item.OptionInfo.Protection = (short) Math2.Clamp(0, short.MaxValue,
                            result.Item.OptionInfo.Protection + effect.Value[0]);
                        break;
                    case "AttackRange":
                        result.Item.OptionInfo.EffectiveRange = (short) Math2.Clamp(0, short.MaxValue,
                            result.Item.OptionInfo.EffectiveRange + effect.Value[0]);
                        break;

                    case "MaxDurability":
                        result.Item.OptionInfo.DurabilityMax = (int) Math2.Clamp(1000, int.MaxValue,
                            result.Item.OptionInfo.DurabilityMax + (long) (effect.Value[0] * 1000));
                        if (result.Item.OptionInfo.DurabilityMax < result.Item.OptionInfo.Durability)
                            result.Item.OptionInfo.Durability = result.Item.OptionInfo.DurabilityMax;
                        break;

                    case "MagicDefense":
                        // MDEF:f:1.000000;MPROT:f:1.000000;MTWR:1:1;
                        var mdef = result.Item.MetaData1.GetFloat("MDEF");
                        result.Item.MetaData1.SetFloat("MDEF", Math2.Clamp(0, int.MaxValue, mdef + effect.Value[0]));
                        break;

                    case "MagicProtection":
                        // MDEF:f:1.000000;MPROT:f:1.000000;MTWR:1:1;
                        var mprot = result.Item.MetaData1.GetFloat("MPROT");
                        result.Item.MetaData1.SetFloat("MPROT", Math2.Clamp(0, int.MaxValue, mprot + effect.Value[0]));
                        break;

                    case "ManaUse":
                        // WU:s:00000003000000
                        var manaUseWU = new WUUpgrades(result.Item.MetaData1.GetString("WU"));
                        manaUseWU.ManaUse += (sbyte) effect.Value[0];
                        result.Item.MetaData1.SetString("WU", manaUseWU.ToString());
                        break;

                    case "ManaBurn":
                        var manaBurnWU = new WUUpgrades(result.Item.MetaData1.GetString("WU"));

                        // Prior to G15S2 players lost all their Mana when
                        // they unequipped a wand. This was removed via
                        // feature, but before that this upgrade allowed
                        // one to reduce the amount of Mana lost.
                        // Afterwards the ManaBurn upgrade was turned into
                        // a ManaUse automatically, but the bonus was halfed,
                        // meaning if a ManaBurn upgrade gave -4% burn,
                        // it gave -2% use after this update.
                        if (!IsEnabled("ManaBurnRemove"))
                            manaBurnWU.ManaBurn += (sbyte) effect.Value[0];
                        else
                            manaBurnWU.ManaUse += (sbyte) (effect.Value[0] / 2);

                        result.Item.MetaData1.SetString("WU", manaBurnWU.ToString());
                        break;

                    case "ChainCasting":
                        // Chain Casting: +4, Magic Attack: +21
                        // EHLV:4:5;MTWR:1:1;OWNER:s:username;WU:s:30201400000015;
                        var chainCastWU = new WUUpgrades(result.Item.MetaData1.GetString("WU"));
                        chainCastWU.ChainCastSkillId = (ushort) effect.Value[0];
                        chainCastWU.ChainCastLevel = (byte) effect.Value[1];
                        result.Item.MetaData1.SetString("WU", chainCastWU.ToString());
                        break;

                    case "MagicDamage":
                        // Charging Speed: +12%, MA: +16
                        // EHLV:4:5;MTWR:1:1;OWNER:s:username;WU:s:00000000000c10;
                        var magicDmgWU = new WUUpgrades(result.Item.MetaData1.GetString("WU"));
                        magicDmgWU.MagicDamage += (sbyte) effect.Value[0];
                        result.Item.MetaData1.SetString("WU", magicDmgWU.ToString());
                        break;

                    case "CastingSpeed":
                        // Charging Speed: +12%, MA: +16
                        // EHLV:4:5;MTWR:1:1;OWNER:s:username;WU:s:00000000000c10;
                        var castingSpeedWU = new WUUpgrades(result.Item.MetaData1.GetString("WU"));
                        castingSpeedWU.CastingSpeed += (sbyte) effect.Value[0];
                        result.Item.MetaData1.SetString("WU", castingSpeedWU.ToString());
                        break;

                    case "MusicBuffBonus":
                        // MBB:4:8;MBD:4:10;MTWR:1:2;OTU:1:1;SPTEC:1:1;
                        var musicBuff = result.Item.MetaData1.GetInt("MBB");
                        result.Item.MetaData1.SetInt("MBB", musicBuff + (int) effect.Value[0]);
                        break;

                    case "MusicBuffDuration":
                        // MBB:4:8;MBD:4:10;MTWR:1:2;OTU:1:1;SPTEC:1:1;
                        var musicBuffDur = result.Item.MetaData1.GetInt("MBD");
                        result.Item.MetaData1.SetInt("MBD", musicBuffDur + (int) effect.Value[0]);
                        break;

                    case "CollectionBonus":
                        // CTBONUS:2:40;CTSPEED:4:750;MTWR:1:1;
                        var collectionBonusBuff = result.Item.MetaData1.GetShort("CTBONUS");
                        result.Item.MetaData1.SetShort("CTBONUS", (short) (collectionBonusBuff + effect.Value[0]));
                        break;

                    case "CollectionBonusProduct":
                        // CTBONUSPT:4:64004;CTBONUS:2:20;
                        result.Item.MetaData1.SetInt("CTBONUSPT", (int) effect.Value[0]);
                        break;

                    case "CollectionSpeed":
                        // CTBONUS:2:40;CTSPEED:4:750;MTWR:1:1;
                        var collectionSpeedBuff = result.Item.MetaData1.GetInt("CTSPEED");
                        result.Item.MetaData1.SetInt("CTSPEED", collectionSpeedBuff + (int) effect.Value[0]);
                        break;

                    case "LancePiercing":
                        // EHLV:4:5;LKUP:8:262244;LP:1:4;LP_E:1:0;OWNER:s:character;SPTRP:1:1;   << Piercing Level 4
                        // LP:1:1;QUAL:4:70;   << Piercing Level 1
                        var lancePiercingBuff = result.Item.MetaData1.GetByte("LP");
                        result.Item.MetaData1.SetByte("LP", (byte) (lancePiercingBuff + effect.Value[0]));
                        break;

                    case "SplashRadius":
                        // SP_DMG:f:0.250000;SP_RAD:4:70;
                        var splashRadiusBuff = result.Item.MetaData1.GetInt("SP_RAD");
                        result.Item.MetaData1.SetInt("SP_RAD", splashRadiusBuff + (int) effect.Value[0]);
                        break;

                    case "SplashDamage":
                        // SP_DMG:f:0.250000;SP_RAD:4:70;
                        var splashDamageBuff = result.Item.MetaData1.GetFloat("SP_DMG");
                        result.Item.MetaData1.SetFloat("SP_DMG", splashDamageBuff + effect.Value[0]);
                        break;

                    case "ImmuneMelee":
                        // IM_MGC:f:0.050000;IM_MLE:f:0.050000;IM_RNG:f:0.050000;MDEF:f:2.000000;MPROT:f:3.000000;OTU:1:1;
                        var immuneMeleeBuff = result.Item.MetaData1.GetFloat("IM_MLE");
                        result.Item.MetaData1.SetFloat("IM_MLE", immuneMeleeBuff + effect.Value[0]);
                        break;

                    case "ImmuneRanged":
                        // IM_MGC:f:0.050000;IM_MLE:f:0.050000;IM_RNG:f:0.050000;MDEF:f:2.000000;MPROT:f:3.000000;OTU:1:1;
                        var immuneRangedBuff = result.Item.MetaData1.GetFloat("IM_RNG");
                        result.Item.MetaData1.SetFloat("IM_RNG", immuneRangedBuff + effect.Value[0]);
                        break;

                    case "ImmuneMagic":
                        // IM_MGC:f:0.050000;IM_MLE:f:0.050000;IM_RNG:f:0.050000;MDEF:f:2.000000;MPROT:f:3.000000;OTU:1:1;
                        var immuneMagicBuff = result.Item.MetaData1.GetFloat("IM_MGC");
                        result.Item.MetaData1.SetFloat("IM_MGC", immuneMagicBuff + effect.Value[0]);
                        break;

                    case "Artisan":
                        var artisanId = (int) result.Upgrade.Effects["Artisan"][0];
                        var artisanData = AuraData.ArtisanUpgradesDb.Find(artisanId);
                        if (artisanData == null)
                        {
                            Log.Warning("NpcScript.Upgrade: Upgrade '{0}' references unknown Artisan upgrade '{1}'.",
                                result.Upgrade.Ident, artisanId);
                            break;
                        }

                        // Choose a random number of random effect sets to apply to item.
                        // Has to be at least one of the random effect sets.
                        if (artisanData.Random.Count > 0)
                        {
                            var randomEffectSet = UniqueRnd(Random(artisanData.Random.Count) + 1,
                                artisanData.Random.ToArray());
                            foreach (var setid in randomEffectSet)
                            {
                                var optionSetData = AuraData.OptionSetDb.Find(setid);
                                if (optionSetData == null)
                                {
                                    Log.Warning(
                                        "NpcScript.Upgrade: Artisan upgrade '{0}' references unknown option set '{1}'.",
                                        artisanId, setid);
                                    continue;
                                }

                                result.Item.ApplyOptionSet(AuraData.OptionSetDb.Find(setid), false);
                            }
                        }

                        // 'Always' effect sets will always be applied to item.
                        foreach (var setid in artisanData.Always)
                        {
                            var optionSetData = AuraData.OptionSetDb.Find(setid);
                            if (optionSetData == null)
                            {
                                Log.Warning(
                                    "NpcScript.Upgrade: Artisan upgrade '{0}' references unknown option set '{1}'.",
                                    artisanId, setid);
                                continue;
                            }

                            result.Item.ApplyOptionSet(AuraData.OptionSetDb.Find(setid), false);
                        }
                        break;

                    // TODO:
                    // - MaxBullets

                    default:
                        Log.Unimplemented("Item upgrade '{0}'", effect.Key);
                        break;
                }

            // Personalization
            if (result.Upgrade.Personalize)
            {
                result.Item.OptionInfo.Flags |= ItemFlags.Personalized;
                result.Item.MetaData1.SetString("OWNER", Player.Name);
            }

            // Update item
            Send.ItemUpdate(Player, result.Item);

            // Send result
            Send.ItemUpgradeResult(Player, result.Item, result.Upgrade.Ident);

            result.Success = true;

            Player.Keywords.Give("ExperienceUpgrade");

            return result;
        }

        /// <summary>
        ///     Opens guild robe creation interface.
        /// </summary>
        public void OpenGuildRobeCreation()
        {
            var entityId = Player.EntityId;
            var guildName = "?";
            var color = 0x000000u;

            var guild = Player.Guild;
            if (guild != null)
                guildName = guild.Name;

            var rnd = new MTRandom(ErinnTime.Now.DateTimeStamp);
            color = AuraData.ColorMapDb.GetRandom(1, rnd);

            Player.Vars.Temp["GuildRobeColor"] = color;

            Send.GuildOpenGuildRobeCreation(Player, entityId, guildName, color);
        }

        // Player Methods
        // ------------------------------------------------------------------

        /// <summary>
        ///     Adds item(s) to player's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Obsolete]
        public bool GiveItem(int itemId, int amount = 1)
        {
            return Player.GiveItem(itemId, amount);
        }

        /// <summary>
        ///     Adds item to player's inventory.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        [Obsolete]
        public bool GiveItem(Item item)
        {
            return Player.GiveItem(item);
        }

        /// <summary>
        ///     Adds warp scroll to player's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="portal"></param>
        /// <returns></returns>
        [Obsolete]
        public bool GiveWarpScroll(int itemId, string portal)
        {
            return Player.GiveWarpScroll(itemId, portal);
        }

        /// <summary>
        ///     Adds item to player's inventory and shows an acquire window.
        /// </summary>
        /// <param name="itemId"></param>
        [Obsolete]
        public void AcquireItem(int itemId)
        {
            Player.AcquireItem(itemId);
        }

        /// <summary>
        ///     Adds given amount of gold to the player's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Obsolete]
        public bool GiveGold(int amount)
        {
            return Player.GiveGold(amount);
        }

        /// <summary>
        ///     Adds an item to player's inventory with specific colors.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="color3"></param>
        /// <returns></returns>
        [Obsolete]
        public bool GiveItem(int itemId, uint color1, uint color2, uint color3)
        {
            var item = new Item(itemId);
            item.Info.Color1 = color1;
            item.Info.Color2 = color2;
            item.Info.Color3 = color3;

            return Player.GiveItem(item);
        }

        /// <summary>
        ///     Removes item(s) from a player's inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Obsolete]
        public bool RemoveItem(int itemId, int amount = 1)
        {
            return Player.RemoveItem(itemId, amount);
        }

        /// <summary>
        ///     Checks if player has item(s) in their inventory.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Obsolete]
        public bool HasItem(int itemId, int amount = 1)
        {
            return Player.HasItem(itemId, amount);
        }

        /// <summary>
        ///     Checks if player has the skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        [Obsolete]
        public bool HasSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            return Player.HasSkill(skillId, rank);
        }

        /// <summary>
        ///     Checks if player has the skill on the specified rank.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        [Obsolete]
        public bool IsSkill(SkillId skillId, SkillRank rank)
        {
            return Player.IsSkill(skillId, rank);
        }

        /// <summary>
        ///     Gives skill to player if he doesn't have it on that rank yet.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="rank"></param>
        [Obsolete]
        public void GiveSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            Player.GiveSkill(skillId, rank);
        }

        /// <summary>
        ///     Trains the specified condition for skill by one.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="condition"></param>
        [Obsolete]
        protected void TrainSkill(SkillId skillId, int condition)
        {
            Player.TrainSkill(skillId, condition);
        }

        /// <summary>
        ///     Returns true if quest is in progress.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        [Obsolete]
        public bool QuestActive(int questId, string objective = null)
        {
            return Player.QuestActive(questId, objective);
        }

        /// <summary>
        ///     Returns true if player has quest, completed or not.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        [Obsolete]
        public bool HasQuest(int questId)
        {
            return Player.HasQuest(questId);
        }

        /// <summary>
        ///     Returns true if quest was completed.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        [Obsolete]
        public bool QuestCompleted(int questId)
        {
            return Player.QuestCompleted(questId);
        }

        /// <summary>
        ///     Returns true if player has quest, but it's not done yet,
        ///     or hasn't been completed.
        /// </summary>
        /// <param name="questId"></param>
        /// <returns></returns>
        [Obsolete]
        public bool QuestActiveUncompleted(int questId)
        {
            return Player.QuestInProgress(questId);
        }

        /// <summary>
        ///     Finishes objective in quest.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        [Obsolete]
        public bool FinishQuest(int questId, string objective)
        {
            return Player.FinishQuestObjective(questId, objective);
        }

        /// <summary>
        ///     Returns current quest objective.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="objective"></param>
        /// <returns></returns>
        [Obsolete]
        public string QuestObjective(int questId)
        {
            return Player.GetCurrentQuestObjective(questId);
        }

        /// <summary>
        ///     Starts quest.
        /// </summary>
        /// <param name="questId"></param>
        [Obsolete]
        public void StartQuest(int questId)
        {
            Player.StartQuest(questId);
        }

        /// <summary>
        ///     Sends quest to player via owl.
        /// </summary>
        /// <param name="questId"></param>
        [Obsolete]
        public void SendOwl(int questId)
        {
            Player.SendOwl(questId);
        }

        /// <summary>
        ///     Sends quest to player via owl after the delay.
        /// </summary>
        /// <param name="questId"></param>
        /// <param name="delay">Arrival delay in seconds.</param>
        [Obsolete]
        public void SendOwl(int questId, int delay)
        {
            Player.SendOwl(questId, delay);
        }

        /// <summary>
        ///     Completes quest (incl rewards).
        /// </summary>
        /// <param name="questId"></param>
        [Obsolete]
        public void CompleteQuest(int questId)
        {
            Player.CompleteQuest(questId);
        }

        /// <summary>
        ///     Starts PTJ quest.
        /// </summary>
        /// <param name="questId"></param>
        [Obsolete]
        public void StartPtj(int questId)
        {
            Player.StartPtj(questId, NPC.Name);
        }

        /// <summary>
        ///     Completes PTJ quest, if one is active. Rewards the selected rewards.
        /// </summary>
        /// <param name="rewardReply">Example: @reward:0</param>
        [Obsolete]
        public void CompletePtj(string rewardReply)
        {
            Player.CompletePtj(rewardReply);
        }

        /// <summary>
        ///     Gives up Ptj (fail without rewards).
        /// </summary>
        [Obsolete]
        public void GiveUpPtj()
        {
            Player.GiveUpPtj();
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active and its type matches
        ///     the given one.
        /// </summary>
        [Obsolete]
        public bool DoingPtj(PtjType type)
        {
            return Player.IsDoingPtj(type);
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active.
        /// </summary>
        [Obsolete]
        public bool DoingPtj()
        {
            return Player.IsDoingPtj();
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active.
        /// </summary>
        [Obsolete]
        public bool DoingPtjForNpc()
        {
            return Player.IsDoingPtjFor(NPC);
        }

        /// <summary>
        ///     Returns true if a PTJ quest is active for a different NPC.
        /// </summary>
        [Obsolete]
        public bool DoingPtjForOtherNpc()
        {
            return Player.IsDoingPtjNotFor(NPC);
        }

        /// <summary>
        ///     Returns true if the player can do a PTJ of type, because he hasn't
        ///     done one of the same type today.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete]
        public bool CanDoPtj(PtjType type, int remaining = 99)
        {
            return Player.CanDoPtj(type, remaining);
        }

        /// <summary>
        ///     Returns the player's level (basic, int, adv) for the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete]
        public QuestLevel GetPtjQuestLevel(PtjType type)
        {
            return Player.GetPtjQuestLevel(type);
        }

        /// <summary>
        ///     Of the given <paramref name="questIds" />, returns those matched to
        ///     the player's success rate for the given PTJ <paramref name="type" />.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="questIds"></param>
        /// <returns></returns>
        [Obsolete]
        public IEnumerable<int> GetLevelMatchingQuestIds(PtjType type, params int[] questIds)
        {
            throw new NotSupportedException("Not available anymore, use GetLevelMatchingQuestIds in QuestScript.");
        }

        /// <summary>
        ///     Returns a random quest id from the given ones, based on the current
        ///     Erinn day and the player's success rate for this PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="questIds"></param>
        /// <returns></returns>
        [Obsolete]
        public int RandomPtj(PtjType type, params int[] questIds)
        {
            throw new NotSupportedException("Not available anymore, use GetRandomPtj in QuestScript.");
        }

        /// <summary>
        ///     Returns number of times the player has done the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete]
        public int GetPtjDoneCount(PtjType type)
        {
            return Player.GetPtjDoneCount(type);
        }

        /// <summary>
        ///     Returns number of times the player has successfully done the given PTJ type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete]
        public int GetPtjSuccessCount(PtjType type)
        {
            return Player.GetPtjSuccessCount(type);
        }

        /// <summary>
        ///     Returns how well the current PTJ has been done (so far).
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public QuestResult GetPtjResult()
        {
            return Player.GetPtjResult();
        }

        /// <summary>
        ///     Displays notice.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Obsolete]
        public void Notice(string format, params object[] args)
        {
            Player.Notice(format, args);
        }

        /// <summary>
        ///     Displays notice.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Obsolete]
        public void Notice(NoticeType type, string format, params object[] args)
        {
            Player.Notice(type, format, args);
        }

        /// <summary>
        ///     Displays as notice and system message.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Obsolete]
        public void SystemNotice(string format, params object[] args)
        {
            Player.SystemNotice(format, args);
        }

        /// <summary>
        ///     Displays system message in player's chat log.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        [Obsolete]
        public void SystemMsg(string format, params object[] args)
        {
            Player.SystemMsg(format, args);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        [Obsolete]
        public bool HasKeyword(string keyword)
        {
            return Player.HasKeyword(keyword);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        [Obsolete]
        public void GiveKeyword(string keyword)
        {
            Player.GiveKeyword(keyword);
        }

        /// <summary>
        ///     Returns true if player has the keyword.
        /// </summary>
        /// <param name="keyword"></param>
        [Obsolete]
        public void RemoveKeyword(string keyword)
        {
            Player.RemoveKeyword(keyword);
        }

        // Dialog
        // ------------------------------------------------------------------

        /// <summary>
        ///     Sends one of the passed messages.
        /// </summary>
        /// <param name="msgs"></param>
        public void RndMsg(params string[] msgs)
        {
            var msg = Rnd(msgs);
            if (msg != null)
                Msg(msgs[Random(msgs.Length)]);
        }

        /// <summary>
        ///     Sends one of the passed messages + FavorExpression.
        /// </summary>
        /// <param name="msgs"></param>
        public void RndFavorMsg(params string[] msgs)
        {
            var msg = Rnd(msgs);
            if (msg != null)
                Msg(Hide.None, msgs[Random(msgs.Length)], FavorExpression());
        }

        /// <summary>
        ///     Sends dialog to player's client.
        /// </summary>
        /// <param name="elements"></param>
        public void Msg(params DialogElement[] elements)
        {
            Msg(Hide.None, elements);
        }

        /// <summary>
        ///     Sends dialog to player's client.
        /// </summary>
        /// <param name="hide"></param>
        /// <param name="elements"></param>
        public void Msg(Hide hide, params DialogElement[] elements)
        {
            var element = new DialogElement();

            if (hide == Hide.Face || hide == Hide.Both)
                element.Add(new DialogPortrait(null));
            else if (NPC.DialogPortrait != null)
                element.Add(new DialogPortrait(NPC.DialogPortrait));

            if (hide == Hide.Name || hide == Hide.Both)
                element.Add(new DialogTitle(null));

            element.Add(elements);

            var xml = string.Format(
                "<call convention='thiscall' syncmode='non-sync'>" +
                "<this type='character'>{0}</this>" +
                "<function>" +
                "<prototype>void character::ShowTalkMessage(character, string)</prototype>" +
                "<arguments>" +
                "<argument type='character'>{0}</argument>" +
                "<argument type='string'>{1}</argument>" +
                "</arguments>" +
                "</function>" +
                "</call>",
                Player.EntityId, HtmlEncode(element.ToString()));

            Send.NpcTalk(Player, xml);
        }

        /// <summary>
        ///     Encodes HTML characters in string.
        /// </summary>
        /// <remarks>
        ///     Encodes &, >, <, ", and \.
        /// 
        /// Custom method, as HttpUtility.HtmlEncode encodes UTF8 characters,
        /// but the client doesn't understand the codes.
        /// </remarks>
        /// <param name="html"></param>
        /// <returns></returns>
        private string HtmlEncode(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html;

            var result = new StringBuilder();

            for (var i = 0; i < html.Length; ++i)
            {
                var chr = html[i];
                switch (chr)
                {
                    case '&':
                        result.Append("&amp;");
                        break;

                    case '>':
                        result.Append("&gt;");
                        break;

                    case '<':
                        result.Append("&lt;");
                        break;

                    case '"':
                        result.Append("&quot;");
                        break;

                    case '\'':
                        result.Append("&#39;");
                        break;

                    default:
                        result.Append(chr);
                        break;
                }
            }

            return result.ToString();
        }

        /// <summary>
        ///     Closes dialog box, by sending NpcTalkEndR, and leaves the NPC.
        /// </summary>
        /// <param name="message">Dialog closes immediately if null.</param>
        public void Close(string message = null)
        {
            Close(Hide.Both, message);
        }

        /// <summary>
        ///     Closes dialog box, by sending NpcTalkEndR, and leaves the NPC.
        /// </summary>
        /// <param name="hide"></param>
        /// <param name="message">Dialog closes immediately if null.</param>
        public void Close(Hide hide, string message)
        {
            Close2(hide, message);
            Exit();
        }

        /// <summary>
        ///     Sends NpcTalkEndR but doesn't leave NPC.
        /// </summary>
        /// <param name="message">Dialog closes immediately if null.</param>
        public void Close2(string message = null)
        {
            Close2(Hide.Both, message);
        }

        /// <summary>
        ///     Sends NpcTalkEndR but doesn't leave NPC.
        /// </summary>
        /// <param name="hide"></param>
        /// <param name="message">Dialog closes immediately if null.</param>
        public void Close2(Hide hide, string message)
        {
            if (message != null)
            {
                if (hide == Hide.Face || hide == Hide.Both)
                    message = new DialogPortrait(null) + message;
                else if (NPC.DialogPortrait != null)
                    message = new DialogPortrait(NPC.DialogPortrait) + message;

                if (hide == Hide.Name || hide == Hide.Both)
                    message = new DialogTitle(null) + message;
            }

            Send.NpcTalkEndR(Player, NPC.EntityId, message);
        }

        /// <summary>
        ///     Throws exception to leave NPC.
        /// </summary>
        public void Exit()
        {
            throw new OperationCanceledException("NPC closed by script");
        }

        /// <summary>
        ///     Informs the client that something can be selected now.
        /// </summary>
        public async Task<string> Select()
        {
            var script = string.Format(
                "<call convention='thiscall' syncmode='sync' session='{1}'>" +
                "<this type='character'>{0}</this>" +
                "<function>" +
                "<prototype>string character::SelectInTalk(string)</prototype>" +
                "<arguments><argument type='string'>&#60;keyword&#62;&#60;gift&#62;</argument></arguments>" +
                "</function>" +
                "</call>"
                , Player.EntityId, Player.Client.NpcSession.Id);

            Send.NpcTalk(Player, script);

            ConversationState = ConversationState.Select;
            await _resumeSignal.WaitAsync(_cancellation.Token);
            ConversationState = ConversationState.Ongoing;
            return _response;
        }

        /// <summary>
        ///     Opens keyword window.
        /// </summary>
        /// <remarks>
        ///     Select should be sent afterwards...
        ///     so you can actually select a keyword.
        /// </remarks>
        protected void ShowKeywords()
        {
            var script = string.Format(
                "<call convention='thiscall' syncmode='non-sync'>" +
                "<this type='character'>{0}</this>" +
                "<function>" +
                "<prototype>void character::OpenTravelerMemo(string)</prototype>" +
                "<arguments>" +
                "<argument type='string'>(null)</argument>" +
                "</arguments>" +
                "</function>" +
                "</call>"
                , Player.EntityId);

            Send.NpcTalk(Player, script);
        }

        // Dialog factory
        // ------------------------------------------------------------------

        public DialogElement Elements(params DialogElement[] elements)
        {
            return new DialogElement(elements);
        }

        public DialogButton Button(string text, string keyword = null, string onFrame = null)
        {
            return new DialogButton(text, keyword, onFrame);
        }

        public DialogBgm Bgm(string file)
        {
            return new DialogBgm(file);
        }

        public DialogImage Image(string name)
        {
            return new DialogImage(name, false, 0, 0);
        }

        public DialogImage Image(string name, int width = 0, int height = 0)
        {
            return new DialogImage(name, false, width, height);
        }

        public DialogImage Image(string name, bool localize = false, int width = 0, int height = 0)
        {
            return new DialogImage(name, localize, width, height);
        }

        public DialogList List(string text, int height, string cancelKeyword, params DialogButton[] elements)
        {
            return new DialogList(text, height, cancelKeyword, elements);
        }

        public DialogList List(string text, params DialogButton[] elements)
        {
            return List(text, elements.Length, elements);
        }

        public DialogList List(string text, int height, params DialogButton[] elements)
        {
            return List(text, height, "@end", elements);
        }

        public DialogInput Input(string title = "Input", string text = "", byte maxLength = 20, bool cancelable = true)
        {
            return new DialogInput(title, text, maxLength, cancelable);
        }

        public DialogAutoContinue AutoContinue(int duration)
        {
            return new DialogAutoContinue(duration);
        }

        public DialogFaceExpression Expression(string expression)
        {
            return new DialogFaceExpression(expression);
        }

        public DialogFaceExpression FavorExpression()
        {
            var favor = Favor;

            if (favor > 40)
                return Expression("love");
            if (favor > 15)
                return Expression("good");
            if (favor > -15)
                return Expression("normal");
            if (favor > -40)
                return Expression("bad");

            return Expression("hate");
        }

        public DialogMovie Movie(string file, int width, int height, bool loop = true)
        {
            return new DialogMovie(file, width, height, loop);
        }

        public DialogText Text(string format, params object[] args)
        {
            return new DialogText(format, args);
        }

        public DialogHotkey Hotkey(string text)
        {
            return new DialogHotkey(text);
        }

        public DialogMinimap Minimap(bool zoom, bool maxSize, bool center)
        {
            return new DialogMinimap(zoom, maxSize, center);
        }

        public DialogShowPosition ShowPosition(int region, int x, int y, int remainingTime)
        {
            return new DialogShowPosition(region, x, y, remainingTime);
        }

        public DialogShowDirection ShowDirection(int x, int y, int angle)
        {
            return new DialogShowDirection(x, y, angle);
        }

        public DialogSetDefaultName SetDefaultName(string name)
        {
            return new DialogSetDefaultName(name);
        }

        public DialogSelectItem SelectItem(string title, string caption, string tags)
        {
            return new DialogSelectItem(title, caption, tags);
        }

        public DialogPtjDesc PtjDesc(int questId, string name, string title, int maxAvailableJobs, int remainingJobs,
            int history)
        {
            return new DialogPtjDesc(questId, name, title, maxAvailableJobs, remainingJobs, history);
        }

        public DialogPtjReport PtjReport(QuestResult result)
        {
            return new DialogPtjReport(result);
        }

        // ------------------------------------------------------------------

        protected enum ItemState : byte
        {
            Up = 0,
            Down = 1
        }

        protected enum GiftReaction
        {
            Dislike,
            Neutral,
            Like,
            Love
        }
    }

    public enum Hide
    {
        None,
        Face,
        Name,
        Both
    }

    public enum ConversationState
    {
        Ongoing,
        Select,
        Ended
    }

    public enum HookResult
    {
        /// <summary>
        ///     Continues to next hook.
        /// </summary>
        Continue,

        /// <summary>
        ///     Breaks hook loop and returns to script.
        /// </summary>
        Break,

        /// <summary>
        ///     Breaks hook loop and ends script
        /// </summary>
        End
    }

    public enum NpcMood
    {
        VeryStressed,
        Stressed,
        BestFriends,
        Friends,
        Hates,
        ReallyDislikes,
        Dislikes,
        Neutral,
        Likes,
        ReallyLikes,
        Love
    }

    public struct RepairResult
    {
        public bool HadGold;
        public long ItemEntityId;
        public Item Item;
        public int Points;
        public int Successes;
        public int Fails;
        public bool Used100Rate;
    }

    /// <summary>
    ///     Information about upgrade process
    /// </summary>
    /// <remarks>
    ///     Does not require HadGold or something, the client disables
    ///     upgrades you can't use because of insufficient gold or prof.
    /// </remarks>
    public struct UpgradeResult
    {
        public long ItemEntityId;
        public Item Item;
        public ItemUpgradeData Upgrade;
        public bool Success;
    }

    /// <summary>
    ///     Upgrade information that are combined in one meta data string called "WU".
    /// </summary>
    public class WUUpgrades
    {
        private byte _ccLevel;

        /// <summary>
        ///     Creates new, nulled instance.
        /// </summary>
        public WUUpgrades()
        {
        }

        /// <summary>
        ///     Creates new instance, parsing the given value.
        /// </summary>
        /// <param name="val"></param>
        /// <example>
        ///     var wu = new WUUpgrades("12345603010203");
        /// </example>
        public WUUpgrades(string val)
        {
            // Null or empty is fine, just ignore
            if (string.IsNullOrWhiteSpace(val))
                return;

            // Check length
            if (val.Length > 14)
                throw new ArgumentException("Value must have <= 14 characters");

            // Fix length for old values, from before ToString was fixed.
            if (val.Length < 14)
                val = val.PadLeft(14, '0');

            ParseValue(val);
        }

        /// <summary>
        ///     Chain Cast upgrade, affected skill id
        /// </summary>
        public ushort ChainCastSkillId { get; set; }

        /// <summary>
        ///     Chain Cast upgrade, chain level
        /// </summary>
        public byte ChainCastLevel
        {
            get => _ccLevel;
            set
            {
                if (value > 9)
                    throw new ArgumentOutOfRangeException("Value too big");

                _ccLevel = value;
            }
        }

        /// <summary>
        ///     Mana Consumption upgrade
        /// </summary>
        public sbyte ManaUse { get; set; }

        /// <summary>
        ///     Evaporated Mana upgrade
        /// </summary>
        public sbyte ManaBurn { get; set; }

        /// <summary>
        ///     Charging Speed upgrade
        /// </summary>
        public sbyte CastingSpeed { get; set; }

        /// <summary>
        ///     Magic Attack upgrade
        /// </summary>
        public sbyte MagicDamage { get; set; }

        private void ParseValue(string val)
        {
            ChainCastSkillId = Convert.ToUInt16(val.Substring(0, 5));
            ChainCastLevel = Convert.ToByte(val.Substring(5, 1), 16);
            ManaUse = Convert.ToSByte(val.Substring(6, 2), 16);
            ManaBurn = Convert.ToSByte(val.Substring(8, 2), 16);
            CastingSpeed = Convert.ToSByte(val.Substring(10, 2), 16);
            MagicDamage = Convert.ToSByte(val.Substring(12, 2), 16);
        }

        public override string ToString()
        {
            var result = new StringBuilder();

            result.Append(ChainCastSkillId.ToString("00000"));
            result.Append(ChainCastLevel.ToString().Substring(0, 1));
            result.Append(ManaUse.ToString("x2"));
            result.Append(ManaBurn.ToString("x2"));
            result.Append(CastingSpeed.ToString("x2"));
            result.Append(MagicDamage.ToString("x2"));

            return result.ToString();
        }
    }

#if __MonoCS__ // Added in Mono 3.0.8, adding it here for convenience.
	public static class SemaphoreSlimExtension
	{
		public static Task WaitAsync(this SemaphoreSlim slim, CancellationToken cancellationToken)
		{
			return Task.Factory.StartNew(() => slim.Wait(cancellationToken), cancellationToken);
		}
	}
#endif
}