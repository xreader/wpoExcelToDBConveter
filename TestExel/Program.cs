
using ClosedXML.Excel;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using TestExel;
using TestExel.DBConnection;
using TestExel.StandartModels;

class Program
{           
    static  void Main()
    {
        string excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx";
        var pumpService = new PumpService(excelFilePath);

        var standartPumps = pumpService.CreateListStandartPumps();
        var oldPumps = pumpService.GetAllPumpsFromExel();
        //int[] outTempWarm = { -7, 2, 7, 12, -7, 2, 7, 12 };
        //int[] inTempWarm = { 35, 35, 31, 26, 55, 55, 46, 34 };
        //pumpService.GetDataInListStandartPumps(standartPumps, outTempWarm, inTempWarm, "Warm");

        int[] outTempMidFor35 = { -25, -10, -7, 2, 7, 12 };
        int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2");
        int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
        int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2");

        int[] outTempColdFor35 = { -25, -22, -15, -7, 2, 7, 12 };
        int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1");
        int[] outTempColdFor55 = { -20, -15, -7, 2, 7, 12 };
        int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
        pumpService.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1");

        var myPump = standartPumps.FirstOrDefault(x => x.Name == "YKF12CRC");
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseSqlite("Data Source=D:\\Work\\wpopt-server\\wpoServer\\bin\\Debug\\wpov5_referenz_change.db;") 
        .Options;

        using (var _context = new ApplicationDBContext(options))
        {
            var wp = _context.leaves.FirstOrDefault(x => x.value == myPump.Name); // находим насос
            if(wp != null)
            {
                var wpId = wp.nodeid_fk_nodes_nodeid; //находим его айди
                var Idnid = wpId+1;
                while(_context.leaves.Count(x =>x.nodeid_fk_nodes_nodeid == Idnid) == 6) // Всегда 6 записей в которых храняться данные 
                {
                    var dataWp = _context.leaves.Where(x=>x.nodeid_fk_nodes_nodeid == Idnid).ToList();
                    var WPleistATemp = dataWp.Find(x => x.objectid_fk_properties_objectid == 1351); // берем температуру на улице
                    if(WPleistATemp != null) 
                    {
                        var WPleistATempValue = WPleistATemp.value_as_int;
                        if (myPump.Data.TryGetValue((int)WPleistATempValue, out var myPumpData)) // проеверяем есть ли данные при такой температуре на улице
                        {
                            var WPleistVTemp = dataWp.Find(x => x.objectid_fk_properties_objectid == 1011);
                            var RefKlimazone14825 = dataWp.Find(x => x.objectid_fk_properties_objectid == 1356);
                            if (WPleistVTemp != null && RefKlimazone14825!=null)
                            {
                                var WPleistVTempValue = WPleistVTemp.value_as_int;
                                var RefKlimazone14825Value = RefKlimazone14825.value_as_int;
                                var dataPumpForThisData = myPumpData.FirstOrDefault(x => x.ForTemp == WPleistVTempValue && x.Climate == RefKlimazone14825Value.ToString());
                                if(dataPumpForThisData != null)
                                {
                                    var WPleistHeiz = dataWp.Find(x => x.objectid_fk_properties_objectid == 1012);                                    
                                    var WPleistCOP = dataWp.Find(x => x.objectid_fk_properties_objectid == 1221);
                                    WPleistHeiz.value_as_int = (int)(dataPumpForThisData.MinHC * 100);
                                    WPleistCOP.value_as_int = (int)(dataPumpForThisData.MinCOP * 100);
                                }
                            }
                        }
                    }
                    
                }
            }


            //var a = _context.leaves.Where(x => x.nodeid_fk_nodes_nodeid == 137057).ToList();
            //var str = "#" + -25 + "#" + 6 + "#" + 7;
            //str = 74934 + str;
            //int hash = GetHashCode(str); // = -1774235343
            
            
        }






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
    static int GetHashCode(string s)
    {
        int hash = 0;
        int len = s.Length;

        if (len == 0)
            return hash;

        for (int i = 0; i < len; i++)
        {
            char chr = s[i];
            hash = ((hash << 5) - hash) + chr;
            hash |= 0; // Convert to 32-bit integer
        }

        return hash;
    }
}
