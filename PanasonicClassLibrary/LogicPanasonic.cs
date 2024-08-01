using PanasonicClassLibrary.DBService;
using PanasonicClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace PanasonicClassLibrary
{
    public class LogicPanasonic
    {
        private const int ID_Company_In_DB = 2820;
        private const int Num_Climate = 3; //Number of climates in which the pumps operate
        private PumpServiceForDBPanasonic _pumpDBServiceForPanasonic;
        public LogicPanasonic(string dataBasePath)
        {
            _pumpDBServiceForPanasonic = new PumpServiceForDBPanasonic(dataBasePath);
        }
        public async Task GoalLogicPanasonic()
        {
            string excelFilePath;
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Panasonic: ");
                Console.WriteLine("1. For Luft");
                Console.WriteLine("2. Exit!");
                var typePumpForPanasonic = Console.ReadLine();

                switch (typePumpForPanasonic)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Panasonicl (Luft):");
                        excelFilePath = "E:\\Work\\wpoExcelToDBConveter\\TestExel\\Panasonic\\PanasonicNewPumps08_24.xlsx"; //Console.ReadLine();
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


            //foreach (var pump in standartPumpsForPanasonic)
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
            var _pumpServiceForPanasonic = new PumpServicePanasonic(excelFilePath);
            var standartPumpsForPanasonic = _pumpServiceForPanasonic.CreateListStandartPumps();
            var oldPumpsForPanasonic = _pumpServiceForPanasonic.GetAllPumpsFromExel();

            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempWarmFor55, inTempMidWarm55, 55, "3");

            await ChooseWhatUpdate(standartPumpsForPanasonic, oldPumpsForPanasonic, "Luft");
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
                            await _pumpDBServiceForPanasonic.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForPanasonic.ChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
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
