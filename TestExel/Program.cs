
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.IO;
using TestExel;

class Program
{           
    static void Main()
    {
        // Путь к файлу Excel
        string excelFilePath = "C:\\Users\\User\\Desktop\\Projects\\TestExel\\TestExel\\test.xlsx";
        using (var workbook = new XLWorkbook(excelFilePath))
        {
            var pumpService = new PumpService();
            int[] outTempWarm = { -7, 2, 7, 12, -7, 2, 7, 12 };
            int[] inTempWarm = { 35, 35, 31, 26, 55, 55, 46, 34 };
            var pumpsWarm = pumpService.Test(workbook, outTempWarm, inTempWarm, "Warm");
            int[] outTempMid = { -20, -15, -10, -7, 2, 7, 12, -20, -15, -10, -7, 2, 7, 12};
            int[] inTempMid = { 35, 35, 35, 34, 30, 27, 24, 55, 55, 55, 52, 42, 36, 30 };
            var pumpsMid = pumpService.Test(workbook, outTempMid, inTempMid, "Mid");
            int[] outTempCold = { -20, -7, 2, 7, 12, -20, -7, 2, 7, 12};
            int[] inTempCold = { 35, 30, 27, 25, 24, 55, 44, 37, 32, 30 };
            var pumpsCold = pumpService.Test(workbook, outTempCold, inTempCold, "Cold");
            foreach (var pump in pumpsMid)
            {
                Console.WriteLine($"Pump: {pump.Name}, Type: {pump.Type}");
                foreach (var dataPair in pump.Data)
                {
                    Console.WriteLine($"  Time: {dataPair.Key}");
                    foreach (var data in dataPair.Value)
                    {
                        Console.WriteLine($"    Temp: {data.Temp}, Climate: {data.Climate} , HC: {data.MaxHC}, COP: {data.MaxCOP}");
                    }
                }
            }
            

        }
    }
}
