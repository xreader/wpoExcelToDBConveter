
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using TestExel;
using TestExel.DBConnection;
using TestExel.DBModels;
using TestExel.Services;
using TestExel.StandartModels;

class Program
{           
    static void Main()
    {
        // Створити об'єкт Stopwatch
        Stopwatch stopwatch = new Stopwatch();

        // Почати вимірювання часу
        stopwatch.Start();
        string excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx";
        var pumpService = new PumpService(excelFilePath);

        var standartPumps = pumpService.CreateListStandartPumps();
        var oldPumps = pumpService.GetAllPumpsFromExel();

        //var myPump = oldPumps.Where(x => x.Name == "YKF30CRB");

        int[] outTempMidFor35 = { -25, -10, -7, 2, 7, 12 };
        int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2");
        int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
        int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2");

        int[] outTempColdFor35 = { -25, -22, -15, -7, 2, 7, 12 };
        int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1");
        int[] outTempColdFor55 = { -20, -15,-10, -7, 2, 7, 12 };
        int[] inTempMidCold55 = { 55, 55,55, 44, 37, 32, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1");



        var pumpServiceForDB = new PumpServiceForDB("D:\\Work\\wpopt-server\\wpoServer\\bin\\Debug\\wpov5_referenz_change.db");
        //foreach (var pump in standartPumps)
        //{
        //    pumpServiceForDB.GoalLogic(pump);

        //}


        var pump = standartPumps.FirstOrDefault(x => x.Name == "YKF30CRB");
        pumpServiceForDB.GoalLogic(pump);

        stopwatch.Stop();

        // Отримати час, що пройшов
        TimeSpan elapsedTime = stopwatch.Elapsed;

        // Вивести результат
        Console.WriteLine($"Час виконання: {elapsedTime.TotalMilliseconds} мс");



    }
    

    static void GoalLogic()
    {
        // Путь к файлу Excel
        string excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx";
        var pumpService = new PumpService(excelFilePath);

        var standartPumps = pumpService.CreateListStandartPumps();
        var oldPumps = pumpService.GetAllPumpsFromExel();
        //int[] outTempWarm = { -7, 2, 7, 12, -7, 2, 7, 12 };
        //int[] inTempWarm = { 35, 35, 31, 26, 55, 55, 46, 34 };
        //pumpService.GetDataInListStandartPumps(standartPumps, outTempWarm, inTempWarm, "Warm");

        int[] outTempMidFor35 = { -25, -10, -7, 2, 7, 12 };
        int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "Mittel");
        int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
        int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "Mittel");

        int[] outTempColdFor35 = { -25, -22, -15, -7, 2, 7, 12 };
        int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "Kalt");
        int[] outTempColdFor55 = { -20, -15, -7, 2, 7, 12 };
        int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "Kalt");
        //foreach (var pump in standartPumps)
        //{
        //    Console.WriteLine($"Pump: {pump.Name}");
        //    foreach (var dataPair in pump.Data)
        //    {
        //        Console.WriteLine($"  Temp Out: {dataPair.Key}");
        //        foreach (var data in dataPair.Value)
        //        {
        //            Console.WriteLine($"Climat: {data.Climate}");
        //            Console.WriteLine($" TempFor: {data.ForTemp}    FlowTemp: {data.FlowTemp}, MinHC: {data.MinHC},MidHC: {data.MidHC},MaxHC: {data.MaxHC}, MinCOP: {data.MinCOP}, MidCOP: {data.MidCOP}, MaxCOP: {data.MaxCOP}");
        //        }
        //    }
        //}

    }
    
}
