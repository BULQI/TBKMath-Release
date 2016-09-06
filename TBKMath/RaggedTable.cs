using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TBKMath
{
    public class RaggedTable
    {
        public RaggedTable() { }

        private static string[] GetStringBuffer(byte[] byteBuffer)
        {
            return Encoding.ASCII.GetString(byteBuffer).Split('\n');
        }

        public static RaggedTable FromString(string tableString)
        {
            StringReader reader = new StringReader(tableString);

            List<string> temp = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                temp.Add(line.TrimEnd());
                line = reader.ReadLine();
            }
            reader.Close();

            RaggedTable table = new RaggedTable();
            table.nRows = temp.Count;
            table.data = new List<string>[table.nRows];
            for (int i = 0; i < table.nRows; i++)
            {
                string[] buffer = temp[i].Split('\t');
                table.data[i] = new List<string>();
                for (int j = 0; j < buffer.Length; j++)
                {
                    table.data[i].Add(buffer[j]);
                }
            }
            return table;
        }

        public static RaggedTable ReadFromFile(string FileName)
        {
            FileInfo src = new FileInfo(FileName);
            TextReader reader = null;
            try
            {
                reader = src.OpenText();
            }
            catch
            {
                throw new FileLoadException("The file could not be opened.");
            }
            return FromString(reader.ToString());
        }

        private List<string>[] data;
        public List<string>[] Data
        {
            get { return this.data; }
        }

        private int nRows;
        public int NRows
        {
            get { return this.nRows; }
        }
    }

}
