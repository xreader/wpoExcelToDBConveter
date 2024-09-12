using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;

namespace EcoforestClassLibrary.DBService
{
    internal class PumpServiceForDBEcoforest : PumpServiceForDB
    {
        public PumpServiceForDBEcoforest(string pathDB) : base(pathDB)
        {
        }
    }
}
