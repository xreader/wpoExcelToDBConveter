﻿using AlphaInnotecClassLibrary.DBService;
using AlphaInnotecClassLibrary.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.ServicesForDB;
using TestExel.StandartModels;

namespace AlphaInnotecClassLibrary
{
    public class LogicAlphaInnotec
    {
        private PumpServiceForAlphaInnotec _pumpServiceForAlphaInnotec;
        private PumpServiceForDBAlphaInotec _pumpDBServiceForAlphaInnotec;
        public async Task GoalLogicAlphaInnotec(string dataBasePath)
        {
            _pumpDBServiceForAlphaInnotec = new PumpServiceForDBAlphaInotec(dataBasePath);
            string excelFilePath;
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Alpha Innotec: ");
                Console.WriteLine("1. For Luft");
                Console.WriteLine("2. For Sole");
                Console.WriteLine("3. For Wasser");
                Console.WriteLine("4. Exit!");
                var typePumpForAlphaInnotec = Console.ReadLine();

                switch (typePumpForAlphaInnotec)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Alpha Innotec (Luft):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\LuftAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\LuftAlphaInnotec.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await LuftLogic(excelFilePath, dataBasePath);

                        break;
                    case "2":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Alpha Innotec (Sole):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\SoleAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\SoleAlphaInnotec.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await SoleLogic(excelFilePath, dataBasePath);
                        break;
                    case "3":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Alpha Innotec (Wasser):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await WasserLogic(excelFilePath, dataBasePath);
                        break;
                    case "4":
                        exit = false;
                        break; // Go back to company selection
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }
        }
        
        public async Task LuftLogic(string excelFilePath, string dataBasePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2,12,"B","D","J");
            oldPumps[0].Name = "ABAMA10T/Luft"; //My test pump
            ConvertToStandartForAlpaInnotecForLuft(standartPumps, oldPumps,"Luft");
            await ChooseWhatUpdate(standartPumps, oldPumps, "Luft");
            //ViewData(standartPumps);


        }
        public async Task SoleLogic(string excelFilePath, string dataBasePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2, 4, "B", "D", "J");
            ConvertToStandartForAlpaInnotecForWasserAndSole(standartPumps, oldPumps,"Sole");
            await ChooseWhatUpdate(standartPumps, oldPumps, "Sole");

            //ViewData(standartPumps);

        }
        public async Task WasserLogic(string excelFilePath, string dataBasePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2, 4, "B", "D", "J");
            ConvertToStandartForAlpaInnotecForWasserAndSole(standartPumps, oldPumps, "Wasser");
            await ChooseWhatUpdate(standartPumps, oldPumps, "Wasser");

            //ViewData(standartPumps);

        }
        public async Task ChooseWhatUpdate(List<StandartPump> standartPumps, List<Pump> oldPumps, string typePump)
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
                            await _pumpDBServiceForAlphaInnotec.ChangeDataenEN14825LGInDbByExcelData(pump, typePump);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForAlphaInnotec.ChangeLeistungsdatenInDbByExcelData(pump, typePump);
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
        public void ConvertToStandartForAlpaInnotecForLuft(List<StandartPump> standartPumps, List<Pump> oldPumps, string typeFile)
        {
            int[] outTempMidFor35 = { -20, -10, -7,  2,  7, 12 };
            int[] inTempMidFor35 = {   35,  35, 34, 30, 27, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2", typeFile);

            int[] outTempMidFor55 = { -20, -10, -7,  2,  7, 12 };
            int[] inTempMidFor55 = {   55,  55, 52, 42, 36, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2", typeFile);

            int[] outTempColdFor35 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1", typeFile);
            int[] outTempColdFor55 = { -20,-10, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55,44, 37, 32, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1", typeFile);
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempWarmFor35, inTempWarmFor35, 35, "3", typeFile);
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempWarmFor55, inTempMidWarm55, 55, "3", typeFile);
        }
        public void ConvertToStandartForAlpaInnotecForWasserAndSole(List<StandartPump> standartPumps, List<Pump> oldPumps, string typeFile)
        {
            int[] outTempMidFor35 = { -22, -15, -10, -7, 2, 7, 12 };
            int[] inTempMidFor35 = { 35, 35, 35, 34, 30, 27, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2", typeFile);

            int[] outTempMidFor55 = { -22, -15, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 55, 52, 42, 36, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2", typeFile);

            int[] outTempColdFor35 = { -22, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1", typeFile);
            int[] outTempColdFor55 = { -22, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1", typeFile);
            int[] outTempWarmFor35 = { -7, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 31, 26 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempWarmFor35, inTempWarmFor35, 35, "3", typeFile);
            int[] outTempWarmFor55 = { -7, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 46, 34 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumpsAlpha(standartPumps, oldPumps, outTempWarmFor55, inTempMidWarm55, 55, "3", typeFile);
        }

        public void ViewData(List<StandartPump> standartPumps)
        {
            foreach (var standartPump in standartPumps)
            {
                Console.WriteLine("Name " + standartPump.Name);
                foreach (var data in standartPump.Data)
                {
                    Console.WriteLine("Temp Out" + data.Key);
                    foreach (var datas in data.Value)
                    {
                        Console.WriteLine("--------Climate: " + datas.Climate);
                        Console.WriteLine("----------------ForTemp: " + datas.ForTemp);
                        Console.WriteLine("------------------------FlowTemp: " + datas.FlowTemp);
                        Console.WriteLine("------------------------MaxVorlauftemperatur: " + datas.MaxVorlauftemperatur);
                        Console.WriteLine("------------------------MinHC: " + datas.MinHC);
                        Console.WriteLine("------------------------MidHC: " + datas.MidHC);
                        Console.WriteLine("------------------------MaxHC: " + datas.MaxHC);
                        Console.WriteLine("------------------------MinCOP: " + datas.MinCOP);
                        Console.WriteLine("------------------------MidCOP: " + datas.MidCOP);
                        Console.WriteLine("------------------------MaxCOP: " + datas.MaxCOP);

                    }
                }
            }
        }


        

    }
}