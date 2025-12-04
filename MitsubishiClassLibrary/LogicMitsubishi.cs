using MitsubishiClassLibrary.DBService;
using MitsubishiClassLibrary.Services;
using TestExel.Models;
using TestExel.StandartModels;

namespace MitsubishiClassLibrary;

public class LogicMitsubishi
{
    private const int ID_Company_In_DB = 171702;//
    private const int Num_Climate = 3; //Number of climates in which the pumps operate
    private PumpServiceForDBMitsubishi _pumpDBServiceForMitsubishi;
    public LogicMitsubishi(string dataBasePath)
    {
       _pumpDBServiceForMitsubishi = new PumpServiceForDBMitsubishi(dataBasePath);
    }
    public async Task GoalLogicMitsubishi()
    {
        string excelFilePath;
        bool exit = true;
        while (exit)
        {
            Console.WriteLine();
            Console.WriteLine("Choose Exel File For Mitsubishi: ");
            Console.WriteLine("1. For Luft");
            Console.WriteLine("2. Exit!");
            var typePumpForMitsubishi = Console.ReadLine();

            switch (typePumpForMitsubishi)
            {
                case "1":
                    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Console.WriteLine("Write full path to Excel File for Mitsubishil (Luft):");
                    excelFilePath = Console.ReadLine();//"E:\\Work\\wpoExcelToDBConveter\\TestExel\\Mitsubishi\\MitsubishiDATA.xlsx"; 
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
        var _pumpServiceForMitsubishi = new PumpServiceMitsubishi(excelFilePath);
        var standartPumpsForMitsubishi = _pumpServiceForMitsubishi.CreateListStandartPumps();
        var oldPumpsForMitsubishi = _pumpServiceForMitsubishi.GetAllPumpsFromExel();

        int[] outTempMidFor35 = { -10, -7, 2, 7, 12 };
        int[] inTempMidFor35 = { 35, 34, 30, 27, 24 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempMidFor35, inTempMidFor35, 35, "2");

        int[] outTempMidFor55 = { -10, -7, 2, 7, 12 };
        int[] inTempMidFor55 = { 55, 52, 42, 36, 30 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempMidFor55, inTempMidFor55, 55, "2");

        int[] outTempColdFor35 = { -22, -15, -7, 2, 7, 12 };
        int[] inTempColdFor35 = { 35, 35, 30, 27, 25, 24 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempColdFor35, inTempColdFor35, 35, "1");
        int[] outTempColdFor55 = { -22, -15, -7, 2, 7, 12 };
        int[] inTempMidCold55 = { 55, 55, 44, 37, 32, 30 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempColdFor55, inTempMidCold55, 55, "1");
        int[] outTempWarmFor35 = { -7, 2, 2, 7, 12 };
        int[] inTempWarmFor35 = { 35, 35, 35, 31, 26 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempWarmFor35, inTempWarmFor35, 35, "3");
        int[] outTempWarmFor55 = { -7, 2, 2, 7, 12 };
        int[] inTempMidWarm55 = { 55, 55, 55, 46, 34 };
        _pumpServiceForMitsubishi.GetDataInListStandartPumpsForLuftMitsubishi(standartPumpsForMitsubishi, oldPumpsForMitsubishi, outTempWarmFor55, inTempMidWarm55, 55, "3");

        await ChooseWhatUpdate(standartPumpsForMitsubishi, oldPumpsForMitsubishi, "Luft");
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
                       
                        await _pumpDBServiceForMitsubishi.ChangeDataenEN14825LGInDbByExcelData(pump, typePump, ID_Company_In_DB, Num_Climate);
                    }
                    break;
                case "2":
                    foreach (var pump in oldPumps)
                    {
                        var a = new Pump { Name = pump.Name,
                        Data = pump.Data
                                        .ToDictionary(
                                            kvp => kvp.Key,
                                            kvp => kvp.Value.Where(d => d.Temp is 35 or 55)
                                            .Where(x => x.MaxCOP != 0 && x.MaxHC != 0).ToList()

                                        )
                        };

                        await _pumpDBServiceForMitsubishi.ChangeLeistungsdatenInDbByExcelData(a, typePump, ID_Company_In_DB);
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
