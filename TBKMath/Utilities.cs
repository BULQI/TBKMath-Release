using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TBKMath
{
    /// <summary>
    /// Provides a set of methods for performing various useful tasks.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Writes a two-dimensional matrix to storage in a natural human-readable format.
        /// </summary>
        /// <param name="FileName">A legitimate filename to which the matrix will be written.</param>
        /// <param name="matrix">The two-dimensional matrix to be written.</param>
        public static void WriteMatrixToDisk(string FileName, double[,] matrix)
        {
            using (StreamWriter writer = File.CreateText(FileName))
            {
                for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                {
                    writer.Write("\t" + iCol);
                }
                writer.WriteLine();

                for (int iRow = 0; iRow < matrix.GetLength(0); iRow++)
                {
                    writer.Write(iRow);
                    for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                    {
                        writer.Write("\t" + matrix[iRow, iCol]);
                    }
                    writer.WriteLine();
                }
            }
        }

        /// <summary>
        /// Writes a three-dimensional matrix to storage in a natural human-readable format.
        /// </summary>
        /// <param name="FileName">A legitimate filename to which the matrix will be written.</param>
        /// <param name="matrix">The three-dimensional matrix to be written.</param>
        public static void WriteMatrixToDisk(string FileName, double[, ,] matrix)
        {
            using (StreamWriter writer = File.CreateText(FileName))
            {
                for (int iLayer = 0; iLayer < matrix.GetLength(2); iLayer++)
                {
                    for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                    {
                        writer.Write("\t" + iCol);
                    }
                    writer.WriteLine();

                    for (int iRow = 0; iRow < matrix.GetLength(0); iRow++)
                    {
                        writer.Write(iRow);
                        for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                        {
                            writer.Write("\t" + matrix[iRow, iCol, iLayer]);
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Writes a three-dimensional matrix to storage in a natural human-readable format.
        /// </summary>
        /// <param name="FileName">A legitimate filename to which the matrix will be written.</param>
        /// <param name="matrix">The three-dimensional matrix to be written.</param> 
        public static void WriteMatrixToDisk(string FileName, int[, ,] matrix)
        {
            using (StreamWriter writer = File.CreateText(FileName))
            {
                for (int iLayer = 0; iLayer < matrix.GetLength(2); iLayer++)
                {
                    for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                    {
                        writer.Write("\t" + iCol);
                    }
                    writer.WriteLine();

                    for (int iRow = 0; iRow < matrix.GetLength(0); iRow++)
                    {
                        writer.Write(iRow);
                        for (int iCol = 0; iCol < matrix.GetLength(1); iCol++)
                        {
                            writer.Write("\t" + matrix[iRow, iCol, iLayer]);
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        public static bool AreIdentical(char[] seq1, char[] seq2)
        {
            if (seq1.Length != seq2.Length)
                return false;

            for (int i = 0; i < seq1.Length; i++)
            {
                if (seq1[i] != seq2[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Computes the Hamming distance between two strings.
        /// </summary>
        /// <param name="s1">The first of two strings to compare.</param>
        /// <param name="s2">The second of two strings to compare.</param>
        /// <returns>The Hamming distance, defined as the number of positions at which the two strings differ.</returns>
        public static uint HammingDistance(string s1, string s2)
        {
            uint dist = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i]) dist++;
            }
            return dist;
        }

        public static uint HammingDistance(char[] s1, char[] s2)
        {
            uint dist = 0;
            for (int i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i]) dist++;
            }
            return dist;
        }

        public static string BuildNumber()
        {
            string returnVal = string.Empty;
            return returnVal;
        }

        /// <summary>
        /// Converts any string to a string that can be used as a filename by removing all disallowed characters.
        /// </summary>
        /// <param name="s">The input string.</param>
        /// <returns>The output string with all disallowed characters removed.</returns>
        public static string SafeStringForFilename(string s)
        {
            // simply removes disallowed characters
            string safe = s.Trim();

            foreach (char lDisallowed in System.IO.Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(lDisallowed.ToString(), "");
            }
            foreach (char lDisallowed in System.IO.Path.GetInvalidPathChars())
            {
                safe = safe.Replace(lDisallowed.ToString(), "");
            }
            // ensure that the name is not too long. Max length for the fully 
            // qualifiedi path name is 260 characters, but
            // cut to 120 to allow for directory names of some length
            return safe.Substring(0, Math.Min(120, safe.Length));
        }

        /// <summary>
        /// Removes all non-printing characters.
        /// </summary>
        /// <param name="s">Input string.</param>
        /// <returns>String with all non-printing characters removed.</returns>
        public static string RemoveWhiteSpace(string s)
        {
            StringBuilder stripped = new StringBuilder();

            foreach (char c in s)
            {
                if (c > 32) stripped.Append(c);
            }
            return stripped.ToString();
        }

        public static double[][] DeepCopy(double[][] array)
        {
            if (array == null)
                return null;

            double[][] copy = new double[array.Length][];
            for (int i = 0; i < array.Length; i++)
            {
                copy[i] = (double[])array[i].Clone();
            }
            return copy;
        }

        public static double LogStar(double x)
        {
            double y = Math.Log(x);
            if (y <= 1)
            {
                // if y < 1, the next term will be negative, so unwind the recursion here
                return y;
            }
            else
            {
                // if y > 1, the next term will be positive, so keep going
                return y + LogStar(y);
            }
        }

        public static void FillHomogeneously<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = value;
            }
        }

        public static void FillCumulatively(this double[] array, double value)
        {
            if (array.Length > -1)
                array[0] = value;
            for (int i = 1; i < array.Length; i++)
            {
                array[i] = array[i - 1] + value;
            }
        }

        public static double[] Cumulate(this double[] vector)
        {
            if (vector == null)
                return null;

            if (vector.Length == 0)
                return new double[0];

            double[] cumulated = new double[vector.Length];
            cumulated[0] = vector[0];
            for (int i = 1; i < cumulated.Length; i++)
            {
                cumulated[i] = cumulated[i - 1] + vector[i];
            }
            return cumulated;
        }

        public static int[] Cumulate(this int[] vector)
        {
            if (vector == null)
                return null;

            if (vector.Length == 0)
                return new int[0];

            int[] cumulated = new int[vector.Length];
            cumulated[0] = vector[0];
            for (int i = 1; i < cumulated.Length; i++)
            {
                cumulated[i] = cumulated[i - 1] + vector[i];
            }
            return cumulated;
        }

        /// <summary>
        /// Makes a deep copy of a dictionary. Code from 
        /// https://stackoverflow.com/questions/28383150/make-a-deep-copy-of-a-dictionary
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="original"></param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>(
         Dictionary<TKey, TValue> original) where TValue : ICloneable
        {
            Dictionary<TKey, TValue> ret = new Dictionary<TKey, TValue>(
                 original.Count, original.Comparer);

            foreach (KeyValuePair<TKey, TValue> entry in original)
            {
                ret.Add(entry.Key, (TValue)entry.Value.Clone());
            }
            return ret;
        }

    }

    public static class Averager
    {
        public static double[] Average(double[] v1, double[] v2, double w1, double w2)
        {
            double[] avg = new double[v1.Length];
            try
            {
                for (int i = 0; i < v1.Length; i++)
                {
                    avg[i] = w1 * v1[i] + w2 * v2[i];
                }
            }
            catch (IndexOutOfRangeException e)
            {
                throw new ArgumentException("Argument vectors are not the same length.", e);
            }
            return avg;
        }

        public static double[][] Average(double[][] v1, double[][] v2, double w1, double w2)
        {
            double[][] avg = new double[v1.Length][];
            if (v1.Length == 0)
                return avg;

            try
            {
                for (int i = 0; i < v1.Length; i++)
                {
                    avg[i] = Average(v1[i], v2[i], w1, w2);
                }
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("Exeption in Averager.Average", e);
            }

            return avg;
        }
    }
}
