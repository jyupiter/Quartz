using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quartz.Classes
{
    class UpdateStuff
    {
        public string AppName { get; set; }
        public string AppVers { get; set; }

        public UpdateStuff()
        {
            //Default Constructor
        }

        public UpdateStuff(string AppName, string AppVers)
        {
            this.AppName = AppName;
            this.AppVers = AppVers;
        }
    }
}
