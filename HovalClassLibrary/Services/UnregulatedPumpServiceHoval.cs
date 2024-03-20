using BaseClassLibrary.Models;
using BaseClassLibrary.Services;
using BaseClassLibrary.StandartModels;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HovalClassLibrary.Services
{
    internal class UnregulatedPumpServiceHoval : UnregulatedPumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);

        public UnregulatedPumpServiceHoval(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        //Get all pumps from Exel
        public List<UnregulatedPump> GetAllUnregulatedPumpsFromExel()
        {
            List<UnregulatedPump> pumps = new List<UnregulatedPump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var cell = worksheet.Cell("A3");
                var cellWithNamePump = new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString());

                var pump = new UnregulatedPump(worksheet);
                var cellWithDataPump = GetCellWithDataForPump(worksheet, cellWithNamePump);
                var countTempOut = cellWithDataPump[1].Num - cellWithDataPump[0].Num;
                pump.Name = cellWithNamePump.Data.ToString();
                var cellWith35GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "35");
                if (cellWith35GradData != null)
                    GetDataInUnregulated(cellWith35GradData, 35, pump, countTempOut, worksheet);
                var cellWith55GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "55");
                if (cellWith55GradData != null)
                    GetDataInUnregulated(cellWith55GradData, 55, pump, countTempOut, worksheet);
                GetMaxForlauftemperaturInUnregulatedPump(cellWithDataPump, pump, worksheet, countTempOut);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP_InUnregulatedPumps(pumps);
            return pumps;
        }
        public List<Cell> GetCellWithDataForPump(IXLWorksheet _sheet, Cell cellWithNamePump)
        {

            // Select cells by range
            var range = _sheet.Range(cellWithNamePump.Letter + (cellWithNamePump.Num + 1) + ":" + cellWithNamePump.Letter + 300);
            // Список для хранения адресов ячеек с заданным содержимым
            List<Cell> cellAddresses = new List<Cell>();
            // Проходим по каждой ячейке в диапазоне
            foreach (var cell in range.CellsUsed())
            {
                if (cell.GetString() != "tVL")
                    // Добавляем адрес ячейки в список
                    cellAddresses.Add(new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString()));

            }

            return cellAddresses;
        }
        public void GetDataInUnregulated(Cell adressFirstCell, int tempWaterIn, UnregulatedPump pump, int countTempOut, IXLWorksheet _sheet)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<UnregulatedDataPump>>();
            // Номер строки, содержащей данные
            int rowNumber = adressFirstCell.Num;

            // Буква столбца, с которого начинаются данные
            string startColumnLetter = adressFirstCell.Letter;

            // Получаем индекс столбца по его букве
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;


            for (int i = 0; i < countTempOut; i++)
            {

                var cellDataList = GetDataInRow(_sheet, rowNumber, startColumnIndex);
                pump.Data.TryGetValue(Convert.ToInt32(cellDataList[0]), out var datasPump);
                if (datasPump == null)
                    datasPump = new List<UnregulatedDataPump>();
                if (cellDataList.Contains("-"))
                {
                    // Замена всех вхождений "-" на "0" в каждой строке списка
                    for (int j = 1; j < cellDataList.Count; j++)
                    {
                        cellDataList[j] = cellDataList[j].Replace("-", "0");
                    }
                }
                datasPump.Add(new UnregulatedDataPump
                {
                    Temp = tempWaterIn,
                    HC = Convert.ToDouble(cellDataList[2]),
                    COP = Convert.ToDouble(cellDataList[3]),
                    MaxVorlauftemperatur = 666
                });



                if (!pump.Data.Any(x => x.Key == Convert.ToInt32(cellDataList[0])))
                    pump.Data.Add(Convert.ToInt32(cellDataList[0]), datasPump);
                rowNumber++;
            }



        }
        public List<string> GetDataInRow(IXLWorksheet _sheet, int rowNumber, int startColumnIndex)
        {
            // Создаем список для хранения данных из ячеек
            List<string> cellDataList = new List<string>();
            // Проходимся по каждому столбцу, начиная с указанного
            for (int columnIndex = startColumnIndex; ; columnIndex++)
            {
                // Получаем значение ячейки
                string cellValue = _sheet.Cell(rowNumber, columnIndex).GetString();

                // Проверяем, является ли значение пустым
                if (string.IsNullOrWhiteSpace(cellValue))
                {
                    // Если значение пустое, это означает, что строка закончилась, выходим из цикла
                    break;
                }

                // Добавляем значение ячейки в список
                cellDataList.Add(cellValue);
            }
            return cellDataList;
        }
        public void GetMaxForlauftemperaturInUnregulatedPump(List<Cell> adressCells, UnregulatedPump pump, IXLWorksheet _sheet, int countTempOut)
        {
            var lastCell = adressCells.FirstOrDefault(x => x.Data == "35");
            foreach (Cell cell in adressCells.Where(x => Convert.ToInt32(x.Data) > 35))
            {
                // Номер строки, содержащей данные
                int rowNumber = cell.Num;

                // Буква столбца, с которого начинаются данные
                string startColumnLetter = cell.Letter;

                // Получаем индекс столбца по его букве
                int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;


                for (int i = 0; i < countTempOut; i++)
                {

                    var cellDataList = GetDataInRow(_sheet, rowNumber, startColumnIndex);

                    pump.Data.TryGetValue(Convert.ToInt32(cellDataList[0]), out var datasPump);
                    if (datasPump == null)
                        datasPump = new List<UnregulatedDataPump>();
                    if (cellDataList.Count <= 1)
                    {
                        foreach (var data in datasPump)
                        {
                            data.MaxVorlauftemperatur = Convert.ToInt32(lastCell.Data);
                        }

                    }
                    else
                    {
                        if (cellDataList.Skip(1).All(item => item == "-"))
                        {
                            foreach (var data in datasPump)
                            {
                                data.MaxVorlauftemperatur = Convert.ToInt32(lastCell.Data);
                            }
                        }
                        else
                        {
                            foreach (var data in datasPump)
                            {
                                data.MaxVorlauftemperatur = Convert.ToInt32(cell.Data);
                            }
                        }
                    }


                    rowNumber++;

                }
                lastCell = cell;
            }
        }
        public List<UnregulatedStandartPump> UnregulatedGetDataInListStandartPumpsHoval(List<UnregulatedStandartPump> standartPumps, List<UnregulatedPump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat, string typeFile)
        {
            foreach (var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                Dictionary<int, List<UnregulatedDataPump>> oldDictionary = oldPump.Data;
                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<UnregulatedStandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    UnregulatedGetConvertDataAndCheckOutTemp(outTemps, flowTemps, forTemp, climat, newDictionary, oldDictionary);

                }
                else
                {
                    Dictionary<int, List<UnregulatedStandartDataPump>> newDictionary = new Dictionary<int, List<UnregulatedStandartDataPump>>();
                    UnregulatedGetConvertDataAndCheckOutTemp(outTemps, flowTemps, forTemp, climat, newDictionary, oldDictionary);
                    var standartPump = new UnregulatedStandartPump()
                    {
                        Name = oldPump.Name,
                        Data = newDictionary
                    };
                    standartPumps.Add(standartPump);
                }
            }

            return standartPumps;


        }
        //Get already converted data(get first value where count == 2)
        private void UnregulatedGetConvertDataAndCheckOutTemp(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<UnregulatedStandartDataPump>> newDictionary, Dictionary<int, List<UnregulatedDataPump>> oldDictionary)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {
                if (!oldDictionary.Keys.Contains(outTemps[i]))
                {
                    var firstDataForEachKey = oldDictionary.Values.Where(x => x.Count == 2).FirstOrDefault();
                    //Convert values
                    UnregulatedConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary);
                }
                else
                {
                    int[] outT = new int[] { outTemps[i] };
                    int[] flowT = new int[] { flowTemp[i] };
                    UnregulatedGetConvertData(outT, flowT, forTemp, climat, newDictionary, oldDictionary);
                }

            }
        }
    }
}
