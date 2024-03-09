using HovalClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;

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
            foreach (var pump in oldPumpsForHoval)
            {
                Console.WriteLine(pump.Name);

                foreach (var kvp in pump.Data)
                {
                    Console.WriteLine($"Key: {kvp.Key}");

                    foreach (var dataPump in kvp.Value)
                    {
                        Console.WriteLine($"Temp: {dataPump.Temp}");
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
