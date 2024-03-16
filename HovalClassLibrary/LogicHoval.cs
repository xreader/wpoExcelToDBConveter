using HovalClassLibrary.DBService;
using HovalClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace HovalClassLibrary
{
    public class LogicHoval
    {
        public async Task GoalLogicHoval(string dataBasePath)
        {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Console.WriteLine("Write full path to Excel File for Hoval:");
            var excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalLuft.xlsx";//Console.ReadLine();
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            var _pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllPumpsFromExel();
            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = {-10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -10, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3");
            foreach (var pump in standartPumpsForHoval)
            {
                Console.WriteLine(pump.Name);

                foreach (var kvp in pump.Data)
                {
                    Console.WriteLine($"Key: {kvp.Key}");

                    foreach (var dataPump in kvp.Value)
                    {
                        Console.WriteLine($"Temp: {dataPump.ForTemp}");
                        Console.WriteLine($"FlowTemp: {dataPump.FlowTemp}");
                        Console.WriteLine($"Climate: {dataPump.Climate}");
                        Console.WriteLine($"MaxVorlauftemperatur: {dataPump.MaxVorlauftemperatur}");
                        Console.WriteLine($"MinHC: {dataPump.MinHC}");
                        Console.WriteLine($"MidHC: {dataPump.MidHC}");
                        Console.WriteLine($"MaxHC: {dataPump.MaxHC}");
                        Console.WriteLine($"MinCOP: {dataPump.MinCOP}");
                        Console.WriteLine($"MidCOP: {dataPump.MidCOP}");
                        Console.WriteLine($"MaxCOP: {dataPump.MaxCOP}");

                        Console.WriteLine();
                    }
                }
            }

            //var pumpServiceForDBForYork = new PumpServiceForDBHoval(dataBasePath);
            //bool exit = true;
            //while (exit)
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("Choose operation: ");
            //    Console.WriteLine("1. Update Dataen EN 14825 LG");
            //    Console.WriteLine("2. Update Leistungsdaten");
            //    Console.WriteLine("3. Back!");
            //    var operationForYork = Console.ReadLine();
            //    switch (operationForYork)
            //    {
            //        case "1":
            //            foreach (var pump in standartPumpsForHoval)
            //            {

            //                //await pumpServiceForDBForYork.ChangeDataenEN14825LGInDbByExcelData(pump);
            //            }
            //            break;
            //        case "2":
            //            foreach (var pump in oldPumpsForHoval)
            //            {
            //               //await pumpServiceForDBForYork.ChangeLeistungsdatenInDbByExcelData(pump);
            //            }
            //            break;
            //        case "3":
            //            exit = false;
            //            break; // Go back to company selection
            //        default:
            //            Console.WriteLine("Error input");
            //            break;
            //    }
            //}

        }
    }
}
