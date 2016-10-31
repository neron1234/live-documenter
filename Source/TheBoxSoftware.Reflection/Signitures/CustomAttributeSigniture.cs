﻿
namespace TheBoxSoftware.Reflection.Signitures
{
    using System;
    using Core;

    /// <summary>
    /// The signiture for a custom attribute as described in section 23.3 of ECMA 335.
    /// </summary>
    internal sealed class CustomAttributeSigniture : Signiture
    {
        /// <summary>
        /// Initialises a new instance of the CustomAttributeSigniture class.
        /// </summary>
        /// <param name="signiture">The byte contents of the signiture.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a value for the prolog differs from 0x0001. This indicates
        /// the incorrect signiture type is being read or the signiture contents
        /// are invalid.
        /// </exception>
        public CustomAttributeSigniture(byte[] signiture) : base(Signitures.CustomAttribute)
        {
            Offset offset = 0;

            // Prolog (0x00001) always and only one instance
            PrologSignitureToken prolog = new PrologSignitureToken(signiture, offset);
            Tokens.Add(prolog);
            
            // TODO: Incomplete
            //  Fixed arguments
            //  Num named arguments
            //  Named arguments
        }
    }
}