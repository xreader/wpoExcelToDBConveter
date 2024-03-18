using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;
using YorkClassLibrary.DBService;
using YorkClassLibrary.Services;

namespace YorkClassLibrary
{
    public class LogicYork
    {
        private const int ID_Company_In_DB = 135287;
        private const int Num_Climate = 2; //Number of climates in which the pumps operate
        private const string Type_Pump = "Luft"; //In York all pumps are only Luft
        private readonly PumpServiceForDBYork _pumpServiceForDBYork;
        public LogicYork(string dataBasePath)
        {
            _pumpServiceForDBYork = new PumpServiceForDBYork(dataBasePath);
        }
        public async Task GoalLogicYourk()
        {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Console.WriteLine("Write full path to Excel File for York:");
            var excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\York.xlsx";//Console.ReadLine();
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            var pumpServiceForYork = new PumpServiceYork(excelFilePath);
            var standartPumpsForYork = pumpServiceForYork.CreateListStandartPumps();
            var oldPumpsForYork = pumpServiceForYork.GetAllPumpsFromExel();
            int[] outTempMidFor35 = { -25, -10, -7, 2, 7, 12 };
            int[] inTempMidFor35 = { 35, 35, 34, 30, 27, 24 };
            pumpServiceForYork.GetDataInListStandartPumps(standartPumpsForYork, oldPumpsForYork, outTempMidFor35, inTempMidFor35, 35, "2");
            int[] outTempMidFor55 = { -20, -10, -7, 2, 7, 12 };
            int[] inTempMidFor55 = { 55, 55, 52, 42, 36, 30 };
            pumpServiceForYork.GetDataInListStandartPumps(standartPumpsForYork, oldPumpsForYork, outTempMidFor55, inTempMidFor55, 55, "2");

            int[] outTempColdFor35 = { -25, -22, -15, -7, 2, 7, 12 };
            int[] inTempColdFor35 = { 35, 35, 35, 30, 27, 25, 24 };
            pumpServiceForYork.GetDataInListStandartPumps(standartPumpsForYork, oldPumpsForYork, outTempColdFor35, inTempColdFor35, 35, "1");
            int[] outTempColdFor55 = { -20, -15, -10, -7, 2, 7, 12 };
            int[] inTempColdFor55 = { 55, 55, 55, 44, 37, 32, 30 };
            pumpServiceForYork.GetDataInListStandartPumps(standartPumpsForYork, oldPumpsForYork, outTempColdFor55, inTempColdFor55, 55, "1");
            
            bool exit = true;
            while (exit)
            {
                Console.WriteLine();
                Console.WriteLine("Choose operation: ");
                Console.WriteLine("1. Update Dataen EN 14825 LG");
                Console.WriteLine("2. Update Leistungsdaten");
                Console.WriteLine("3. Back!");
                var operationForYork = Console.ReadLine();
                switch (operationForYork)
                {
                    case "1":
                        foreach (var pump in standartPumpsForYork)
                        {
                            
                            await _pumpServiceForDBYork.ChangeDataenEN14825LGInDbByExcelData(pump, Type_Pump, ID_Company_In_DB,Num_Climate);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumpsForYork)
                        {
                            await _pumpServiceForDBYork.ChangeLeistungsdatenInDbByExcelData(pump, Type_Pump, ID_Company_In_DB);
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
