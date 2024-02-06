using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace TestExel.Services.ServicesForDifferentCompany
{
    class PumpServiceForYork : PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpServiceForYork(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
       
       
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var pump = new Pump(worksheet);
                pump.Name = worksheet.Name;
                pump.GetData(2, "B", "C", "I", 35);
                pump.GetData(13, "B", "C", "I", 55);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        

    }


}
