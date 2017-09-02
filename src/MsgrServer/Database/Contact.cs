// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Mabi.Const;

namespace Aura.Msgr.Database
{
    public abstract class Contact
    {
        public Contact()
        {
            Nickname = "";
            Status = ContactStatus.Offline;
        }

        public int Id { get; set; }
        public string AccountId { get; set; }
        public long CharacterId { get; set; }
        public string Name { get; set; }
        public string Server { get; set; }
        public ContactStatus Status { get; set; }
        public string Nickname { get; set; }
        public DateTime LastLogin { get; set; }

        public string FullName => Name + "@" + Server;
    }
}