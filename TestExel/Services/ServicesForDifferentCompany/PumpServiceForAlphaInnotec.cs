using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace TestExel.Services.ServicesForDifferentCompany
{
    class PumpServiceForAlphaInnotec : PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpServiceForAlphaInnotec(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        

        //Get already converted data(get first value where count == 2)
        protected override void GetConvertData(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Dictionary<int, List<DataPump>> oldDictionary)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {
                var firstDataForEachKey = oldDictionary.Values.Where(x=>x.Count == 2).FirstOrDefault();
                //Convert values
                ConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary);
            }
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
                pump.GetData(2, "B", "D", "J", 35);
                pump.GetData(4, "B", "D", "J", 55);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        

    }

}

