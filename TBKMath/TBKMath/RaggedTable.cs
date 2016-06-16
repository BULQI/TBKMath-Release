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

        public RaggedTable(string FileName)
        {
            this.Read(FileName);
        }

        private static string[] GetStringBuffer(byte[] byteBuffer)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(byteBuffer).Split('\n');
        }

        public void Read(byte[] buffer)
        {

        }

        public void Read(string FileName)
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

            List<string> temp = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                temp.Add(line.TrimEnd());
                line = reader.ReadLine();
            }
            reader.Close();

            this.nRows = temp.Count;
            this.data = new List<string>[nRows];
            for (int i = 0; i < nRows; i++)
            {
                string[] buffer =  temp[i].Split('\t');
                data[i] = new List<string>();
                for (int j = 0; j < buffer.Length; j++)
                {
                    data[i].Add(buffer[j]);
                }
            }
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
