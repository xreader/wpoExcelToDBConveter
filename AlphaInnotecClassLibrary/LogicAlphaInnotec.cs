using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services.ServicesForDifferentCompany;
using TestExel.StandartModels;

namespace AlphaInnotecClassLibrary
{
    public class LogicAlphaInnotec
    {
        private PumpServiceForAlphaInnotec _pumpServiceForAlphaInnotec;

        public async Task GoalLogicAlphaInnotec(string dataBasePath)
        {
            string excelFilePath;
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
                    LuftLogic(excelFilePath);
                    
                    break;
                case "2":
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Console.WriteLine("Write full path to Excel File for Alpha Innotec (Sole):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\SoleAlphaInnotec.xlsx"
                    excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\SoleAlphaInnotec.xlsx";//Console.ReadLine();
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    SoleLogic(excelFilePath);
                    break;
                case "3":
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Console.WriteLine("Write full path to Excel File for Alpha Innotec (Wasser):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx"
                    excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\WasserAlphaInnotec.xlsx";//Console.ReadLine();
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    WasserLogic(excelFilePath);
                    break;
                case "4":
                    break; // Go back to company selection
                default:
                    Console.WriteLine("Error input");
                    break;
            }
        }
        
        public void LuftLogic(string excelFilePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2,12,"B","D","J");
            ConvertToStandartForAlpaInnotec(standartPumps, oldPumps);

        }
        public void SoleLogic(string excelFilePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2, 4, "B", "D", "J");
            ConvertToStandartForAlpaInnotec(standartPumps, oldPumps);

        }
        public void WasserLogic(string excelFilePath)
        {
            _pumpServiceForAlphaInnotec = new PumpServiceForAlphaInnotec(excelFilePath);
            var standartPumps = _pumpServiceForAlphaInnotec.CreateListStandartPumps();
            var oldPumps = _pumpServiceForAlphaInnotec.GetAllPumpsFromExel(2, 4, "B", "D", "J");
            ConvertToStandartForAlpaInnotec(standartPumps, oldPumps);
        }
        
        public void ConvertToStandartForAlpaInnotec(List<StandartPump> standartPumps, List<Pump> oldPumps)
        {
            int[] outTempMidFor35 = { -22, -15, -10, -7, 2, 7, 12 };
            int[] inTempMidFor35 = { 35, 35, 35, 34, 30, 27, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -22, -15, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 55, 52, 42, 36, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -22, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -22, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 31, 26 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 46, 34 };
            _pumpServiceForAlphaInnotec.GetDataInListStandartPumps(standartPumps, oldPumps, outTempWarmFor55, inTempMidWarm55, 55, "3");
        }

        //foreach (var standartPump in standartPumps)
        //{
        //    Console.WriteLine("Name " + standartPump.Name);
        //    foreach (var data in standartPump.Data)
        //    {
        //        Console.WriteLine("Temp Out" + data.Key);
        //        foreach (var datas in data.Value)
        //        {
        //            Console.WriteLine("--------Climate: " + datas.Climate);
        //            Console.WriteLine("--------FlowTemp: " + datas.FlowTemp);
        //            Console.WriteLine("--------MaxVorlauftemperatur: " + datas.MaxVorlauftemperatur);
        //            Console.WriteLine("--------ForTemp: " + datas.ForTemp);
        //            Console.WriteLine("--------MinHC: " + datas.MinHC);
        //            Console.WriteLine("--------MidHC: " + datas.MidHC);
        //            Console.WriteLine("--------MaxHC: " + datas.MaxHC);
        //            Console.WriteLine("--------MinCOP: " + datas.MinCOP);
        //            Console.WriteLine("--------MidCOP: " + datas.MidCOP);
        //            Console.WriteLine("--------MaxCOP: " + datas.MaxCOP);

        //        }
        //    }
        //}

        //oldPumps[0].Name = "WWC 100HX";
        //var pumpServiceForDB = new PumpServiceForDBAlphaInotec(dataBasePath);
        //await pumpServiceForDB.ChangeLeistungsdatenInDbByExcelData(oldPumps[0]);
        //Console.WriteLine("OK!");

    }
}
