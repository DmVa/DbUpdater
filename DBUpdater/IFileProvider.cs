using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUpdater
{
    interface IFileProvider
    {
        /// Scripts that should be executed for correctly setup Verifing table.
        /// </summary>
        List<UpdateScript> InitVerifingTable { get; }

        /// <summary>
        /// Scripts that should be executed and registered in Verifing table.
        /// </summary>
        List<UpdateScript> Scripts { get;  }
         void Read();
         string FullSourceName { get; }
    }
}
