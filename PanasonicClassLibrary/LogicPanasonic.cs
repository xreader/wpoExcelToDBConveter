using PanasonicClassLibrary.DBService;
using PanasonicClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace PanasonicClassLibrary
{
    public class LogicPanasonic
    {
        private const int ID_Company_In_DB = 2820;
        private const int Num_Climate = 3; //Number of climates in which the pumps operate
        private PumpServiceForDBPanasonic _pumpDBServiceForPanasonic;
        public LogicPanasonic(string dataBasePath)
        {
            _pumpDBServiceForPanasonic = new PumpServiceForDBPanasonic(dataBasePath);
        }
        public async Task GoalLogicPanasonic()
        {
            string excelFilePath;
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Exel File For Panasonic: ");
                Console.WriteLine("1. For Luft");
                Console.WriteLine("2. Exit!");
                var typePumpForPanasonic = Console.ReadLine();

                switch (typePumpForPanasonic)
                {
                    case "1":
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        Console.WriteLine("Write full path to Excel File for Panasonicl (Luft):");
                        //excelFilePath = "E:\\Work\\wpoExcelToDBConveter\\TestExel\\Panasonic\\PanasonicNewPumps08_24.xlsx"; 
                        excelFilePath = Console.ReadLine();
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


            //foreach (var pump in standartPumpsForPanasonic)
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
            var _pumpServiceForPanasonic = new PumpServicePanasonic(excelFilePath);
            var standartPumpsForPanasonic = _pumpServiceForPanasonic.CreateListStandartPumps();
            var oldPumpsForPanasonic = _pumpServiceForPanasonic.GetAllPumpsFromExel();

            int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };

            int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempMidFor35, inTempMidFor35, 35, "2");

            int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -22, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -22, -15, -7, 2, 7, 12 };
            int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempColdFor55, inTempMidCold55, 55, "1");
            int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
            int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempWarmFor35, inTempWarmFor35, 35, "3");
            int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
            int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
            _pumpServiceForPanasonic.GetDataInListStandartPumpsForLuftPanasonic(standartPumpsForPanasonic, oldPumpsForPanasonic, outTempWarmFor55, inTempMidWarm55, 55, "3");

            await ChooseWhatUpdate(standartPumpsForPanasonic, oldPumpsForPanasonic, "Luft");
        }

        // =====================================================
        // WHITELIST: Nur diese WPs importieren (aus PAD-Liste kopieren)
        // Format: Einfach aus Excel kopieren, eine WP pro Zeile
        // Wenn leer ("") → alle WPs werden importiert
        // =====================================================
        private static readonly string _whitelistRaw = @"
