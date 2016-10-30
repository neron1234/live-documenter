﻿
namespace TheBoxSoftware.Reflection.Signitures
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a container for one or more SignitureToken instances. This is
    /// a constuct that allows groups of tokens that are always parsed together to
    /// be represented and contained via a single class.
    /// </summary>
    internal abstract class SignitureTokenContainer : SignitureToken
    {
        private List<SignitureToken> _childTokens = new List<SignitureToken>();

        /// <summary>
        /// Initialises a new instance of the SignitureTokenContainer class.
        /// </summary>
        /// <param name="tokenType">The type of signiture token represented.</param>
        protected SignitureTokenContainer(SignitureTokens tokenType) : base(tokenType)
        {
        }

        public List<SignitureToken> Tokens
        {
            get { return _childTokens; }
            set { _childTokens = value; }
        }
    }
}