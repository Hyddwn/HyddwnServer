// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Msgr.Database
{
	public class Note
	{

		public long Id { get; set; }
		public string Sender { get; set; }
		public string Receiver { get; set; }
		public string Message { get; set; }
		public DateTime Time { get; set; }
		public bool Read { get; set; }

		public string FromCharacterName { get; set; }
		public string FromServer { get; set; }

		/// <summary>
		/// Returns time in a special long format the client expects for notes.
		/// </summary>
		/// <remarks>
		/// Because using a timestamp or a string is way too convenient.
		/// </remarks>
		/// <returns></returns>
		public long GetLongTime()
		{
			long result = this.Time.Second;
			result += (this.Time.Minute * 100);
			result += (this.Time.Hour * 10000);
			result += (this.Time.Day * 1000000);
			result += (this.Time.Month * 100000000);
			result += (this.Time.Year * 10000000000);

			return result;
		}
	}
}
