using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace TBKMath
{
    public class EstimationProcess
    {
        public EstimationProcess() { }

        public delegate double objectiveFunctionDelegate(List<double> point);

        public objectiveFunctionDelegate function;
        public objectiveFunctionDelegate Function
        {
            set { function = value; }
        }

        private List<double> point;
        public List<double> Point
        {
            set { point = value; }
            get { return point; }
        }

        private List<double> scale;
        public List<double> Scale
        {
            set { scale = value; }
            get { return scale; }
        }

        public virtual void Start() { }

        public int MaxNumIterations;
        public int NumIterations;

        public bool verbose = false;

        public HistoryFile HF;

    }

    public class HistoryFile
    {
        public HistoryFile(string name) 
        {
            Name = name;
            fi = new FileInfo(Name);
        }

        public void Close()
        {
            sw.Close();
        }

        public void WriteHeaderInfo() { }

        public void ReadHeaderInfo() { }

        public void AppendHeaderInfo() { }

        public void ReadData() { }

        public void AppendData(double functionValue, List<double> parameters) 
        {
            if (sw == null)
            {
                sw = fi.AppendText();
            }
            sw.Write(functionValue.ToString());
            foreach (double p in parameters)
            {
                sw.Write("\t" + p.ToString());
            }
            sw.WriteLine();
            sw.Flush();
        }

        public void WriteData() { }

        public string Name;

        private FileInfo fi;
        private StreamWriter sw;
    }
}
