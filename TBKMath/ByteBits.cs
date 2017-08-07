namespace TBKMath
{
    /// <summary>
    /// Allows a byte to be treated as an array of bits.
    /// Adapted from John Sharp's C# VIndex-by-VIndex
    /// </summary>
    public struct ByteBits
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="byteValue">The byte to be converted to a bit array.</param>
        public ByteBits(byte byteValue)
        {
            bits = byteValue;
        }

        /// <summary>
        /// Allows access to the bit array by index.
        /// </summary>
        /// <param name="index">The index of the bit array. Must be between 0 and 7 inclusive.</param>
        /// <returns>The value of the bit at the requested index: 0 or 1.</returns>
        public bool this[byte index]
        {
            get
            {
                return (bits & (1 << index)) != 0;
            }
            set
            {
                if (value)
                {
                    bits |= (byte)(1 << index);
                }
                else
                {
                    bits &= (byte)~(1 << index);
                }
            }
        }

        /// <summary>
        /// Counts the number of ones, or "set" bits.
        /// </summary>
        /// <returns>The number of "set" bits.</returns>
        public int NSet()
        {
            int returnVal = 0;
            for (byte i = 0; i < 8; i++)
            {
                if (this[i]) returnVal++;
            }
            return returnVal;
        }

        private byte bits;
    }
}
