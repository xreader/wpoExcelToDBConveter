using RemehaClassLibrary.DBService;
using RemehaClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace RemehaClassLibrary
{
    public class LogicRemeha
    {
        private const int ID_Company_In_DB = 64677;//
        private const int Num_Climate = 3; //Number of climates in which the pumps operate
        private PumpServiceForDBRemeha _pumpDBServiceForRemeha;
        public LogicRemeha(string dataBasePath)
        {
            _pumpDBServiceForRemeha = new PumpServiceForDBRemeha(dataBasePath);
        }


        public async Task GoalLogicRemeha()
        {
            string excelFilePath;
            bool exit = true;

            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Remeha: ");
                Console.WriteLine("1. For Luft");                
                Console.WriteLine("2. Exit!");
                var typePumpRemeha = Console.ReadLine();

                switch (typePumpRemeha)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Hoval (Luft):");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\LuftAlphaInnotec.xlsx"
                        excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\RemehaLuft.xlsx";//Console.ReadLine();
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        await LuftLogic(excelFilePath);
                        break;
                    case "2":
                        exit = false;
                        break; // Go back to company selection                        
                    default:
                        Console.WriteLine("Error input");
                        break;
                }
            }
        }
        private async Task LuftLogic(string excelFilePath)
        {
            var _pumpServiceForRemeha = new PumpServiceRemeha(excelFilePath);
            var standartPumpsForRemeha = _pumpServiceForRemeha.CreateListStandartPumps();
            var oldPumpsForRemeha = _pumpServiceForRemeha.GetAllPumpsFromExel();
            

            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 30, 27, 25, 24 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 44, 37, 32, 30 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForRemeha.GetDataInListStandartPumpsForLuftRemeha(standartPumpsForRemeha, oldPumpsForRemeha, outTempWarmFor55, inTempMidWarm55, 55, "3");

            foreach (var pump in standartPumpsForRemeha)
            {
                Console.WriteLine(pump.Name);

                foreach (var kvp in pump.Data)
                {
                    Console.WriteLine($"Key: {kvp.Key}");

                    foreach (var dataPump in kvp.Value)
                    {
                        Console.WriteLine($"Temp: {dataPump.ForTemp}");
                        Console.WriteLine($"FlowTemp: {dataPump.FlowTemp}");
                        Console.WriteLine($"Climate: {dataPump.Climate}");
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
            await ChooseWhatUpdate(standartPumpsForRemeha, oldPumpsForRemeha, "Luft");
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
                            await _pumpDBServiceForRemeha.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumps)
                        {
                            await _pumpDBServiceForRemeha.ChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
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
