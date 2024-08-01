using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;
using TestExel.StandartModels;

namespace PanasonicClassLibrary.Services
{
    internal class PumpServicePanasonic : PumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);
        public record Vel(string Letter, int Num, string VelData); //Pump power percentage

        public PumpServicePanasonic(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                if (worksheet.Name.Contains("Data"))
                {
                    var cell = worksheet.Cell("I5");
                    var cellWithNamePump = new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString());

                    var pump = new Pump(worksheet);
                    pump.Name = cellWithNamePump.Data.ToString();

                    var vels = GetVelsPump(worksheet, "B", 17);
                    var cellsWithTemperatur = GetCellsWithTemperatures(worksheet, "D", 15);
                    GetDataForPump(worksheet, vels, cellsWithTemperatur, pump);
                    if (pump != null && pump.Name != "")
                        pumps.Add(pump);
                }
            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        private List<Vel> GetVelsPump(IXLWorksheet _sheet, string letter, int firstNum)
        {
            var vels = new List<Vel>();
            var cell = _sheet.Cell(letter + firstNum);
            while (cell.GetString() != "")
            {
                vels.Add(new Vel(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, VelData: cell.GetString()));
                firstNum++;
                cell = _sheet.Cell(letter + firstNum);
            }
            return vels;
        }
        private List<Cell> GetCellsWithTemperatures(IXLWorksheet _sheet, string firstLetter, int firstNum)
        {
            var cells = new List<Cell>();
            // Получаем индекс столбца по его букве
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(firstLetter);
            var cell = _sheet.Cell(firstNum, startColumnIndex);
            while (cell.GetString() != "")
            {
                if (cell.GetString().Contains("35") || cell.GetString().Contains("55"))
                {
                    cells.Add(new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString()));
                }
                startColumnIndex += 3;
                cell = _sheet.Cell(firstNum, startColumnIndex);
            }


            return cells;
        }
        private static (int TempOut, int Temp) ExtractNumbers(string input)
        {
            // Используем регулярное выражение для поиска чисел в строке
            Regex regex = new Regex(@"-?\d+");
            MatchCollection matches = regex.Matches(input);
            var tempOut = Convert.ToInt32(matches[0].Value);
            var temp = Convert.ToInt32(matches[1].Value);

            return (tempOut, temp);
        }
        private void GetDataForPump(IXLWorksheet _sheet, List<Vel> vels, List<Cell> cellsWithTemperatures, Pump pump)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();

            foreach (var cellWithTemperatures in cellsWithTemperatures)
            {
                var temps = ExtractNumbers(cellWithTemperatures.Data);

                pump.Data.TryGetValue((temps.TempOut), out var datasPump);
                if (datasPump == null)
                    datasPump = new List<DataPump>();

                int startColumnIndex = XLHelper.GetColumnNumberFromLetter(cellWithTemperatures.Letter);

                var velMin = GetVelMin(vels, startColumnIndex, _sheet);
                var velMid = GetVelMid(vels, startColumnIndex, _sheet);
                var velMax = GetVelMax(vels, startColumnIndex, _sheet);

                var minHC = _sheet.Cell(velMin.Num, startColumnIndex).Value.ToString();
                var minCOP = _sheet.Cell(velMin.Num, startColumnIndex + 2).Value.ToString();
                var midHC = _sheet.Cell(velMid.Num, startColumnIndex).Value.ToString();
                var midCOP = _sheet.Cell(velMid.Num, startColumnIndex + 2).Value.ToString();
                var maxHC = _sheet.Cell(velMax.Num, startColumnIndex).Value.ToString();
                var maxCOP = _sheet.Cell(velMax.Num, startColumnIndex + 2).Value.ToString();

                datasPump.Add(new DataPump()
                {
                    MaxVorlauftemperatur = 55,
                    Temp = temps.Temp,
                    MinHC = minHC == "-" ? 0 : Convert.ToDouble(minHC),
                    MinCOP = minCOP == "-" ? 0 : Convert.ToDouble(minCOP),
                    MidHC = midHC == "-" ? 0 : Convert.ToDouble(midHC),
                    MidCOP = midCOP == "-" ? 0 : Convert.ToDouble(midCOP),
                    MaxHC = maxHC == "-" ? 0 : Convert.ToDouble(maxHC),
                    MaxCOP = maxCOP == "-" ? 0 : Convert.ToDouble(maxCOP)
                });


                if (!pump.Data.Any(x => x.Key == temps.TempOut))
                    pump.Data.Add((temps.TempOut), datasPump);
            }
        }
        private Vel GetVelMin(List<Vel> vels, int startColumnIndex, IXLWorksheet _sheet)
        {
            for (var i = 0; i < vels.Count; i++)
            {
                if (_sheet.Cell(vels[i].Num, startColumnIndex).Value.ToString() != "-")
                    return vels[i];
            }
            return vels[0];
        }
        private Vel GetVelMid(List<Vel> vels, int startColumnIndex, IXLWorksheet _sheet)
        {
            List<Vel> velsWithData = new List<Vel>();

            for (var i = 0; i < vels.Count; i++)
            {
                if (_sheet.Cell(vels[i].Num, startColumnIndex).Value.ToString() != "-")
                    velsWithData.Add(vels[i]);
            }
            Vel midVel = velsWithData[0];
            double maxCOP = Convert.ToDouble(_sheet.Cell(velsWithData[0].Num, startColumnIndex + 2).Value.ToString());
            foreach (var velWithData in velsWithData)
            {
                var copVel = Convert.ToDouble(_sheet.Cell(velWithData.Num, startColumnIndex + 2).Value.ToString());
                if (maxCOP <= copVel)
                {
                    maxCOP = copVel;
                    midVel = velWithData;
                }
            }


            return midVel;
        }

        private Vel GetVelMax(List<Vel> vels, int startColumnIndex, IXLWorksheet _sheet)
        {
            for (var i = vels.Count - 1; i < vels.Count; i--)
            {
                if (_sheet.Cell(vels[i].Num, startColumnIndex).Value.ToString() != "-")
                    return vels[i];
            }
            return vels[vels.Count - 1];
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

        public void GetData(Cell adressFirstCell, int tempWaterIn, Pump pump, int countTempOut, IXLWorksheet _sheet)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            // Номер строки, содержащей данные
            int rowNumber = adressFirstCell.Num;

            // Буква столбца, с которого начинаются данные
            string startColumnLetter = adressFirstCell.Letter;

            // Получаем индекс столбца по его букве
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;


            for (int i = 0; i < countTempOut; i++)
            {

                var cellDataList = GetDataInRow(_sheet, rowNumber, startColumnIndex);
                if (!cellDataList.Skip(1).All(item => item == "-"))
                {
                    pump.Data.TryGetValue(Convert.ToInt32(cellDataList[0]), out var datasPump);
                    if (datasPump == null)
                        datasPump = new List<DataPump>();
                    if (cellDataList.Contains("-"))
                    {
                        // Замена всех вхождений "-" на "0" в каждой строке списка
                        for (int j = 1; j < cellDataList.Count; j++)
                        {
                            cellDataList[j] = cellDataList[j].Replace("-", "0");
                        }
                    }
                    datasPump.Add(new DataPump
                    {
                        Temp = tempWaterIn,
                        MinHC = Convert.ToDouble(cellDataList[7]),
                        MidHC = Convert.ToDouble(cellDataList[4]),
                        MaxHC = Convert.ToDouble(cellDataList[1]),
                        MinCOP = Convert.ToDouble(cellDataList[9]),
                        MidCOP = Convert.ToDouble(cellDataList[6]),
                        MaxCOP = Convert.ToDouble(cellDataList[3]),
                        MaxVorlauftemperatur = 35
                    });



                    if (!pump.Data.Any(x => x.Key == Convert.ToInt32(cellDataList[0])))
                        pump.Data.Add(Convert.ToInt32(cellDataList[0]), datasPump);
                }

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

        public void GetMaxForlauftemperatur(List<Cell> adressCells, Pump pump, IXLWorksheet _sheet, int countTempOut)
        {
            var lastCell = adressCells.FirstOrDefault(x => x.Data == "35");
            var listWithReadyMaxVor = new List<string>();
            ; foreach (Cell cell in adressCells.Where(x => Convert.ToInt32(x.Data) > 35))
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
                        datasPump = new List<DataPump>();
                    if (cellDataList.Count <= 1)
                    {
                        foreach (var data in datasPump)
                        {
                            data.MaxVorlauftemperatur = Convert.ToInt32(lastCell.Data);
                        }

                    }
                    else if (!listWithReadyMaxVor.Contains(cellDataList[0]))
                    {
                        if (cellDataList.Skip(1).All(item => item == "-"))
                        {
                            foreach (var data in datasPump)
                            {
                                data.MaxVorlauftemperatur = Convert.ToInt32(lastCell.Data);
                            }
                            listWithReadyMaxVor.Add(cellDataList[0]);
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

        public List<StandartPump> GetDataInListStandartPumpsForLuftPanasonic(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
        {
            foreach (var oldPump in oldPumps)
            {
                int[] flowTemps2 = flowTemps;
                int[] outTemps2 = outTemps;
                if (climat == "2" || climat == "1")
                {

                    int minKey = oldPump.Data
                                .Where(pair => pair.Value.Any(data => data.Temp == forTemp))
                                .Select(pair => pair.Key)
                                .DefaultIfEmpty() // Возвращаем значение по умолчанию (0), если нет удовлетворяющего ключа
                                .Min();
                    if (!outTemps.Contains(minKey))
                    {

                        bool correctOutTemp = climat == "1" && outTemps.Count() > 6 ? true
                                                 : climat == "2" && outTemps.Count() > 5 ? true :
                                                 false;
                        if (!correctOutTemp)
                        {
                            outTemps2 = new int[] { minKey }.Concat(outTemps).ToArray();
                            flowTemps2 = new int[] { forTemp }.Concat(flowTemps).ToArray();
                        }

                    }


                }


                //Get the pump data dictionary
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;

                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    GetConvertData(outTemps2, flowTemps2, forTemp, climat, newDictionary, oldDictionary, oldPump);

                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    GetConvertData(outTemps2, flowTemps2, forTemp, climat, newDictionary, oldDictionary, oldPump);
                    var standartPump = new StandartPump()
                    {
                        Name = oldPump.Name,
                        Data = newDictionary
                    };
                    standartPumps.Add(standartPump);
                }

            }
            return standartPumps;
        }

        public List<StandartPump> GetDataInListStandartPumpsPanasonic(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat, string typeFile)
        {
            foreach (var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    GetConvertDataAndCheckOutTemp(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump, typeFile);

                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    GetConvertDataAndCheckOutTemp(outTemps, flowTemps, forTemp, climat, newDictionary, oldPump, typeFile);
                    var standartPump = new StandartPump()
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
        private void GetConvertDataAndCheckOutTemp(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump, string typeFile)
        {
            Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
            for (int i = 0; i < outTemps.Length; i++)
            {
                if (!oldDictionary.Keys.Contains(outTemps[i]))
                {
                    List<DataPump> firstDataForEachKey = new List<DataPump>();
                    switch (typeFile)
                    {
                        case "Wasser":
                            oldDictionary.TryGetValue(10, out List<DataPump> dataWasser);
                            if (dataWasser != null)
                                firstDataForEachKey = dataWasser;
                            else
                                firstDataForEachKey = oldDictionary.Values.Where(x => x.Count == 2).FirstOrDefault();
                            break;
                        case "Sole":
                            oldDictionary.TryGetValue(0, out List<DataPump> dataSole);
                            if (dataSole != null)
                                firstDataForEachKey = dataSole;
                            else
                                firstDataForEachKey = oldDictionary.Values.Where(x => x.Count == 2).FirstOrDefault();
                            break;
                        default:
                            firstDataForEachKey = oldDictionary.Values.Where(x => x.Count == 2).FirstOrDefault();
                            break;
                    }
                    //Convert values
                    ConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);
                }
                else
                {
                    int[] outT = new int[] { outTemps[i] };
                    int[] flowT = new int[] { flowTemp[i] };
                    GetConvertData(outT, flowT, forTemp, climat, newDictionary, oldDictionary, oldPump);
                }

            }
        }
    }
}
