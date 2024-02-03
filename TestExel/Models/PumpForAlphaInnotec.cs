using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.Models
{
    internal class PumpForAlphaInnotec
    {
        public string Name { get; set; }
        public List<DataPump> Data { get; set; }
        private IXLWorksheet _sheet;

        public PumpForAlphaInnotec(IXLWorksheet sheet)
        {
            _sheet = sheet;
        }
        public PumpForAlphaInnotec()
        {

        }

        public void GetData(int numFirstDataLine, string letterColumnWithBeginningData, string letterColumnWithEndData, int inSystemGrad)
        {
            if (Data == null)
                Data = new List<DataPump>();
            List<double> getLineData = ReadExcelRangeToDoubleArray(letterColumnWithBeginningData + numFirstDataLine + ":" + letterColumnWithEndData + numFirstDataLine);
            //Add Values in dictionary 

            AddValuesInList(Data, getLineData, inSystemGrad);            
        }
        //Read numeric data from Excel
        private List<double> ReadExcelRangeToDoubleArray(string cellRange)
        {
            // Select cells by range
            var range = _sheet.Range(cellRange);

            // Convert data to double array
            var dataArray = range.Cells().Select(cell =>
            {
                string cellValue = cell.GetString();
                return string.IsNullOrWhiteSpace(cellValue) ? 0.0 : double.Parse(cellValue);
            }).ToList();

            return dataArray;
        }
        //Add the required data to the dictionary with pump data
        private static void AddValuesInList(List<DataPump> list, List<double> allData, int tempWaterIn)
        {
            list.Add(new DataPump
            {
                Temp = tempWaterIn,
                MinHC = allData[0],
                MidHC = allData[1],
                MaxHC = allData[2],
                MinCOP = allData[3],
                MidCOP = allData[4],
                MaxCOP = allData[5],
                MaxVorlauftemperatur = 65
            });
        }
    }
}
