﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TBKMath
{
    public class StringTable
    {
        private string[,] data;

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

        private List<string> columnNames;
        public List<string> ColumnNames
        {
            get
            {
                return columnNames;
            }
        }

        private Dictionary<string, int> columnIndex;

        public StringTable(int nRows, int nColumns)
        {
            this.nRows = nRows;
            this.nColumns = nColumns;
            data = new string[nRows, nColumns];
            columnNames = new List<string>();
            columnIndex = new Dictionary<string, int>();
            for (int i= 0; i< nColumns; i++)
            {
                columnNames.Add("Column" + i);
                columnIndex.Add(columnNames[i], i);
            }
        }

        public bool RenameColumn(string oldName, string newName)
        {
            if (!columnNames.Contains(oldName))
                return false;

            int rememberIndex = columnIndex[oldName];
            columnIndex.Remove(oldName);
            columnIndex.Add(newName, rememberIndex);
            columnNames[rememberIndex] = newName;
            return true;
        }

        public static StringTable FromString(string tableString, char delimiter = '\t')
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
                return new StringTable(0, 0);
            }

            int nCols = temp[0].Count(c => { return c == delimiter; }) + 1;
            int nRows = temp.Count - 1;
            StringTable table = new StringTable(nRows, nCols);

            table.columnNames = temp[0].Split(delimiter).ToList();
            table.columnIndex.Clear();
            for (int iCol = 0; iCol < nCols; iCol++)
            {
                table.columnIndex.Add(table.columnNames[iCol], iCol);
            }

            string[] buffer;
            for (int i = 0; i < table.nRows; i++)
            {
                buffer = temp[i + 1].Split(delimiter);
                // check to see if it is blank
                if (!IsTrailingBlankRow(buffer))
                {
                    int l = Math.Min(buffer.Length, nCols);
                    for (int j = 0; j < l; j++)
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

        public static StringTable ReadFromFile(string FileName, char delimiterAsChar = '\t')
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

            StringTable table = FromString(tableString, delimiterAsChar);
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

        public string[] this[string columnName]
        {
            get
            {
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

                if (!columnIndex.ContainsKey(columnName))
                {
                    throw new Exception("That column name is not found in this table.");
                }

                data[iRow, columnIndex[columnName]] = value;
            }
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

        public bool ContainsColumn(string columnName)
        {
            return columnNames.Contains(columnName);
        }

        public override string ToString()
        {
            if (NRows == 0 | nColumns == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            bool firstColumn = true;
            foreach (KeyValuePair<string, int> index in columnIndex)
            {
                sb.Append((firstColumn ? "" : "\t") + index.Key);
                firstColumn = false;
            }
            sb.AppendLine();
            for (int iRow = 0; iRow < nRows; iRow++)
            {
                firstColumn = true;
                foreach (KeyValuePair<string, int> index in columnIndex)
                {
                    sb.Append((firstColumn ? "" : "\t") + data[iRow, index.Value]);
                    firstColumn = false;
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static StringTable MergeTableByKeyColumns(StringTable t1, StringTable t2, string mergeColumn1, string mergeColumn2)
        {
            // collect unique keys
            if (!t1.ContainsColumn(mergeColumn1) | !t2.ContainsColumn(mergeColumn2))
            {
                throw new Exception("The tables do not contain the specified column keys.");
            }

            int mergeKey1 = t1.columnIndex[mergeColumn1];
            int mergeKey2 = t2.columnIndex[mergeColumn2];

            List<int> keyReference = new List<int>();
            Dictionary<string, int[]> rowKeys = new Dictionary<string, int[]>();
            for (int iRow = 0; iRow < t1.NRows; iRow++)
            {
                if (rowKeys.ContainsKey(t1.data[iRow, mergeKey1]))
                {
                    throw new Exception("Table 1 contains duplicate merge key entries.");
                }

                rowKeys.Add(t1.data[iRow, mergeKey1], new int[] { iRow, iRow, -1 });
                keyReference.Add(1);
            }

            for (int iRow = 0; iRow < t2.NRows; iRow++)
            {
                if (rowKeys.ContainsKey(t2.data[iRow, mergeKey2]))
                {
                    if (rowKeys[t2.data[iRow, mergeKey2]][2] > 0)
                    {
                        throw new Exception("Table 2 contains duplicate merge key entries.");
                    }
                    else
                    {
                        rowKeys[t2.data[iRow, mergeKey2]][2] = iRow;
                    }
                }
                else
                {
                    rowKeys.Add(t2.data[iRow, mergeKey2], new int[] { t1.NRows + iRow, -1, iRow });
                    keyReference.Add(2);
                }
            }

            int nRows = rowKeys.Count;
            int nColumns = t1.nColumns + t2.nColumns - 1;
            StringTable t = new StringTable(nRows, nColumns);
            t.columnIndex.Clear();

            Dictionary<string, int[]> originalColumnNumber = new Dictionary<string, int[]>();

            // put the merge key into column 1
            t.columnIndex.Add(mergeColumn1, 0);
            foreach (KeyValuePair<string, int> kvp in t1.columnIndex)
            {
                if (kvp.Key == mergeColumn1)
                    continue;

                t.columnIndex.Add(kvp.Key, kvp.Value);
                originalColumnNumber.Add(kvp.Key, new int[] { 1, kvp.Value });
            }

            int count = t1.nColumns;
            foreach (KeyValuePair<string, int> kvp in t2.columnIndex)
            {
                if (kvp.Value == mergeKey2)
                    continue;

                string newName = kvp.Key;
                while (t.columnIndex.ContainsKey(newName))
                {
                    newName += ".2";
                }
                t.columnIndex.Add(newName, count);
                originalColumnNumber.Add(newName, new int[] { 2, kvp.Value });
                count++;
            }
            t.columnNames = t.columnIndex.Keys.ToList();

            // fill out merged key column
            for (int iRow = 0; iRow < t.nRows; iRow++)
            {
                switch (keyReference[iRow])
                {
                    case 1:
                        t.data[iRow, 0] = t1.data[iRow, mergeKey1];
                        break;
                    case 2:
                        t.data[iRow, 0] = t2.data[iRow - t1.NRows, mergeKey2];
                        break;
                } 
            }

            foreach (KeyValuePair<string, int> column in t.columnIndex)
            {
                if (column.Key == mergeColumn1)
                    continue;

                foreach (KeyValuePair<string, int[]> kvp in rowKeys)
                {
                    if (originalColumnNumber[column.Key][0] == 1) // this column comes from table 1
                    {
                        if (kvp.Value[1] > -1) // this row is present in table 1
                        {
                            // rowKeys[0] = row index in new table
                            // rowKeys[1] = row index in table 1
                            // rowKeyrs[2] = row index in table 2
                            t.data[kvp.Value[0], column.Value] = t1.data[kvp.Value[1], originalColumnNumber[column.Key][1]];
                        }
                        else
                        {
                            t.data[kvp.Value[0], column.Value] = "NULL";
                        }
                    }
                    else // this column comes from table two
                    {
                        if (kvp.Value[2] > -1) // this row is present in table 2
                        {
                            // rowKeys[0] = row index in new table
                            // rowKeys[1] = row index in table 1
                            // rowKeyrs[2] = row index in table 2
                            t.data[kvp.Value[0], column.Value] = t2.data[kvp.Value[2], originalColumnNumber[column.Key][1]];
                        }
                        else
                        {
                            t.data[kvp.Value[0], column.Value] = "NULL";
                        }
                    }
                }
            }
            return t;
        }
    }
}
