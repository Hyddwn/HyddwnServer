// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.World.Entities.Props
{
	public class PropExtensionManager
	{
		private Prop _prop;
		private List<PropExtension> _extensions;

		/// <summary>
		/// Returns true if this prop has extensions.
		/// </summary>
		public bool HasAny { get { lock (_extensions) return _extensions.Count != 0; } }

		/// <summary>
		/// Creates new extension manager.
		/// </summary>
		/// <param name="prop"></param>
		public PropExtensionManager(Prop prop)
		{
			_extensions = new List<PropExtension>();
			_prop = prop;
		}

		/// <summary>
		/// Adds extension and *doesn't* update clients.
		/// </summary>
		/// <param name="extension"></param>
		public void AddSilent(PropExtension extension)
		{
			lock (_extensions)
				_extensions.Add(extension);
		}

		/// <summary>
		/// Adds extension and updates nearby clients.
		/// </summary>
		/// <param name="extension"></param>
		public void Add(PropExtension extension)
		{
			this.AddSilent(extension);
			Send.AddPropExtension(_prop, extension);
		}

		/// <summary>
		///	Removes all extensions from prop and updates nearby clients.
		/// </summary>
		public void Clear()
		{
			lock (_extensions)
			{
				foreach (var ext in _extensions)
					Send.RemovePropExtension(_prop, ext);

				_extensions.Clear();
			}
		}

		/// <summary>
		/// Returns list of all extensions.
		/// </summary>
		/// <returns></returns>
		public IList<PropExtension> GetList()
		{
			lock (_extensions)
				return _extensions.ToList();
		}

		/// <summary>
		/// Returns true if any extension meets the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public bool Any(Func<PropExtension, bool> predicate)
		{
			lock (_extensions)
				return _extensions.Any(predicate);
		}
	}
}
