using System;
using System.Collections.Generic;

namespace TBKMath
{
    public class DataTable
    {
        public List<DataRow> Rows;
        public Dictionary<string, DataColumn> Columns;

        private int nRows;
        public int NRows
        {
            get { return nRows; }
        }

        private int nCols;
        public int NCols
        {
            get { return nCols; }
        }

        // Constructor
        public DataTable()
        {
            Rows = new List<DataRow>();
            Columns = new Dictionary<string, DataColumn>();
        }

        private void countCategories()
        {
            foreach (string columnName in Columns.Keys)
            {
                if (Columns[columnName].Type == DataType.Categorical)
                {
                    countCategories(columnName);
                }
            }
        }

        private void countCategories(string columnName)
        {
            if (!Columns.ContainsKey(columnName))
            {
                throw new Exception("Column not found.");
            }

            if (Columns[columnName].Type != DataType.Categorical)
            {
                return;
            }

            Columns[columnName].Categories = new List<string>();
            foreach (DataRow r in Rows)
            {
                // don't count missings:
                if (r.CatComponents[columnName] == "") continue;

                if (!Columns[columnName].Categories.Contains(r.CatComponents[columnName]))
                {
                    Columns[columnName].Categories.Add(r.CatComponents[columnName]);
                }
            }
        }

        public void ChangeType(string columnName)
        {
            if (Columns[columnName].Type == DataType.Categorical)
            {
                foreach (DataRow r in Rows)
                {
                    double dummy;
                    if (!double.TryParse(r.CatComponents[columnName], out dummy))
                    {
                        dummy = double.NaN;
                    }
                    r.NumComponents.Add(columnName, dummy);
                    r.CatComponents.Remove(columnName);
                }
                Columns[columnName].Type = DataType.Numerical;
                Columns[columnName].Categories = null;
            }
            else
            {
                foreach (DataRow r in Rows)
                {
                    if (double.IsNaN(r.NumComponents[columnName]))
                    {
                        r.CatComponents.Add(columnName, "");
                    }
                    else
                    {
                        r.CatComponents.Add(columnName, r.NumComponents[columnName].ToString());
                    }
                    r.NumComponents.Remove(columnName);
                }
                Columns[columnName].Type = DataType.Categorical;
                countCategories(columnName);
            }
        }

        public void Read(string fileName)
        {
            // read the file, treating all table entries as strings
            Table t = new Table(fileName);
            nRows = t.NRows - 1;
            nCols = t.NColumns;

            // characterize each column
            for (int iCol = 0; iCol < nCols; iCol++)
            {
                Columns.Add(t.Data[0, iCol], new DataColumn(inferType(t, iCol)));
            }

            // parse each element into the appropriate data type
            for (int iRow = 1; iRow < NRows + 1; iRow++)
            {
                DataRow row = new DataRow();
                int iCol = 0;
                foreach (string columnName in Columns.Keys)
                {
                    if (Columns[columnName].Type == DataType.Categorical)
                    {
                        row.CatComponents.Add(columnName, t.Data[iRow, iCol]);
                    }
                    else
                    {
                        double dummy;
                        if (!double.TryParse(t.Data[iRow, iCol], out dummy))
                        {
                            dummy = double.NaN;
                        }
                        row.NumComponents.Add(columnName, dummy);
                    }
                    iCol++;
                }
                Rows.Add(row);
            }
            // count the number of categories for categorical data
            countCategories();
        }

        private void indexRows()
        {
            int index = 0;
            foreach (DataRow row in Rows)
            {
                row.Index = index;
                index++;
            }
        }

        private DataType inferType(TBKMath.Table t, int iCol)
        {
            int numbers = 0;
            int totalNonMissing = 0;
            double dummy;
            for (int iRow = 1; iRow < t.NRows; iRow++)
            {
                if (double.TryParse(t.Data[iRow, iCol], out dummy) | t.Data[iRow, iCol] == "NaN")
                {
                    numbers++;
                }

                if (t.Data[iRow, iCol] != "")
                {
                    totalNonMissing++;
                }
            }
            if (numbers > 0.5 * totalNonMissing)
            {
                return DataType.Numerical;
            }
            else
            {
                return DataType.Categorical;
            }
        }

        public int RemoveMissings()
        {
            int nRemoved = 0;
            for (int i = 0; i < nRows; i++)
            {
                bool toBeRemoved = false;
                foreach (string columnName in Rows[i].CatComponents.Keys)
                {
                    if (Rows[i].CatComponents[columnName] == "")
                    {
                        toBeRemoved = true;
                        break;
                    }
                }
                if (!toBeRemoved)
                {
                    foreach (string columnName in Rows[i].NumComponents.Keys)
                    {
                        if (double.IsNaN(Rows[i].NumComponents[columnName]))
                        {
                            toBeRemoved = true;
                            break;
                        }
                    }
                }
                if (toBeRemoved)
                {
                    Rows.Remove(Rows[i]);
                    i--;
                    nRows--;
                    nRemoved++;
                }
            }
            // recount categories, in case any has been removed
            countCategories();

            // reindex the rows to be sequential
            indexRows();

            return nRemoved;
        }

        public int RemoveMissings(List<string> columnNames)
        {
            // removes those rows that have missing data in one of 
            // the columns indicated in the argument
            int nRemoved = 0;
            for (int i = 0; i < nRows; i++)
            {
                bool toBeRemoved = false;
                foreach (string columnName in columnNames)
                {
                    if (!Rows[i].CatComponents.ContainsKey(columnName)) continue;
                    if (Rows[i].CatComponents[columnName] == "")
                    {
                        toBeRemoved = true;
                        break;
                    }
                }
                if (!toBeRemoved)
                {
                    foreach (string columnName in columnNames)
                    {
                        if (!Rows[i].NumComponents.ContainsKey(columnName)) continue;
                        if (double.IsNaN(Rows[i].NumComponents[columnName]))
                        {
                            toBeRemoved = true;
                            break;
                        }
                    }
                }
                if (toBeRemoved)
                {
                    Rows.Remove(Rows[i]);
                    i--;
                    nRows--;
                    nRemoved++;
                }
            }

            // recount categories, in case any has been removed
            countCategories();
            return nRemoved;
        }
    }

    public class DataRow
    {
        public DataRow()
        {
            NumComponents = new Dictionary<string, double>();
            CatComponents = new Dictionary<string, string>();
        }

        public Dictionary<string, double> NumComponents;
        public Dictionary<string, string> CatComponents;
        public int Index;
    }

    public enum DataType { Numerical, Categorical, Unknown };

    public class DataColumn
    {
        public DataColumn(DataType type)
        {
            Type = type;
        }

        public List<string> Categories;
        public DataType Type;
    }
}
