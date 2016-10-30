﻿
/*
 * Signiture defined in: 23.2.13
 * 
 * RANK->NUMSIZES->SIZE<->NUMLOBOUNDS->LOBOUND<->
 */

namespace TheBoxSoftware.Reflection.Signitures
{
    using System.Text;
    using Core;

    /// <summary>
    /// A signiture that describes the shape of an array as defined.
    /// </summary>
	internal class ArrayShapeSignitureToken : SignitureToken
    {
        private int _rank;         // specifies the number of dimensions (1 or more)
        private int[] _sizes;      // size of each dimension
        private int[] _loBounds;   // the lower boundaries of those dimensions

        /// <summary>
        /// Initialises a new instance of the ArrayShapeSignitureToken which reads the array share
        /// signiture from the provided <paramref name="signiture"/> at <paramref name="offset"/>.
        /// </summary>
        /// <param name="signiture">The signiture blob to read this token from.</param>
        /// <param name="offset">The offset where this roken begins.</param>
		public ArrayShapeSignitureToken(byte[] signiture, Offset offset) : base(SignitureTokens.ArrayShape)
        {
            int numSizes = 0;
            int numLoBounds = 0;

            _rank = GetCompressedValue(signiture, offset);

            numSizes = GetCompressedValue(signiture, offset);
            _sizes = new int[numSizes];
            for(int i = 0; i < numSizes; i++)
            {
                _sizes[i] = GetCompressedValue(signiture, offset);
            }

            numLoBounds = GetCompressedValue(signiture, offset);
            _loBounds = new int[numLoBounds];
            for(int i = 0; i < numLoBounds; i++)
            {
                _loBounds[i] = GetCompressedValue(signiture, offset);
            }
        }

        /// <summary>
        /// Produces a string representation e.g. '[1..6, 5, ,]' of the array 
        /// shape token.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[ArraySize: [");

            for(int i = 0; i < _rank; i++)
            {
                if(_loBounds.Length > i)
                {
                    sb.AppendFormat("{0}...", _loBounds[i]);
                }
                if(_sizes.Length > i)
                {
                    sb.AppendFormat("{0}", _loBounds[i]);
                }
                if(i != _rank - 1)
                {
                    sb.Append(",");
                }
            }

            sb.Append("]]");

            return sb.ToString();
        }

        /// <summary>
        /// The number of ranks in the array shape.
        /// </summary>
		public int Rank
        {
            get { return _rank; }
            private set { _rank = value; }
        }

        /// <summary>
        /// The defined sizes
        /// </summary>
		public int[] Sizes
        {
            get { return _sizes; }
            private set { _sizes = value; }
        }

        /// <summary>
        /// The values of those low boundaries.
        /// </summary>
		public int[] LoBounds
        {
            get { return _loBounds; }
            private set { _loBounds = value; }
        }
    }
}