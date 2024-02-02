
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
    static async Task Main()
    {
        Console.WriteLine("Write full path to Excel File:");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx"
        string excelFilePath = Console.ReadLine();

        Console.WriteLine("Write full path to Data Base:");//"D:\\Work\\wpopt-server\\wpoServer\\bin\\Debug\\wpov5_referenz_change.db"
        string dataBasePath = Console.ReadLine();


        var pumpService = new PumpServiceForAlphaInnotec(excelFilePath);

        var standartPumps = pumpService.CreateListStandartPumps();
        var oldPumps = pumpService.GetAllPumpsFromExel();
        string dataBasePath = Console.ReadLine();
        //int[] outTempMidFor35 = { -25, -10, -7, 2, 7, 12 };
        //int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
        //pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2");
        //int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
        //int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
        //pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2");

        //int[] outTempColdFor35 = { -25, -22, -15, -7, 2, 7, 12 };
        //int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
        //pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1");
        //int[] outTempColdFor55 = { -20, -15, -10, -7, 2, 7, 12 };
        //int[] inTempMidCold55 = { 55, 55, 55, 44, 37, 32, 30 };
        //pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1");


        //var pumpServiceForDB = new PumpServiceForDB(dataBasePath);

        //while (true)
        //{
        //    Console.WriteLine();
        //    Console.WriteLine("Choose operation: ");
        //    Console.WriteLine("1. Update Dataen EN 14825 LG");
        //    Console.WriteLine("2. Update Leistungsdaten");
        //    var operation = Console.ReadLine();
        //    switch (operation)
        //    {
        //        case "1":
        //            foreach (var pump in standartPumps)
        //            {
        //                await pumpServiceForDB.ChangeDataenEN14825LGInDbByExcelData(pump);
        //            }
        //            break;
        //        case "2":
        //            foreach (var pump in oldPumps)
        //            {
        //                await pumpServiceForDB.ChangeLeistungsdatenInDbByExcelData(pump);
        //            }
        //            break;
        //        default:
        //            Console.WriteLine("Error input");
        //            break;
        //    }
        //}      
    }
}
    

