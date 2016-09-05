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

    public class StringTable
    {
        private string[,] data;

        private bool hasHeader;
        public bool HasHeader
        {
            get { return hasHeader; }
        }

        private string[] columnNames;
        public string[] ColumnNames
        {
            get
            {
                if (hasHeader)
                    return columnNames;

                else return null;
            }
        }

        private Dictionary<string, int> columnIndex;

        public StringTable(int nRows, int nColumns, bool hasHeader = false)
        {
            this.nRows = nRows;
            this.nColumns = nColumns;
            this.hasHeader = hasHeader;
            if (hasHeader)
            {
                columnNames = new string[nColumns];
                columnIndex = new Dictionary<string, int>();
            }

            data = new string[nRows, nColumns];
        }

        public static StringTable FromString(string tableString, char delimiter = '\t', bool hasHeader = false)
        {
            StringReader reader = new StringReader(tableString);
            string sDelimiter = delimiter.ToString();
            List<string> temp = new List<string>();
            string line = reader.ReadLine();
            while (line != null)
            {
                temp.Add(line);
                line = reader.ReadLine();
            }
            reader.Close();

            if (temp.Count == 0)
            {
                return new StringTable(0, 0, hasHeader);
            }

            int nCols = temp[0].Count(c => { return c == delimiter; }) + 1;
            int nRows = temp.Count - (hasHeader ? 1 : 0);
            StringTable table = new StringTable(nRows, nCols, hasHeader);

            int offset = 0;
            if (hasHeader)
            {
                table.columnNames = temp[0].Split(delimiter);
                offset = 1;
            }

            string[] buffer;
            for (int i = 0; i < table.nRows - offset; i++)
            {
                buffer = temp[i + offset].Split(delimiter);
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

        public static StringTable ReadFromFile(string FileName, char delimiterAsChar = '\t', bool hasHeader = false)
        {
            string tableString;
            try
            {
                tableString = File.ReadAllText(FileName);
            }
            catch (Exception e)
            {
                throw new Exception("Error reading table.", e);
            }

            StringTable table = FromString(tableString, delimiterAsChar, hasHeader);
            return table;
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

        public string this[int iRow, int iCol]
        {
            get 
            {
                if (iRow < 0 | iRow > nRows | iCol < 0 | iCol > nColumns)
                {
                    throw new IndexOutOfRangeException("Index out range.");
                }

                return data[iRow, iCol];
            }
            set
            {
                if (iRow < 0 | iRow > nRows | iCol < 0 | iCol > nColumns)
                {
                    throw new IndexOutOfRangeException("Index out range.");
                }

                data[iRow, iCol] = value;
            }
        }

        public string[] this[string columnName]
        {
            get
            {
                if (!hasHeader)
                    return null;

                if (!columnIndex.ContainsKey(columnName))
                    return null;

                string[] returnValue = new string[nRows];
                for (int iRow = 0; iRow < nRows; iRow++) 
                {
                    returnValue[iRow] = data[iRow, columnIndex[columnName]];
                }
                return returnValue;
            }

            set
            {
                if (value.Length != nRows)
                {
                    throw new Exception("Length of vector does not equal number of rows in table.");
                }

                if (!hasHeader)
                {
                    throw new Exception("This table does not have column names.");
                }

                if (!columnIndex.ContainsKey(columnName))
                { 
                    throw new Exception("That column name is not found in this table.");
                }

                for (int iRow = 0; iRow < nRows; iRow++)
                {
                    data[iRow, columnIndex[columnName]] = value[iRow];
                }
            }
        }

        public string this[string columnName, int iRow]
        {
            get
            {
                if (!hasHeader)
                    return null;

                if (!columnIndex.ContainsKey(columnName))
                    return null;

                if (iRow < 0 | iRow > nRows)
                {
                    throw new IndexOutOfRangeException("Index out of range.");
                }

                return data[iRow, columnIndex[columnName]];
            }

            set
            {
                if (value.Length != nRows)
                {
                    throw new Exception("Length of vector does not equal number of rows in table.");
                }

                if (!hasHeader)
                {
                    throw new Exception("This table does not have column names.");
                }

                if (!columnIndex.ContainsKey(columnName))
                {
                    throw new Exception("That column name is not found in this table.");
                }

                data[iRow, columnIndex[columnName]] = value;
            }
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

        public override string ToString()
        {
            if (NRows == 0 | nColumns == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            int offset = 0;
            if (hasHeader)
            {
                offset = 1;
                sb.Append(columnNames[0]);
                for (int i = 0; i < nColumns; i++)
                {
                    sb.Append("\t" + columnNames[i]);
                }
                sb.AppendLine();
            }
            for (int iRow = offset; iRow < nRows; iRow++)
            {
                sb.Append(data[iRow, 0]);
                for (int iCol = 1; iCol < nColumns; iCol++)
                {
                    sb.Append("\t" + data[iRow, iCol]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }

}
