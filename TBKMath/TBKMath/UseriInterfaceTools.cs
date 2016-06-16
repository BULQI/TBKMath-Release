using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace TBKMath
{
    public static class WPFExtensions
    {
        public static int AsInt(this TextBox box, int defaultValue = 0)
        {
            int dummy = 0;
            if (int.TryParse(box.Text, out dummy))
            {
                return dummy;
            }
            else
            {
                box.Text = defaultValue.ToString();
                return defaultValue;
            }
        }

        public static double AsDouble(this TextBox box, double defaultValue = double.NaN)
        {
            double dummy = 0;
            if (double.TryParse(box.Text, out dummy))
            {
                return dummy;
            }
            else
            {
                box.Text = defaultValue.ToString();
                return defaultValue;
            }
        }

        public static long AsLong(this TextBox box, long defaultValue = 0)
        {
            long dummy = 0;
            if (long.TryParse(box.Text, out dummy))
            {
                return dummy;
            }
            else
            {
                box.Text = defaultValue.ToString();
                return defaultValue;
            }
        }

        public static DateTime AsDateTime(this TextBox box, DateTime defaultValue)
        {
            DateTime dummy = DateTime.MinValue;
            if (DateTime.TryParse(box.Text, out dummy))
            {
                return dummy;
            }
            else
            {
                box.Text = defaultValue.ToString();
                return defaultValue;
            }
        }

    }
}
