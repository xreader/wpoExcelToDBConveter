using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.ServicesForDB;
using YorkClassLibrary.Services;

namespace YorkClassLibrary
{
    public class LogicYork
    {
        public async Task GoalLogicYourk( string dataBasePath)
        {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Console.WriteLine("Write full path to Excel File for York:");//"D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx"
            var excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\test.xlsx";//Console.ReadLine();
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
            int[] inTempMidCold55 = { 55, 55, 55, 44, 37, 32, 30 };
            pumpServiceForYork.GetDataInListStandartPumps(standartPumpsForYork, oldPumpsForYork, outTempColdFor55, inTempMidCold55, 55, "1");
            var pumpServiceForDBForYork = new PumpServiceForDB(dataBasePath);
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
                            await pumpServiceForDBForYork.ChangeDataenEN14825LGInDbByExcelData(pump);
                        }
                        break;
                    case "2":
                        foreach (var pump in oldPumpsForYork)
                        {
                            await pumpServiceForDBForYork.ChangeLeistungsdatenInDbByExcelData(pump);
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
