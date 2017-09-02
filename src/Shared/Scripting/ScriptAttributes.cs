// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Shared.Scripting
{
    /// <summary>
    ///     Defines a type that's not to load.
    /// </summary>
    /// <remarks>
    ///     Use when overriding an existing NPC, to only load one version of it.
    /// </remarks>
    public class OverrideAttribute : Attribute
    {
        public OverrideAttribute(string typeName)
        {
            TypeName = typeName;
        }

        public string TypeName { get; }
    }

    /// <summary>
    ///     Defines types to remove from loading list.
    /// </summary>
    /// <remarks>
    ///     List types that are to be removed from the loading list.
    ///     Similar to Override in functionality.
    /// </remarks>
    public class RemoveAttribute : Attribute
    {
        public RemoveAttribute(params string[] typeNames)
        {
            TypeNames = typeNames;
        }

        public string[] TypeNames { get; }
    }

    /// <summary>
    ///     Makes script only load if feature is enabled.
    /// </summary>
    public class IfEnabledAttribute : Attribute
    {
        public IfEnabledAttribute(string feature)
        {
            Feature = feature;
        }

        public string Feature { get; protected set; }
    }

    /// <summary>
    ///     Makes script only load if feature is not enabled.
    /// </summary>
    /// <remarks>
    ///     Don't inherit from IfEnabledAttribute, by default reflection
    ///     picks up on the base type and never checks this one.
    /// </remarks>
    public class IfNotEnabledAttribute : Attribute
    {
        public IfNotEnabledAttribute(string feature)
        {
            Feature = feature;
        }

        public string Feature { get; protected set; }
    }
}