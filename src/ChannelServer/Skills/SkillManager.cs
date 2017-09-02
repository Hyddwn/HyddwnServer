// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aura.Channel.Skills.Base;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Skills
{
    public class SkillManager
    {
        private readonly Dictionary<SkillId, ISkillHandler> _handlers;

        public SkillManager()
        {
            _handlers = new Dictionary<SkillId, ISkillHandler>();
        }

        /// <summary>
        ///     Loads all classes with skill attributes as handlers.
        /// </summary>
        public void AutoLoad()
        {
            Log.Info("Loading skill handlers...");

            lock (_handlers)
            {
                _handlers.Clear();

                // Search through all loaded types to find skill attributes
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    var attributes = type.GetCustomAttributes(typeof(SkillAttribute), false);
                    if (attributes == null || attributes.Length == 0)
                        continue;

                    var handler = Activator.CreateInstance(type) as ISkillHandler;
                    foreach (var skillId in (attributes.First() as SkillAttribute).Ids)
                        AddHandler(skillId, handler);
                }
            }

            Log.Info("  done loading {0} skill handlers.", _handlers.Count);
        }

        /// <summary>
        ///     Adds handler for skill id and calls Init if applicable.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="handler"></param>
        public void AddHandler(SkillId skillId, ISkillHandler handler)
        {
            if (_handlers.ContainsKey(skillId))
                Log.Info("SkillManager: Overriding handler for '{0}', using '{1}'.", skillId, handler.GetType().Name);

            var initHandler = handler as IInitiableSkillHandler;
            if (initHandler != null) initHandler.Init();

            _handlers[skillId] = handler;
        }

        /// <summary>
        ///     Returns skill handler for id, or null.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public ISkillHandler GetHandler(SkillId skillId)
        {
            ISkillHandler result;
            lock (_handlers)
            {
                _handlers.TryGetValue(skillId, out result);
            }
            return result;
        }

        /// <summary>
        ///     Returns skill handler for id, or null.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        public T GetHandler<T>(SkillId skillId) where T : class, ISkillHandler
        {
            ISkillHandler result;
            lock (_handlers)
            {
                _handlers.TryGetValue(skillId, out result);
            }
            return result as T;
        }
    }

    public class SkillAttribute : Attribute
    {
        public SkillAttribute(params SkillId[] skillIds)
        {
            Ids = skillIds;
        }

        public SkillId[] Ids { get; protected set; }
    }
}