// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Reflection;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    /// <summary>
    ///     Puzzle script used in dungeons.
    /// </summary>
    /// <example>
    ///     Getting a script: ChannelServer.Instance.ScriptManager.PuzzleScripts.Get("entrance_puzzle");
    /// </example>
    public class PuzzleScript : GeneralScript
    {
        /// <summary>
        ///     Name of the puzzle
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Called when the script is initially created.
        /// </summary>
        /// <returns></returns>
        public override bool Init()
        {
            var attr = GetType().GetCustomAttribute<PuzzleScriptAttribute>();
            if (attr == null)
            {
                Log.Error("PuzzleScript.Init: Missing PuzzleScript attribute.");
                return false;
            }

            Name = attr.Name;

            ChannelServer.Instance.ScriptManager.PuzzleScripts.Add(Name, this);

            return true;
        }

        public virtual void OnPrepare(Puzzle puzzle)
        {
        }

        public virtual void OnPuzzleCreate(Puzzle puzzle)
        {
        }

        public virtual void OnPropEvent(Puzzle puzzle, Prop prop)
        {
        }

        public virtual void OnMobAllocated(Puzzle puzzle, MonsterGroup group)
        {
        }

        public virtual void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
        {
        }
    }

    public class PuzzleScriptAttribute : Attribute
    {
        public PuzzleScriptAttribute(string name)
        {
            Name = name.ToLower();
        }

        public string Name { get; }
    }
}