
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
            int[] a = { -7, 2, 7, 12, -7, 2, 7, 12 };
            int[] b = { 35, 35, 31, 26, 55, 55, 46, 34 };
            var pumps = pumpService.Test(workbook, a, b);
            foreach (var pump in pumps)
            {
                Console.WriteLine($"Pump: {pump.Name}, Type: {pump.Type}");
                foreach (var dataPair in pump.Data)
                {
                    Console.WriteLine($"  Time: {dataPair.Key}");
                    foreach (var data in dataPair.Value)
                    {
                        Console.WriteLine($"    Temp: {data.Temp}, HC: {data.MaxHC}, COP: {data.MaxCOP}");
                    }
                }
            }
            

        }
    }
}
