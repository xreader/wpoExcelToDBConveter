using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;
using TestExel.StandartModels;

namespace RemehaClassLibrary.Services
{
    public class PumpServiceRemeha : PumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);

        public PumpServiceRemeha(string excelFilePath)
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
                
                var namePump = worksheet.Name;


                var cellWithNamePump = new Cell(Letter: "C", Num: 1, Data: "");
                var pump = new Pump(worksheet);
                //The logic has been changed - we get all the records where there are records
                var cellWithDataPump = GetCellWithDataForPump(worksheet, cellWithNamePump);
                if (cellWithDataPump.Count >= 2)
                {
                    var countTempOutFor35Grad = cellWithDataPump.Last(x => x.Data == "35").Num - cellWithDataPump.First(x => x.Data == "35").Num + 1;
                    var countTempOutFor55Grad = cellWithDataPump.Last(x => x.Data == "55").Num - cellWithDataPump.First(x => x.Data == "55").Num + 1;


                    pump.Name = namePump;
                    var cellWith35GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "35");
                    if (cellWith35GradData != null)
                        GetData(cellWith35GradData, 35, pump, countTempOutFor35Grad, worksheet);
                    var cellWith55GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "55");
                    if (cellWith55GradData != null)
                        GetData(cellWith55GradData, 55, pump, countTempOutFor55Grad, worksheet);
                    if (pump != null && pump.Name != "" && pump.Name != "Vorlage")
                        pumps.Add(pump);
                }
                

            }
            RoundCOPAndP(pumps);
            return pumps;
        }

        public List<Cell> GetCellWithDataForPump(IXLWorksheet _sheet, Cell cellWithNamePump)
        {

            // Select cells by range
            var range = _sheet.Range(cellWithNamePump.Letter + (cellWithNamePump.Num + 1) + ":" + cellWithNamePump.Letter + 300);
            // Список для хранения адресов ячеек с заданным содержимым
            List<Cell> cellAddressesWithData = new List<Cell>();
            // Проходим по каждой ячейке в диапазоне
            foreach (var cell in range.CellsUsed())
            {
                if (cell.GetString() != "tVL")
                    // Добавляем адрес ячейки в список
                    cellAddressesWithData.Add(new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString()));

            }
           
            return cellAddressesWithData;
        }

        public void GetData(Cell adressFirstCell, int tempWaterIn, Pump pump, int countTempOut, IXLWorksheet _sheet)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            // Номер строки, содержащей данные
            int rowNumber = adressFirstCell.Num;

            // Буква столбца, с которого начинаются данные
            string startColumnLetter = "A";

            // Получаем индекс столбца по его букве
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;


            for (int i = 0; i < countTempOut / 3; i++)
            {

                var cellDataListMin = GetDataInRow(_sheet, rowNumber, startColumnIndex);
                var cellDataListMid = GetDataInRow(_sheet, rowNumber+1, startColumnIndex);
                var cellDataListMax = GetDataInRow(_sheet, rowNumber+2, startColumnIndex);
                if (!cellDataListMin.Skip(2).Take(2).All(item => item == "/") || !cellDataListMid.Skip(2).Take(2).All(item => item == "/") || !cellDataListMax.Skip(2).Take(2).All(item => item == "/"))
                {
                    pump.Data.TryGetValue(Convert.ToInt32(cellDataListMin[0]), out var datasPump);
                    if (datasPump == null)
                        datasPump = new List<DataPump>();
                    ReplaceSlashWithZero(cellDataListMin);
                    ReplaceSlashWithZero(cellDataListMid);
                    ReplaceSlashWithZero(cellDataListMax);
                    datasPump.Add(new DataPump
                    {
                        Temp = tempWaterIn,
                        MinHC = Convert.ToDouble(cellDataListMin[2]),
                        MidHC = Convert.ToDouble(cellDataListMid[2]),
                        MaxHC = Convert.ToDouble(cellDataListMax[2]),
                        MinCOP = Convert.ToDouble(cellDataListMin[3]),
                        MidCOP = Convert.ToDouble(cellDataListMid[3]),
                        MaxCOP = Convert.ToDouble(cellDataListMax[3]),
                        MaxVorlauftemperatur = Convert.ToInt32(cellDataListMax[4])
                    });



                    if (!pump.Data.Any(x => x.Key == Convert.ToInt32(cellDataListMin[0])))
                        pump.Data.Add(Convert.ToInt32(cellDataListMin[0]), datasPump);
                }

                rowNumber+=3;
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

        
        //Replace / with 0
        void ReplaceSlashWithZero(List<string> cellDataList)
        {
            if (cellDataList.Contains("/"))
            {
                // Замена всех вхождений "/" на "0" в каждой строке списка
                for (int j = 1; j < cellDataList.Count; j++)
                {
                    cellDataList[j] = cellDataList[j].Replace("/", "0");
                }
            }
        }

        public List<StandartPump> GetDataInListStandartPumpsForLuftRemeha(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
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
    }
}
