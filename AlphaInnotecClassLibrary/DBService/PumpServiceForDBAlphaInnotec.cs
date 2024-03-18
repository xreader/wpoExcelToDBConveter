using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Models;
using TestExel.Repository;
using TestExel.ServicesForDB;
using TestExel.StandartModels;


namespace AlphaInnotecClassLibrary.DBService
{
    internal class PumpServiceForDBAlphaInotec : PumpServiceForDB
    {
        
        public PumpServiceForDBAlphaInotec(string pathDB) : base(pathDB)
        {
           
        }
    }
}
