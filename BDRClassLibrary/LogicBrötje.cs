using BrötjeClassLibrary.DBService;
using BrötjeClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace BrötjeClassLibrary
{
    public class LogicBrötje
    {
        private const int ID_Company_In_DB = 51166;//I dont now
        private const int Num_Climate = 3; //Number of climates in which the pumps operate
        private PumpServiceForDBBrötje _pumpDBServiceForBrötje;
        public LogicBrötje(string dataBasePath)
        {
            _pumpDBServiceForBrötje = new PumpServiceForDBBrötje(dataBasePath);
        }
        public async Task GoalLogicBrötje()
        {
            string excelFilePath;
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Brötje: ");
                Console.WriteLine("1. For Luft");
                Console.WriteLine("2. Exit!");
                var typePumpForBrötje = Console.ReadLine();

                switch (typePumpForBrötje)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Brötjel (Luft):");
                        excelFilePath = Console.ReadLine();//"E:\\Work\\wpoExcelToDBConveter\\TestExel\\Brötje\\BrötjeDATA.xlsx"; 
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await LuftLogic(excelFilePath);

                        break;
                    case "2":
                        exit = false;
                        break; // Go back to company selection
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }


            //foreach (var pump in standartPumpsForBrötje)
            //{
            //    Console.WriteLine(pump.Name);

            //    foreach (var kvp in pump.Data)
            //    {
            //        Console.WriteLine($"Key: {kvp.Key}");

            //        foreach (var dataPump in kvp.Value)
            //        {
            //            Console.WriteLine($"Temp: {dataPump.ForTemp}");
            //            Console.WriteLine($"FlowTemp: {dataPump.FlowTemp}");
            //            Console.WriteLine($"Climate: {dataPump.Climate}");
            //            Console.WriteLine($"MaxVorlauftemperatur: {dataPump.MaxVorlauftemperatur}");
            //            Console.WriteLine($"MinHC: {dataPump.MinHC}");
            //            Console.WriteLine($"MidHC: {dataPump.MidHC}");
            //            Console.WriteLine($"MaxHC: {dataPump.MaxHC}");
            //            Console.WriteLine($"MinCOP: {dataPump.MinCOP}");
            //            Console.WriteLine($"MidCOP: {dataPump.MidCOP}");
            //            Console.WriteLine($"MaxCOP: {dataPump.MaxCOP}");

            //            Console.WriteLine();
            //        }
            //    }
            //}



        }

        private async Task LuftLogic(string excelFilePath)
        {
            var _pumpServiceForBrötje = new PumpServiceBrötje(excelFilePath);
            var standartPumpsForBrötje = _pumpServiceForBrötje.CreateListStandartPumps();
            var oldPumpsForBrötje = _pumpServiceForBrötje.GetAllPumpsFromExel();

            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -22, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -22, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForBrötje.GetDataInListStandartPumpsForLuftBrötje(standartPumpsForBrötje, oldPumpsForBrötje, outTempWarmFor55, inTempMidWarm55, 55, "3");

            await ChooseWhatUpdate(standartPumpsForBrötje, oldPumpsForBrötje, "Luft");
        }

        private async Task ChooseWhatUpdate(List<StandartPump> standartPumps, List<Pump> oldPumps, string typePump)
        {
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose operation: ");
                Console.WriteLine("1. Update Dataen EN 14825 LG");
                Console.WriteLine("2. Update Leistungsdaten");
                Console.WriteLine("3. Back!");
                var operationForAlpha = Console.ReadLine();
                switch (operationForAlpha)
                {
                    case "1":
                        foreach (var pump in standartPumps)
                        {
                            await _pumpDBServiceForBrötje.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForBrötje.ChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
                            Console.WriteLine("OK!");
                        }
                        break;
                    case "3":
                        exit = false;
                        break; // Go back to company selection
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }
        }

    }
}
