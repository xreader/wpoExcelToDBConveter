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
            var excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\Hoval.xlsx";//Console.ReadLine();
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            var pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = pumpServiceForHoval.GetAllPumpsFromExel();
            int[] outTempMidFor35 = { -18, -10, -7, 2, 7, 12 };
            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -18, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -18, -10, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -18, -10, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            pumpServiceForHoval.GetDataInListStandartPumps(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3");

            foreach (var pump in standartPumpsForHoval)
            {
                Console.WriteLine(pump.Name);

                foreach (var kvp in pump.Data)
                {
                    Console.WriteLine($"Key: {kvp.Key}");

                    foreach (var dataPump in kvp.Value)
                    {
                        Console.WriteLine($"Climat: {dataPump.Climate}");
                        Console.WriteLine($"Temp: {dataPump.ForTemp}");
                        Console.WriteLine($"InTemp: {dataPump.FlowTemp}");
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
        }
    }
}
