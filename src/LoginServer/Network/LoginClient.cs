// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Login.Database;
using Aura.Shared.Network;

namespace Aura.Login.Network
{
	public class LoginClient : DefaultClient
	{
		public string Ident { get; set; }
		public Account Account { get; set; }

		public override void CleanUp()
		{
			if (this.Account != null)
				LoginServer.Instance.Database.SetAccountLoggedIn(this.Account.Name, false);
		}
	}
}
