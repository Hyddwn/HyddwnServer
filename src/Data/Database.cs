// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using MsgPack.Serialization;

namespace Aura.Data
{
    public interface IDatabase
    {
        /// <summary>
        ///     Amount of entries.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     List of exceptions caught while loading the database.
        /// </summary>
        List<DatabaseWarningException> Warnings { get; }

        /// <summary>
        ///     Removes all entries.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Loads file if it exists, raises exception otherwise.
        /// </summary>
        /// <param name="path">File to load</param>
        /// <param name="clear">Clear database before loading?</param>
        /// <returns></returns>
        int Load(string path, bool clear);

        /// <summary>
        ///     Loads multiple files, ignores missing ones.
        /// </summary>
        /// <param name="files">Files to load</param>
        /// <param name="cache">Path to an optional cache file (null for none)</param>
        /// <param name="clear">Clear database before loading?</param>
        /// <returns></returns>
        int Load(string[] files, string cache, bool clear);
    }

    public abstract class Database<TList, TInfo> : IDatabase, IEnumerable<TInfo>
        where TInfo : class, new()
        where TList : ICollection, new()
    {
        protected List<DatabaseWarningException> _warnings = new List<DatabaseWarningException>();
        public TList Entries = new TList();

        public List<DatabaseWarningException> Warnings => _warnings;

        public int Count => Entries.Count;

        public abstract void Clear();

        public abstract int Load(string path, bool clear);
        public abstract int Load(string[] files, string cache, bool clear);

        public abstract IEnumerator<TInfo> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public abstract class DatabaseBase<TList, TInfo> : Database<TList, TInfo>
        where TInfo : class, new()
        where TList : ICollection, new()
    {
        public override int Load(string path, bool clear)
        {
            if (clear)
                Clear();

            Warnings.Clear();

            LoadFromFile(path);

            return Entries.Count;
        }

        public override int Load(string[] files, string cache, bool clear)
        {
            if (clear)
                Clear();

            Warnings.Clear();

            var fromFiles = false;
            if (cache == null || !File.Exists(cache))
            {
                fromFiles = true;
            }
            else
            {
                if (files.Any(file => File.GetLastWriteTime(file) > File.GetLastWriteTime(cache)))
                    fromFiles = true;
            }

            if (fromFiles || !LoadFromCache(cache))
            {
                LoadFromFiles(files);
                CreateCache(cache);
            }

            AfterLoad();

            return Entries.Count;
        }

        protected void LoadFromFiles(string[] paths)
        {
            foreach (var path in paths.Where(a => File.Exists(a)))
                LoadFromFile(path);
        }

        protected bool LoadFromCache(string path)
        {
            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    var serializer = MessagePackSerializer.Create<TList>();
                    Entries = serializer.Unpack(stream);
                }
            }
            catch (IOException)
            {
                // One server trying to read while the other one is still
                // creating the cache.
                return false;
            }
            catch (TypeInitializationException)
            {
                // Hotfix for issue #20

                Warnings.Add(new DatabaseWarningException("MsgPack failed to deserialize cache. " +
                                                          "This is usually caused by an incorrect version of the MsgPack library. " +
                                                          "Please download and compile the latest version of MsgPack (https://github.com/msgpack/msgpack-cli), " +
                                                          "then place the generated dll in Aura's Lib folder. Lastly, recompile Aura."));

                return false;
            }
            catch (SerializationException)
            {
                // Deserialization failed, probably due to a new database format or corrupt cache file.
                // Catch it, read from the files, and rebuild the cache.
                return false;
            }

            return true;
        }

        protected bool CreateCache(string path)
        {
            // Only create cache if everything went smoothly.
            if (Entries.Count > 0 && Warnings.Count == 0)
                try
                {
                    using (var stream = new FileStream(path, FileMode.OpenOrCreate))
                    {
                        var serializer = MessagePackSerializer.Create<TList>();
                        serializer.Pack(stream, Entries);
                    }

                    return true;
                }
                catch (IOException)
                {
                    // Multiple servers trying to create the cache, doesn't matter if one fails.
                }

            return false;
        }

        protected abstract void LoadFromFile(string path);

        protected virtual void AfterLoad()
        {
        }
    }
}