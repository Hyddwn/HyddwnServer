// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Aura.Shared.Util
{
	/// <summary>
	/// Manages localization strings.
	/// </summary>
	/// <remarks>
	/// Uses Gettext's PO files for localization, supporting (some) plurals
	/// and contexts.
	/// </remarks>
	public static class Localization
	{
		private const string ContextConcat = "$__//_$_//__$";

		private static Dictionary<string, Message> _storage = new Dictionary<string, Message>();

		/// <summary>
		/// Loads messages from given PO file, or PO files in the given
		/// directory.
		/// </summary>
		/// <remarks>
		/// If path is a file, it simply reads the file. If path is a directory,
		/// it starts parsing all files recursively.
		/// </remarks>
		/// <param name="path">What to parse</param>
		public static void Load(string path)
		{
			if (File.Exists(path))
			{
				LoadFromFile(path);
			}
			else if (Directory.Exists(path))
			{
				foreach (var file in Directory.EnumerateFiles(path, "*.po", SearchOption.AllDirectories))
					LoadFromFile(file);
			}
			else
			{
				throw new FileNotFoundException(path + " not found.");
			}
		}

		/// <summary>
		/// Loads messages from given file.
		/// </summary>
		/// <param name="path"></param>
		/// <exception cref="FileNotFoundException">If file doesn't exist.</exception>
		/// <exception cref="ArgumentException">If file does not have a .po extension.</exception>
		public static void LoadFromFile(string path)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException(path);

			if (Path.GetExtension(path) != ".po")
				throw new ArgumentException("File is not a PO file.");

			using (var sr = new StreamReader(path, Encoding.UTF8))
				LoadFromStream(sr);
		}

		/// <summary>
		/// Loads messages from given text reader.
		/// </summary>
		/// <param name="reader"></param>
		public static void LoadFromStream(TextReader reader)
		{
			var regex = new Regex(@"""(.*)""", RegexOptions.Compiled);
			var message = new Message();
			var type = BlockType.None;

			string line = null;
			while ((line = reader.ReadLine()) != null)
			{
				line = line.Trim();

				// Skip comments
				if (line.StartsWith("#"))
					continue;

				// Empty line, initiate new message
				if (string.IsNullOrEmpty(line))
				{
					Add(message);
					message = new Message();
					continue;
				}

				// Decide where to put the data
				if (line.StartsWith("msgctxt"))
					type = BlockType.Context;
				else if (line.StartsWith("msgid_plural"))
					type = BlockType.IdPlural;
				else if (line.StartsWith("msgid"))
					type = BlockType.Id;
				else if (line.StartsWith("msgstr"))
					type = BlockType.Str;
				else if (!line.StartsWith("\""))
					type = BlockType.None;

				// Handle data
				var match = regex.Match(line);
				if (match.Success)
				{
					var val = Unescape(match.Groups[1].Value);
					switch (type)
					{
						case BlockType.Context: message.Context += val; break;
						case BlockType.Id: message.Id += val; break;
						case BlockType.IdPlural: message.IdPlural += val; break;
						case BlockType.Str:
							if (line.StartsWith("msgstr[1]"))
								message.StrPlural += val;
							else
								message.Str += val;
							break;
					}
				}
			}

			// StreamReader skips last empty line, add last message
			Add(message);
		}

		/// <summary>
		/// Unescapes string.
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static string Unescape(string str)
		{
			str = str.Replace("\\t", "\t");
			str = str.Replace("\\r\\n", "\n");
			str = str.Replace("\\n", "\n");

			return str;
		}

		/// <summary>
		/// Adds message to storage.
		/// </summary>
		/// <param name="message"></param>
		private static void Add(Message message)
		{
			if (message.Id == null || (string.IsNullOrEmpty(message.Str) && string.IsNullOrEmpty(message.StrPlural)))
				return;

			_storage[message.Key] = message;
		}

		/// <summary>
		/// Returns localization string, defaults to id if not localized
		/// string exists.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string Get(string id)
		{
			Message message;
			if (!_storage.TryGetValue(id, out message) || string.IsNullOrEmpty(message.Str))
				return id;

			return message.Str;
		}

		/// <summary>
		/// Returns localized string as singular or plural, based on n.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="plural"></param>
		/// <param name="n"></param>
		/// <returns></returns>
		public static string GetPlural(string id, string plural, int n)
		{
			var s = id;
			var p = plural;

			// TODO: International plurals
			// http://localization-guide.readthedocs.org/en/latest/l10n/pluralforms.html
			var isPlural = (n != 1);

			// Get message strings
			Message message;
			if (_storage.TryGetValue(id, out message))
			{
				if (!string.IsNullOrEmpty(message.Str))
					s = message.Str;
				if (!string.IsNullOrEmpty(message.StrPlural))
					p = message.StrPlural;
			}

			return (isPlural ? p : s);
		}

		/// <summary>
		/// Returns localized string for id in context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string GetParticular(string context, string id)
		{
			var fullId = context + ContextConcat + id;

			Message message;
			if (!_storage.TryGetValue(fullId, out message) || string.IsNullOrEmpty(message.Str))
				return id;

			return message.Str;
		}

		/// <summary>
		/// Returns localized string for id in context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		public static string GetParticularPlural(string context, string id, string plural, int number)
		{
			var fullId = context + ContextConcat + id;
			var s = id;
			var p = plural;

			Message message;
			if (_storage.TryGetValue(fullId, out message))
			{
				if (!string.IsNullOrEmpty(message.Str))
					s = message.Str;
				if (!string.IsNullOrEmpty(message.StrPlural))
					p = message.StrPlural;
			}

			// TODO: International plurals
			return (Math.Abs(number) != 1 ? p : s);
		}

		/// <summary>
		/// Represents a localization storage entry.
		/// </summary>
		private class Message
		{
			public string Context { get; set; }
			public string Id { get; set; }
			public string IdPlural { get; set; }
			public string Str { get; set; }
			public string StrPlural { get; set; }

			public string Key
			{
				get
				{
					if (string.IsNullOrEmpty(this.Context))
						return this.Id;
					return this.Context + ContextConcat + this.Id;
				}
			}
		}

		/// <summary>
		/// Block type to read.
		/// </summary>
		private enum BlockType
		{
			None,
			Context,
			Id,
			IdPlural,
			Str,
		}
	}
}
