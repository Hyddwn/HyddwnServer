// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons.Guilds
{
	public class Guild
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string LeaderName { get; set; }
		public string Title { get; set; }

		public int Points { get; set; }
		public int Gold { get; set; }

		public string IntroMessage { get; set; }
		public string WelcomeMessage { get; set; }
		public string LeavingMessage { get; set; }
		public string RejectionMessage { get; set; }

		public GuildType Type { get; set; }
		public GuildLevel Level { get; set; }
		public GuildOptions Options { get; set; }
		public GuildStone Stone { get; set; }
	}
}
