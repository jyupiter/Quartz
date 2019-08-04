using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Classes
{
    public class Log
    {
        public int index { get; set; }
        public string changes { get; set; }
        public DateTime dateTime { get; set; }
        public string path { get; set; }

        public Log(int i, string c,string d, string t, string p)
        {
            dateTime = Convert.ToDateTime(d + " " + t);
            index = i;
            changes = c;
            path = p;
        }
    }
}
