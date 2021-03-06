﻿
namespace TheBoxSoftware.Reflection.Signatures
{
    using System.Diagnostics;
    using Core;

    [DebuggerDisplay("Count: {Count}")]
    internal sealed class CountSignatureToken : SignatureToken
    {
        private ushort _count;

        /// <summary>
        /// Initialises a new Count token from the provided <paramref name="signiture"/> at 
        /// <paramref name="offset"/>.
        /// </summary>
        /// <param name="signiture">The signiture blob.</param>
        /// <param name="offset">The offset in the signiture.</param>
		public CountSignatureToken(byte[] signiture, Offset offset)
            : base(SignatureTokens.Count)
        {
            _count = FieldReader.ToUInt16(signiture, offset.Shift(2));
        }

        public override string ToString()
        {
            return $"[Count: {_count}] ";
        }

		public ushort Count
        {
            get { return _count; }
            set { _count = value; }
        }
    }
}