using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;
using TestExel.StandartModels;
using static HovalClassLibrary.Services.PumpServiceHoval;

namespace HovalClassLibrary.Services
{
    internal class PumpServiceHoval : PumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);

        public PumpServiceHoval(string excelFilePath)
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
                var cell = worksheet.Cell("A3");
                var cellWithNamePump = new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString());

                var pump = new Pump(worksheet);
                var cellWithDataPump = GetCellWithDataForPump(worksheet, cellWithNamePump);
                var countTempOut = cellWithDataPump[1].Num - cellWithDataPump[0].Num;
                pump.Name = cellWithNamePump.Data.ToString();
                var cellWith35GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "35");
                if (cellWith35GradData != null)
                    GetData(cellWith35GradData, 35, pump, countTempOut, worksheet);
                var cellWith55GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "55");
                if (cellWith55GradData != null)
                    GetData(cellWith55GradData, 55, pump, countTempOut, worksheet);
                GetMaxForlauftemperatur(cellWithDataPump, pump, worksheet, countTempOut);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }       
        public List<Cell> GetCellWithDataForPump(IXLWorksheet _sheet, Cell cellWithNamePump)
        {

            // Select cells by range
            var range = _sheet.Range(cellWithNamePump.Letter+(cellWithNamePump.Num+1) + ":" + cellWithNamePump.Letter + 300);
            // Список для хранения адресов ячеек с заданным содержимым
            List<Cell> cellAddresses = new List<Cell>();
            // Проходим по каждой ячейке в диапазоне
            foreach (var cell in range.CellsUsed())
            {                
                if(cell.GetString() != "tVL")
                    // Добавляем адрес ячейки в список
                    cellAddresses.Add(new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString()));

            }
            
            return cellAddresses;
        }

        public void GetData(Cell adressFirstCell,int tempWaterIn, Pump pump, int countTempOut, IXLWorksheet _sheet)
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

        public void GetMaxForlauftemperatur(List<Cell> adressCells,Pump pump , IXLWorksheet _sheet, int countTempOut)
        {
            var lastCell = adressCells.FirstOrDefault(x => x.Data == "35");
            var listWithReadyMaxVor = new List<string>();
;           foreach (Cell cell in adressCells.Where(x => Convert.ToInt32(x.Data) > 35))
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
                    if(cellDataList.Count <= 1) 
                    {
                        foreach (var data in datasPump)
                        {
                            data.MaxVorlauftemperatur = Convert.ToInt32(lastCell.Data);
                        }

                    }
                    else if(!listWithReadyMaxVor.Contains(cellDataList[0]))
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
        
        public List<StandartPump> GetDataInListStandartPumpsForLuftHoval(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
        {
            foreach (var oldPump in oldPumps)
            {
                int[] flowTemps2;
                int[] outTemps2;
                if (climat == "2" || climat == "1")
                {

                    int minKey = oldPump.Data
                                .Where(pair => pair.Value.Any(data => data.Temp == forTemp))
                                .Select(pair => pair.Key)
                                .DefaultIfEmpty() // Возвращаем значение по умолчанию (0), если нет удовлетворяющего ключа
                                .Min();
                    if (!outTemps.Contains(minKey))
                    {
                        outTemps2 = new int[] { minKey }.Concat(outTemps).ToArray();
                        flowTemps2 = new int[] { forTemp }.Concat(flowTemps).ToArray();
                    }
                    else
                    {
                        outTemps2 = outTemps;
                        flowTemps2 = flowTemps;
                    }
                   
                }
                else
                {
                    outTemps2 = outTemps;
                    flowTemps2 = flowTemps;
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

        public List<StandartPump> GetDataInListStandartPumpsHoval(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat, string typeFile)
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
                    ConvertDataInStandart(firstDataForEachKey, flowTemp[i], outTemps[i], forTemp, climat, newDictionary,oldPump);
                }
                else
                {
                    int[] outT = new int[] { outTemps[i] };
                    int[] flowT = new int[] { flowTemp[i] };
                    GetConvertData(outT, flowT, forTemp, climat, newDictionary, oldDictionary,oldPump);
                }
               
            }
        }        

    }
}
