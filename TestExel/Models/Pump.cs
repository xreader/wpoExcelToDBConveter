using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.Models
{
    class Pump
    {
        public string Name { get; set; }
        public Dictionary<int, List<DataPump>> Data { get; set; }

        protected IXLWorksheet _sheet;
        public Pump(IXLWorksheet sheet)
        {
            _sheet = sheet;
        }
        public Pump()
        {

        }

        public void GetData(int numFirstDataLine, string letterColumnWithOutsideTemp, string letterColumnWithBeginningData, string letterColumnWithEndData, int inSystemGrad)
        {
            int[] tempOut = GetAllTZ(letterColumnWithOutsideTemp, ref numFirstDataLine);
            if (Data == null)
                Data = new Dictionary<int, List<DataPump>>();
            foreach (var item in tempOut)
            {

                List<double> getLineData = ReadExcelRangeToDoubleArray(letterColumnWithBeginningData + numFirstDataLine + ":" + letterColumnWithEndData + numFirstDataLine);
                //Add Values in dictionary 

                AddValuesInDictionary(Data, getLineData, item, inSystemGrad);
                numFirstDataLine++;
            }
        }
        //Add the required data to the dictionary with pump data
        private static void AddValuesInDictionary(Dictionary<int, List<DataPump>> dictionary, List<double> allData, int tempOut, int tempWaterIn)
        {
            dictionary.TryGetValue(tempOut, out var datasPump);
            if (datasPump == null)
                datasPump = new List<DataPump>();
            if (!allData.Any(x => x == 0))
            {
                datasPump.Add(new DataPump
                {
                    Temp = tempWaterIn,
                    MinHC = allData[0],
                    MidHC = allData[1],
                    MaxHC = allData[2],
                    MinCOP = allData[3],
                    MidCOP = allData[4],
                    MaxCOP = allData[5],
                    MaxVorlauftemperatur = (int)allData[6]
                });

            }
            if (!dictionary.Any(x => x.Key == tempOut))
                dictionary.Add(tempOut, datasPump);
        }
        //We get all the temperatures outside
        private int[] GetAllTZ(string cellLetter, ref int firstNum)
        {
            List<int> dataArray = new List<int>();
            var num = firstNum;
            var lastTz = _sheet.Cell(cellLetter + (firstNum - 1)).GetString();
            var tz = "";
            if (lastTz == "" || lastTz == "Quelle" || lastTz == "temp out")
            {
                tz = _sheet.Cell(cellLetter + firstNum).GetString();
                while (tz == "")
                {
                    num++;
                    tz = _sheet.Cell(cellLetter + num).GetString();
                }
            }
            else
            {
                tz = lastTz;
                num--;
            }
            firstNum = num;

            while (!string.IsNullOrWhiteSpace(tz))
            {
                dataArray.Add(int.Parse(tz));
                num++;
                tz = _sheet.Cell(cellLetter + num).GetString();
            }
            return dataArray.ToArray();
        }
        //Read numeric data from Excel
        protected List<double> ReadExcelRangeToDoubleArray(string cellRange)
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
    }
}
