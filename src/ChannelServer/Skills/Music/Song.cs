// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Music
{
	/// <summary>
	/// Song skill handler
	/// </summary>
	/// <remarks>
	/// Prepare starts the singing, complete is sent once it's over.
	/// </remarks>
	[Skill(SkillId.Song)]
	public class Song : PlayingInstrument
	{
		/// <summary>
		/// Minimum random score id.
		/// </summary>
		private const int RandomSongScoreMin = 2001;

		/// <summary>
		/// Maximum random score id.
		/// </summary>
		private const int RandomSongScoreMax = 2052;

		/// <summary>
		/// Subscribes skill to events required for training.
		/// </summary>
		public override void Init()
		{
		}

		/// <summary>
		/// Cancels skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public override void Cancel(Creature creature, Skill skill)
		{
			base.Cancel(creature, skill);

			Send.Effect(creature, 356, (byte)0);
		}

		/// <summary>
		/// Returns instrument type to use.
		/// </summary>
		/// <param name="creature"></param>
		/// 
		/// <returns></returns>
		protected override InstrumentType GetInstrumentType(Creature creature)
		{
			if (creature.IsFemale)
				return InstrumentType.FemaleVoiceJp;
			else
				return InstrumentType.MaleVoiceJp;
		}

		/// <summary>
		/// Returns score field name in the item's meta data.
		/// </summary>
		protected override string MetaDataScoreField { get { return "SCSING"; } }

		/// <summary>
		/// Returns random score id.
		/// </summary>
		/// <param name="rnd"></param>
		/// <returns></returns>
		protected override int GetRandomScore(Random rnd)
		{
			return rnd.Next(RandomSongScoreMin, RandomSongScoreMax + 1);
		}

		/// <summary>
		/// Returns random success message.
		/// </summary>
		/// <remarks>
		/// Reference: http://wiki.mabinogiworld.com/view/Song
		/// </remarks>
		/// <param name="quality"></param>
		/// <returns></returns>
		protected override string GetRandomQualityMessage(PlayingQuality quality)
		{
			string[] msgs = null;
			switch (quality)
			{
				case PlayingQuality.VeryGood:
					msgs = new string[] {
						Localization.Get("Your song was heavenly."),
						Localization.Get("That was a perfect song."),
					};
					break;
				case PlayingQuality.Good:
					msgs = new string[] {
						Localization.Get("You did a fine job."),
						Localization.Get("That really boosted your confidence."),
						Localization.Get("You gave a great performance."),
					};
					break;
				case PlayingQuality.Bad:
				case PlayingQuality.VeryBad:
					msgs = new string[] {
						Localization.Get("This song was too difficult for you to sing."),
						Localization.Get("That was too difficult for you to sing."),
						Localization.Get("This song is still difficult for you to sing"),
						Localization.Get("You need to work harder to sing this song."),
						Localization.Get("That was horrible."),
						Localization.Get("Not only was the song you chose horrible, but you also sang it terribly."),
						Localization.Get("You should apologize for that performance."),
						Localization.Get("That was an easy song and you still butchered it."),
						Localization.Get("Did anyone notice how much you messed up?"),
						Localization.Get("You should feel ashamed of your performance."),
						Localization.Get("You sang it well, but you won't improve if you keep singing such easy songs."),
					};
					break;
			}

			if (msgs == null || msgs.Length < 1)
				return "...";

			return msgs[RandomProvider.Get().Next(0, msgs.Length)].Trim();
		}

		/// <summary>
		/// Called when starting playing (training).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="quality"></param>
		protected override void OnPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			Send.Effect(creature, 356, (byte)1);

			if (skill.Info.Rank == SkillRank.Novice)
				skill.Train(1); // Use the skill.
		}

		/// <summary>
		/// Called when completing (training).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="quality"></param>
		protected override void AfterPlay(Creature creature, Skill skill, PlayingQuality quality)
		{
			// All ranks above F have the same 3 first conditions.
			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R1)
			{
				if (quality >= PlayingQuality.Bad)
					skill.Train(1); // Use the skill successfully.

				if (quality == PlayingQuality.Good)
					skill.Train(2); // Give an excellent vocal performance.

				if (quality == PlayingQuality.VeryGood)
					skill.Train(3); // Give a heavenly performance.

				// Very bad training possible till E.
				if (skill.Info.Rank <= SkillRank.RE && quality == PlayingQuality.VeryBad)
					skill.Train(4); // Fail at using the skill.
			}

			// TODO: "Use the skill to grow crops faster."
			// TODO: "Grant a buff to a party member."
		}
	}
}
