using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;

namespace PanasonicClassLibrary.DBService
{
    internal class PumpServiceForDBPanasonic : PumpServiceForDB
    {
        public PumpServiceForDBPanasonic(string pathDB) : base(pathDB)
        {
        }
    }
}