WH-ADC0509L3E51 + WH-WDG05LE5
WH-ADC0509L3E51 + WH-WDG07LE5
WH-ADC0509L3E51 + WH-WDG09LE5
WH-ADC0509L3E5UK1 + WH-WDG05LE5
WH-ADC0509L3E5UK1 + WH-WDG07LE5
WH-ADC0509L3E5UK1 + WH-WDG09LE5
WH-ADC16K6E5 + WH-UDZ16KE5
WH-ADC16K6E5UK + WH-UDZ16KE5
WH-ADC16K6E5AN + WH-UDZ16KE5
WH-SDC16K6E5 + WH-UDZ16KE5
WH-ADC0916M3E51 + WH-WDG12ME5
WH-ADC0916M3E51 + WH-WDG16ME5
WH-ADC0916M3E5UK1 + WH-WDG12ME5
WH-ADC0916M3E5UK1 + WH-WDG16ME5
WH-ADC0316M9E81 + WH-WXG09ME8
WH-ADC0316M9E81 + WH-WXG12ME8
WH-ADC0316M9E81 + WH-WXG16ME8
WH-ADC0916M3E51 + WH-WXG09ME5
WH-ADC0916M3E51 + WH-WXG12ME5
WH-ADC0916M3E5UK1 + WH-WXG09ME5
WH-ADC0916M3E5UK1 + WH-WXG12ME5
WH-ADC16K6E53 + WH-UDZ16KE5
WH-ADC16K6E5AN3 + WH-UDZ16KE5
WH-ADC16K6E5UK3 + WH-UDZ16KE5
WH-ADC0916M3E52 + WH-WDG12ME5
WH-ADC0916M3E52 + WH-WDG16ME5
WH-ADC0916M3E5UK2 + WH-WDG12ME5
WH-ADC0916M3E5UK2 + WH-WDG16ME5
WH-ADC0916M3E5AN2 + WH-WDG12ME5
WH-ADC0916M3E5AN2 + WH-WDG16ME5
WH-ADC0916M6E52 + WH-WDG12ME5
WH-ADC0916M6E52 + WH-WDG16ME5
WH-ADC0916M3E53 + WH-WDG12ME5
WH-ADC0916M3E53 + WH-WDG16ME5
WH-ADC0916M3E5UK3 + WH-WDG12ME5
WH-ADC0916M3E5UK3 + WH-WDG16ME5
WH-ADC0916M3E5AN3 + WH-WDG12ME5
WH-ADC0916M3E5AN3 + WH-WDG16ME5
WH-ADC0916M6E53 + WH-WDG12ME5
WH-ADC0916M6E53 + WH-WDG16ME5
WH-SDC0916M3E5 + WH-WDG12ME5
WH-SDC0916M3E5 + WH-WDG16ME5
WH-SDC0916M6E5 + WH-WDG12ME5
WH-SDC0916M6E5 + WH-WDG16ME5
WH-WDG12ME5
WH-WDG16ME5
WH-WDG12ME5 + WH-CME5
WH-WDG16ME5 + WH-CME5
";

        private static string NormalizeSpaces(string s)
        {
            // Normalize whitespace and ensure " + " format (handles "+" without spaces)
            var result = System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s*\+\s*", " + ");
            return result;
        }

        private static HashSet<string> GetWhitelist()
        {
            if (string.IsNullOrWhiteSpace(_whitelistRaw))
                return null; // null = kein Filter, alle importieren

            return new HashSet<string>(
                _whitelistRaw.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => NormalizeSpaces(line))
                    .Where(line => !string.IsNullOrEmpty(line)),
                StringComparer.OrdinalIgnoreCase
            );
        }

        private static bool IsInWhitelist(string pumpName, HashSet<string> whitelist)
        {
            if (whitelist == null) return true; // kein Filter
            return whitelist.Contains(NormalizeSpaces(pumpName));
        }

        private async Task ChooseWhatUpdate(List<StandartPump> standartPumps, List<Pump> oldPumps, string typePump)
        {
            var whitelist = GetWhitelist();
            if (whitelist != null)
            {
                Console.WriteLine($"\nWHITELIST aktiv: {whitelist.Count} WPs zum Import freigegeben.");
            }
            else
            {
                Console.WriteLine("\nKein Whitelist-Filter, alle WPs werden importiert.");
            }

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
                        int count14825 = 0, skip14825 = 0;
                        foreach (var pump in standartPumps)
                        {
                            if (!IsInWhitelist(pump.Name, whitelist))
                            {
                                Console.WriteLine($"Pump: {pump.Name} übersprungen (nicht in Whitelist).");
                                skip14825++;
                                continue;
                            }
                            await _pumpDBServiceForPanasonic.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                            count14825++;
                        }
                        Console.WriteLine($"\n14825: {count14825} importiert, {skip14825} übersprungen (nicht in Whitelist).");
                        break;
                    case "2":
                        int countLst = 0, skipLst = 0;
                        foreach (var pump in oldPumps)
                        {
                            if (!IsInWhitelist(pump.Name, whitelist))
                            {
                                skipLst++;
                                continue;
                            }
                            try
                            {
                                await _pumpDBServiceForPanasonic.ChangeLeistungsdatenInDbByExcelData(pump, typePump, ID_Company_In_DB);
                                Console.WriteLine("OK!");
                                countLst++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"ERROR bei Pump '{pump.Name}': {ex.Message}");
                                Console.WriteLine("Pump übersprungen, fahre fort...");
                            }
                        }
                        Console.WriteLine($"\nLeistungsdaten: {countLst} importiert, {skipLst} übersprungen (nicht in Whitelist).");
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