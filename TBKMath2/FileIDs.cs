using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TBKMath
{
    // this is not a math class, but it will be put into this library
    // until I have a better place for it
    public class FileIDs
    {
        public FileIDs(string _root, string _suffix)
        {
            root = _root;
            suffix = _suffix;
            SetStamp();
        }

        public FileIDs(string _root)
        {
            root = _root;
            suffix = "";
            SetStamp();
        }

        public FileIDs()
        {
            root = "";
            suffix = "";
            SetStamp();
        }

        public void SetStamp()
        {
            // stamp has form : yydddhhmmss
            DateTime dt = DateTime.Now;
            stamp = dt.Year.ToString()
                + dt.DayOfYear.ToString("D3")
                + dt.Hour.ToString("D2")
                + dt.Minute.ToString("D2")
                + dt.Second.ToString("D2");
            fileName = root + stamp + "." + suffix;
        }

        public void ReStamp()
        {
            SetStamp();
        }

        private string root;
        public string Root
        {
            get { return root; }
            set 
            { 
                root = value;
                fileName = root + stamp + "." + suffix;
            }
        }

        private void constructFileName()
        {
            if (suffix.Length > 0)
            {
                fileName = root + stamp + "." + suffix;
            }
            else
            {
                fileName = root + stamp;
            }
        }

        private void deconstructFileName()
        {
            // extract the suffix, root, and stamp
            // to get suffix, find the last "." in the filename

            if (fileName == null)
            {
                root = "X";
                stamp = "000000000";
                suffix = "";
                return;
            }

            if (fileName == "")
            {
                root = "X";
                stamp = "000000000";
                suffix = "";
                return;
            }

            string[] parsed = fileName.Split('.');
            if (parsed.Count() == 1)
            {
                suffix = "";
            }
            else
            {
                suffix = parsed.Last();
            }

            int len = parsed[0].Length;
            if (len < 10) 
            {
                root = parsed[0];
                stamp = "000000000";
                return;
            }

            root = parsed[0].Substring(0,len-9);
            stamp = parsed[0].Substring(len - 9, 9);
        }

        private string stamp;
        public string Stamp
        {
            get { return stamp; }
            set 
            { 
                stamp = value;
                constructFileName();
            }
        }

        private string suffix;
        public string Suffix
        {
            get { return suffix; }
            set { suffix = value; }
        }

        private string fileName;
        public string FileName
        {
            get 
            {
                return fileName;
            }
            set { fileName = value; }
        }

    }
}
