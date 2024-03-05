using HovalClassLibrary.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HovalClassLibrary
{
    public class LogicHoval
    {
        public async Task GoalLogicHoval(string dataBasePath)
        {
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            Console.WriteLine("Write full path to Excel File for Hoval:");
            var excelFilePath = "D:\\Work\\wpoExcelToDBConveter\\TestExel\\Hoval.xlsx";//Console.ReadLine();
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            var pumpServiceForHoval = new PumpServiceHoval(excelFilePath);
            var standartPumpsForHoval = pumpServiceForHoval.CreateListStandartPumps();
            var oldPumpsForHoval = pumpServiceForHoval.GetAllPumpsFromExel();
        }
    }
}
