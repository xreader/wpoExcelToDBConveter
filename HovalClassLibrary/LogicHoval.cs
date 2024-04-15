using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
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
        private const int ID_Company_In_DB = 141876;
        private const int Num_Climate = 3; //Number of climates in which the pumps operate
        private PumpServiceForDBHoval _pumpDBServiceForHoval; 
        public LogicHoval(string dataBasePath)
        {
            _pumpDBServiceForHoval = new PumpServiceForDBHoval(dataBasePath);
        }
        public async Task GoalLogicHoval()
        {
            string excelFilePath;
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Hoval: ");
                Console.WriteLine("1. For Luft");
                Console.WriteLine("2. For Sole");
                Console.WriteLine("3. For Wasser");
                Console.WriteLine("4. For Sole unregulated pumps");
                Console.WriteLine("5. For Wasser unregulated pumps");
                Console.WriteLine("6. For Luft unregulated pumps");
                Console.WriteLine("7. Exit!");
                var typePumpForAlphaInnotec = Console.ReadLine();

                switch (typePumpForAlphaInnotec)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Hoval (Luft):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\LuftAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalLuft.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await LuftLogic(excelFilePath);

                        break;
                    case "2":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Hoval (Sole):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\SoleAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalSole.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await SoleLogic(excelFilePath);
                        break;
                    case "3":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Hoval (Wasser):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalWasser.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await WasserLogic(excelFilePath);
                        break;
                    case "4":
                        Console.WriteLine("Write full path to Excel File for Hoval (Sole unregulated pumps):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalSoleNicht_verstellbar.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await SoleLogicUnregulatedPumps(excelFilePath);
                        break;
                    case "5":
                        Console.WriteLine("Write full path to Excel File for Hoval (Wasser unregulated pumps):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\HovalWasserNicht_verstellbar.xlsx";//Console.ReadLine();
                        await WasserLogicUnregulatedPumps(excelFilePath);
                        break;
                    case "6":
                        Console.WriteLine("Write full path to Excel File for Hoval (Luft unregulated pumps):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                        excelFilePath = Console.ReadLine();
                        await LuftLogicUnregulatedPumps(excelFilePath);
                        break;
                    case "7":
                        exit = false;
                        break; // Go back to company selection
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }
           
            
            //foreach (var pump in standartPumpsForHoval)
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
            var _pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllPumpsFromExel();

            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.GetDataInListStandartPumpsForLuftHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3");
            var a = new List<StandartPump>();
            a.Add(standartPumpsForHoval.ElementAtOrDefault(3));
            a.Add(standartPumpsForHoval.ElementAtOrDefault(4));
            a.Add(standartPumpsForHoval.ElementAtOrDefault(5));
            a.Add(standartPumpsForHoval.ElementAtOrDefault(6));
            a.Add(standartPumpsForHoval.ElementAtOrDefault(7));
            await ChooseWhatUpdate(a, oldPumpsForHoval, "Luft");
        }
        private async Task SoleLogic(string excelFilePath)
        {
            var _pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllPumpsFromExel();
            int[] outTempMidFor35 = { -20, -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2", "Sole");

            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2", "Sole");

            int[] outTempColdFor35 = { -20, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1", "Sole");
            int[] outTempColdFor55 = { -20, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1", "Sole");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3", "Sole");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3", "Sole");
            await ChooseWhatUpdate(standartPumpsForHoval, oldPumpsForHoval, "Sole");
        }
        private async Task WasserLogic(string excelFilePath)
        {
            var _pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllPumpsFromExel();
            int[] outTempMidFor35 = {-20, -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2","Wasser");

            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2", "Wasser");

            int[] outTempColdFor35 = {-20, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1", "Wasser");
            int[] outTempColdFor55 = { -20, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1", "Wasser");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3", "Wasser");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.GetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3", "Wasser");
            await ChooseWhatUpdate(standartPumpsForHoval, oldPumpsForHoval, "Wasser");
        }
        private async Task LuftLogicUnregulatedPumps(string excelFilePath)
        {
            var _pumpServiceForHoval = new UnregulatedPumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListUnregulatedStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllUnregulatedPumpsFromExel();
            int[] outTempMidFor35 = { -20, -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2", "Luft");

            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2", "Luft");

            int[] outTempColdFor35 = { -22, -20, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1", "Luft");
            int[] outTempColdFor55 = { -22, -20, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1", "Luft");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3", "Luft");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3", "Luft");
            await UnregulatedChooseWhatUpdate(standartPumpsForHoval, oldPumpsForHoval, "Luft");
        }
        private async Task SoleLogicUnregulatedPumps(string excelFilePath)
        {
            var _pumpServiceForHoval = new UnregulatedPumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListUnregulatedStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllUnregulatedPumpsFromExel();
            int[] outTempMidFor35 = { -20, -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2", "Sole");

            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = {   55, 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2", "Sole");

            int[] outTempColdFor35 = {-22, -20, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1", "Sole");
            int[] outTempColdFor55 = {-22, -20, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = {55, 55, 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1", "Sole");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3", "Sole");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3", "Sole");            
            await UnregulatedChooseWhatUpdate(standartPumpsForHoval, oldPumpsForHoval, "Sole");
        }
        private async Task WasserLogicUnregulatedPumps(string excelFilePath)
        {
            var _pumpServiceForHoval = new UnregulatedPumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = _pumpServiceForHoval.CreateListUnregulatedStandartPumps();
            var oldPumpsForHoval = _pumpServiceForHoval.GetAllUnregulatedPumpsFromExel();
            int[] outTempMidFor35 = { -20, -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor35, inTempMidFor35, 35, "2", "Wasser");

            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempMidFor55, inTempMidFor55, 55, "2", "Wasser");

            int[] outTempColdFor35 = { -22, -20, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor35, inTempColdFor35, 35, "1", "Wasser");
            int[] outTempColdFor55 = { -22, -20, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 55, 44, 37, 32, 30 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempColdFor55, inTempMidCold55, 55, "1", "Wasser");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor35, inTempWarmFor35, 35, "3", "Wasser");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForHoval.UnregulatedGetDataInListStandartPumpsHoval(standartPumpsForHoval, oldPumpsForHoval, outTempWarmFor55, inTempMidWarm55, 55, "3", "Wasser");
            await UnregulatedChooseWhatUpdate(standartPumpsForHoval, oldPumpsForHoval, "Wasser");
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
                            await _pumpDBServiceForHoval.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForHoval.ChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
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
        private async Task UnregulatedChooseWhatUpdate(List<UnregulatedStandartPump> standartPumps, List<UnregulatedPump> oldPumps, string typePump)
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
                            await _pumpDBServiceForHoval.UnregulatedChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForHoval.UnregulatedChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
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
