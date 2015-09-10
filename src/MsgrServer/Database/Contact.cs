// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Msgr.Database
{
	public class Contact
	{
		public int Id { get; set; }
		public string AccountId { get; set; }
		public string Name { get; set; }
		public string Server { get; set; }
		public string ChannelName { get; set; }

		public string FullName { get { return (this.Name + "@" + this.Server); } }
	}
}
