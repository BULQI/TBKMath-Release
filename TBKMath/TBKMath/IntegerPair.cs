using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    /// <summary>
    /// Container for an unordered pair of integers.
    /// </summary>
    public class IntegerPair
    {
        public int I1;
        public int I2;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (GetType() != obj.GetType())
                return false;

            return (this == (IntegerPair)obj);
        }

        public static bool operator ==(IntegerPair ip1, IntegerPair ip2)
        {
            return ( ( (ip1.I1 == ip2.I1) && (ip1.I2 == ip2.I2) )  | ( ( ip1.I2 == ip2.I1 ) && (ip1.I2 == ip2.I1) ) ) ;
        }

        public static bool operator !=(IntegerPair ip1, IntegerPair ip2)
        {

            return ( ( ip1.I1 != ip2.I1 | ip1.I2 != ip2.I2) && (ip1.I1 != ip2.I2 | ip1.I2 != ip2.I1) );
        }

        public override int GetHashCode()
        {
            return I1.GetHashCode() ^ I2.GetHashCode();
        }

        public bool HasMember(int i)
        {
            return (i == I1 | i == I2);
        }

    }
}
