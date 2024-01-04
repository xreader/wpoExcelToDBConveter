
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TestExel;
using TestExel.StandartModels;

class Program
{           
    static void Main()
    {
        Stopwatch stopwatch = new Stopwatch();

        // Запускаем таймер
        stopwatch.Start();

        // Путь к файлу Excel
        string excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx";
        var pumpService = new PumpService(excelFilePath);

        var standartPumps = pumpService.CreateListStandartPumps();
        var oldPumps = pumpService.GetAllPumpsFromExel();
        //int[] outTempWarm = { -7, 2, 7, 12, -7, 2, 7, 12 };
        //int[] inTempWarm = { 35, 35, 31, 26, 55, 55, 46, 34 };
        //pumpService.GetDataInListStandartPumps(standartPumps, outTempWarm, inTempWarm, "Warm");

        int[] outTempMidFor35 = {-25, -10, -7, 2, 7, 12};
        int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24};
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35,"Mittel");
        int[] outTempMidFor55 = {-20, -10, -7, 2, 7, 12 };
        int[] inTempMidFor55 = {55, 55, 52, 42, 36, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "Mittel");

        int[] outTempColdFor35 = { -25,-22, -15, -7, 2, 7, 12 };
        int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "Kalt");
        int[] outTempColdFor55 = { -20, -15, -7, 2, 7, 12 };
        int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "Kalt");
        stopwatch.Stop();

        // Выводим результат в консоль
        Console.WriteLine($"Время выполнения кода: {stopwatch.Elapsed}");
        foreach (var pump in standartPumps)
        {
            Console.WriteLine($"Pump: {pump.Name}");
            foreach (var dataPair in pump.Data)
            {
                Console.WriteLine($"  Temp Out: {dataPair.Key}");
                foreach (var data in dataPair.Value)
                {
                    Console.WriteLine($"Climat: {data.Climate}");
                    Console.WriteLine($" TempFor: {data.ForTemp}    FlowTemp: {data.FlowTemp}, MinHC: {data.MinHC},MidHC: {data.MidHC},MaxHC: {data.MaxHC}, MinCOP: {data.MinCOP}, MidCOP: {data.MidCOP}, MaxCOP: {data.MaxCOP}");
                }
            }
        }


    }
}
