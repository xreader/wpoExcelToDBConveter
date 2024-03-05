using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;

namespace HovalClassLibrary.Services
{
    internal class PumpServiceHoval : PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpServiceHoval(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            //var sheetsCount = workbook.Worksheets.Count;
            //for (int i = 1; i <= sheetsCount; i++)
            //{
            //    var worksheet = workbook.Worksheet(i);
            //    var pump = new Pump(worksheet);
            //    pump.Name = worksheet.Name;
            //    pump.GetData(2, "B", "C", "I", 35);
            //    pump.GetData(13, "B", "C", "I", 55);
            //    if (pump != null && pump.Name != "")
            //        pumps.Add(pump);

            //}
            //RoundCOPAndP(pumps);
            return pumps;
        }
    }
}
