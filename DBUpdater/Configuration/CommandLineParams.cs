using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater.Configuration
{
    public class CommandLineParams
    {
        public bool? RunFromConsole { get; set; }
        public string ConnectionString { get; set; }
    }

}
