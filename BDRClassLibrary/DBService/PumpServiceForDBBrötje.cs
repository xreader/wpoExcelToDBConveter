using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;

namespace BrötjeClassLibrary.DBService
{
    internal class PumpServiceForDBBrötje : PumpServiceForDB
    {
        public PumpServiceForDBBrötje(string pathDB) : base(pathDB)
        {
        }
    }
}
