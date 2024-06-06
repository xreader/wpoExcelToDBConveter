using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Models;
using TestExel.Repository;
using TestExel.ServicesForDB;
using TestExel.StandartModels;

namespace HovalClassLibrary.DBService
{
    internal class PumpServiceForDBHoval : PumpServiceForDB
    {
        
        public PumpServiceForDBHoval(string pathDB) : base(pathDB)
        {
            
        }
    }
}
