using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;

namespace RemehaClassLibrary.DBService
{
    internal class PumpServiceForDBRemeha : PumpServiceForDB
    {
        public PumpServiceForDBRemeha(string pathDB) : base(pathDB)
        {
        }
    }
}
