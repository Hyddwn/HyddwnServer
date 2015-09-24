// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;

namespace Aura.Msgr.Database
{
	public abstract class Contact
	{
		public int Id { get; set; }
		public string AccountId { get; set; }
		public string Name { get; set; }
		public string Server { get; set; }
		public ContactStatus Status { get; set; }
		public string Nickname { get; set; }

		public string FullName { get { return (this.Name + "@" + this.Server); } }

		public Contact()
		{
			this.Nickname = "";
			this.Status = ContactStatus.Offline;
		}
	}
}
