// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Mabi.Const
{
	public enum LoginType
	{
		/// <summary>
		/// Only seen in KR
		/// </summary>
		KR = 0,

		/// <summary>
		/// Used to request disconnect when you're already logged in.
		/// </summary>
		/// <remarks>
		/// Sometimes this is sent, sometimes NormalAgain.
		/// </remarks>
		RequestDisconnect = 1,

		/// <summary>
		/// Coming from channel (session key)
		/// </summary>
		FromChannel = 2,

		/// <summary>
		/// NX auth hash
		/// </summary>
		NewHash = 5,

		/// <summary>
		/// Default, hashed password
		/// </summary>
		Normal = 12,

		/// <summary>
		/// Default, hashed password, used for login while requesting logout.
		/// </summary>
		NormalWithDisconnect = 13,

		/// <summary>
		/// ? o.o
		/// </summary>
		CmdLogin = 16,

		/// <summary>
		/// Last seen in EU (no hashed password)
		/// </summary>
		EU = 18,

		/// <summary>
		/// Password + Secondary password
		/// </summary>
		SecondaryPassword = 20,

		/// <summary>
		/// RSA password, used by CH
		/// </summary>
		CH = 23,
	}

	public enum LoginResult
	{
		Fail = 0,
		Success = 1,
		Empty = 2,
		IdOrPassIncorrect = 3,
		/* IdOrPassIncorrect = 4, */
		TooManyConnections = 6,
		AlreadyLoggedIn = 7,
		UnderAge = 33,
		Message = 51,
		SecondaryReq = 90,
		SecondaryFail = 91,
		Banned = 101,
	}

	public enum LoginResultMessage
	{
		UnkKr1 = 1,
		BannedForTimeSpan = 2,
		SuspendedAccountReactivated = 3,
		BanAppealExplanation = 4,
		UnkKr2 = 5,
		UnkKr3 = 6,
		PleioneLogin51_1 = 7,
		UnexpectedCharacterDataError = 8,
		AccountError = 9,
		LoginUnderMaintenance = 10,
		CannotLoginFromCurLocation = 11,
		UnkKr4 = 12,
		UnkKr5 = 13,
		UnkKr6 = 14,
		UnkKr7 = 15,
		UnkKr8 = 16,
		UnknownError = 17,
		Custom = 18,
		PleioneLogin51_2 = 19,
		UnkKr9 = 20,
		UnkKr10 = 21,
		StartFromWebsite = 22,
		HackingAttemptsDetected = 23,
		Custom2 = 24,
		InvalidArgument = 25, // Caused by data sent?
		UserAgreement = 26,
		SelectMabinogiId = 27,
		UnkKr11 = 28,
		UnkKr12 = 29,
		UnkKr13 = 30,
		UnkKr14 = 31, // Accept something?
		UnkKr15 = 32,
		SelectionLimitExceeded = 33,
		UnkKr16 = 34, // Some kind of selection
		UnkKr17 = 35,
		UnkKr18 = 36,
		UnkKr19 = 37,
		UnkKr20 = 38,
	}

	public enum PetCreationOptionsListType : byte
	{
		BlackList = 0, WhiteList = 1
	}

	public enum DeletionFlag { Normal, Recover, Ready, Delete }
}
