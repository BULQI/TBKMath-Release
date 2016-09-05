using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace TBKMath
{
    public class Table
    {
        public Table() { }

        public Table(string FileName)
        {
            if (Path.GetExtension(FileName) == ".csv")
            {
                Read(FileName, ',');
            }
            else
            {
                Read(FileName);
            }
        }

        public Table(Stream stream)
        {
            Read(stream);
        }

        private static string[] GetStringBuffer(byte[] byteBuffer)
        {
            return System.Text.ASCIIEncoding.ASCII.GetString(byteBuffer).Split('\n');
        }

        public void Read(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            List<string> temp = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                temp.Add(line);
                line = reader.ReadLine();
            }
            reader.Close();

            nRows = temp.Count;
            if (nRows == 0)
            {
                nColumns = 0;
                data = new string[nRows, nColumns];
                return;
            }
            nColumns = Regex.Matches(temp[0], "\t").Count + 1;
            string[] buffer = new string[nColumns];
            data = new string[nRows, nColumns];
            for (int i = 0; i < nRows; i++)
            {
                buffer = temp[i].Split('\t');
                if (!IsTrailingBlankRow(buffer))
                {
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        data[i, j] = buffer[j];
                    }
                }
                else
                {
                    nRows--;
                }
            }
        }

        public static Table FromString(string tableString, char delimiterAsChar ='\t')
        {
            Table table = new Table();
            StringReader reader = new StringReader(tableString);
            string delimiter = delimiterAsChar.ToString();
            List<string> temp = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                temp.Add(line);
                line = reader.ReadLine();
            }
            reader.Close();

            table.nRows = temp.Count;
            if (table.nRows == 0)
            {
                table.nColumns = 0;
                table.data = new string[table.nRows, table.nColumns];
                return table;
            }
            table.nColumns = Regex.Matches(temp[0], delimiter).Count + 1;
            string[] buffer = new string[table.nColumns];
            table.data = new string[table.nRows, table.nColumns];
            for (int i = 0; i < table.nRows; i++)
            {
                buffer = temp[i].Split(delimiterAsChar);
                // check to see if it is blank
                if (!IsTrailingBlankRow(buffer))
                {
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        table.data[i, j] = buffer[j];
                    }
                }
                else
                {
                    table.nRows--;
                }
            }

            return table;
        }

        public void Read(string FileName, char delimiterAsChar='\t')
        {
            string delimiter = delimiterAsChar.ToString();
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
                temp.Add(line);
                line = reader.ReadLine();
            }
            reader.Close();

            nRows = temp.Count;
            if (nRows == 0)
            {
                nColumns = 0;
                data = new string[nRows, nColumns];
                return;
            }
            nColumns = Regex.Matches(temp[0], delimiter).Count + 1;
            string[] buffer = new string[nColumns];
            data = new string[nRows, nColumns];
            for (int i = 0; i < nRows; i++)
            {
                buffer = temp[i].Split(delimiterAsChar);
                // check to see if it is blank
                if (!IsTrailingBlankRow(buffer))
                {
                    for (int j = 0; j < buffer.Length; j++)
                    {
                        data[i, j] = buffer[j];
                    }
                }
                else
                {
                    nRows--;
                }
            }
        }

        private static bool IsTrailingBlankRow(string[] buffer)
        {
            if (buffer[0].Length > 0)
                return false;

            for (int iCol = 1; iCol < buffer.Length; iCol++)
            {
                if (buffer[iCol] != null)
                    return false;
            }
            return true;
        }

        private string[,] data;
        public string[,] Data
        {
            get { return data; }
        }

        private int nRows;
        public int NRows
        {
            get { return nRows; }
        }

        private int nColumns;
        public int NColumns
        {
            get { return nColumns; }
        }

        public string[] Row(int i)
        {
            if (data == null)
                return null;

            string[] row = new string[nColumns];
            for (int j = 0; j < nColumns; j++)
            {
                row[j] = data[i, j];
            }
            return row;
        }

        public string[] Column(int i)
        {
            if (data == null)
                return null;

            string[] column = new string[nRows];
            for (int j = 0; j < nRows; j++)
            {
                column[j] = data[j, i];
            }
            return column;
        }
    }
}
