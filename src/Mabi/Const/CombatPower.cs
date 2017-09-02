// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
    /// <summary>
    ///     Power of one creature compared to another.
    /// </summary>
    /// <remarks>
    ///     < 0.8x Weakest
    ///         0.8 x - 1.0 x Weak
    ///         1.0 x - 1.4 x Normal
    ///         1.4 x - 2.0 x Strong
    ///         2.0 x - 3.0 x Awful>
    ///         3.0x  Boss
    /// </remarks>
    public enum PowerRating
    {
        Weakest,
        Weak,
        Normal,
        Strong,
        Awful,
        Boss
    }
}