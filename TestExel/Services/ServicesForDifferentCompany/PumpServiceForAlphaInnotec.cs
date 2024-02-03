using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace TestExel.Services
{
    class PumpServiceForAlphaInnotec : PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpServiceForAlphaInnotec(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        //the method now only copies the values and transfers them to the standard,
        //provided that the temperature outside is already the same as in the old model and the temperature inside is also at the same temperature outside
        //and so far only for warm climates

        public List<StandartPump> GetDataInListStandartPumps(List<StandartPump> standartPumps, List<PumpForAlphaInnotec> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
        {
            foreach (var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    GetConvertData(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump.Data);

                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    GetConvertData(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump.Data);
                    var standartPump = new StandartPump()
                    {
                        Name = oldPump.Name,
                        Data = newDictionary
                    };
                    standartPumps.Add(standartPump);
                }
            }

            return standartPumps;


        }
                    
        //Get already converted data
        private void GetConvertData(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, List<DataPump> oldDictionary)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {
                //Code if there is no such temperature outside in the table
                //Search for data for a temperature outside when there is none
                //var oldDataPump = FindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                //Convert values
                ConvertDataInStandart(oldDictionary, flowTemp[i], outTemps[i], forTemp, climat, newDictionary);
            }
        }
        //Get all pumps from Exel
        public List<PumpForAlphaInnotec> GetAllPumpsFromExel()
        {
            List<PumpForAlphaInnotec> pumps = new List<PumpForAlphaInnotec>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var pump = new PumpForAlphaInnotec(worksheet);
                pump.Name = worksheet.Name;
                pump.GetData(2, "D", "I", 35);
                pump.GetData(3, "D", "I", 55);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        //Data rounding
        private static void RoundCOPAndP(List<PumpForAlphaInnotec> pumps)
        {
            foreach (var pump in pumps)
            {
                foreach (var data in pump.Data)
                {
                    data.MinCOP = Math.Round(data.MinCOP * 100) / 100;
                    data.MidCOP = Math.Round(data.MidCOP * 100) / 100;
                    data.MaxCOP = Math.Round(data.MaxCOP * 100) / 100;

                    data.MinHC = Math.Round(data.MinHC * 100) / 100;
                    data.MidHC = Math.Round(data.MidHC * 100) / 100;
                    data.MaxHC = Math.Round(data.MaxHC * 100) / 100;
                }
            }
        }

    }

}

