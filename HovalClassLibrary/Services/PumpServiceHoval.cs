using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;

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
                if (worksheet.Name.Contains("Heizen") && !worksheet.Name.Contains("Diagramme"))
                {                    
                   
                    var cellsWithNamePump = GetCellWithNamePump(worksheet);
                    foreach(var cellWithNamePump in cellsWithNamePump)
                    {
                        var pump = new Pump(worksheet);
                        var cellWithDataPump = GetCellWithDataForPump(worksheet, cellWithNamePump);

                        pump.Name = cellWithNamePump.Data.ToString();
                        var cellWith35GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "35");
                        if (cellWith35GradData != null)
                            GetData(cellWith35GradData, 35, pump,worksheet);
                        var cellWith55GradData = cellWithDataPump.FirstOrDefault(x => x.Data == "55");
                        if (cellWith55GradData != null)
                            GetData(cellWith55GradData, 55, pump, worksheet);
                        GetMaxForlauftemperatur(cellWithDataPump, pump, worksheet);
                        if (pump != null && pump.Name != "")
                            pumps.Add(pump);
                    }
                }              
            }
            RoundCOPAndP(pumps);
            return pumps;
        }

        public List<Cell> GetCellWithNamePump(IXLWorksheet _sheet)
        {
            //Получаем список с ячейками где есть название насоса
            // Select cells by range
            var range = _sheet.Range("A3:IV3");
            // Список для хранения адресов ячеек с заданным содержимым
            List<Cell> cellAddresses = new List<Cell>();
            // Проходим по каждой ячейке в диапазоне
            foreach (var cell in range.CellsUsed())
            {
                // Добавляем адрес ячейки в список
                cellAddresses.Add(new Cell(Letter: cell.Address.ColumnLetter, Num: cell.Address.RowNumber, Data: cell.GetString()));                
            }           
            return cellAddresses;
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

        public void GetData(Cell adressFirstCell,int tempWaterIn, Pump pump, IXLWorksheet _sheet)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            // Номер строки, содержащей данные
            int rowNumber = adressFirstCell.Num;

            // Буква столбца, с которого начинаются данные
            string startColumnLetter = adressFirstCell.Letter;

            // Получаем индекс столбца по его букве
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;

            
            for (int i = 0; i < 10; i++)
            {

                var cellDataList = GetDataInRow(_sheet, rowNumber, startColumnIndex);
                pump.Data.TryGetValue(Convert.ToInt32(cellDataList[0]), out var datasPump);
                if (datasPump == null)
                    datasPump = new List<DataPump>();

                datasPump.Add(new DataPump
                {
                    Temp = tempWaterIn,
                    MinHC = Convert.ToDouble(cellDataList[5]),
                    MidHC = Convert.ToDouble(cellDataList[2]),
                    MaxHC = Convert.ToDouble(cellDataList[2]),
                    MinCOP = Convert.ToDouble(cellDataList[6]),
                    MidCOP = Convert.ToDouble(cellDataList[3]),
                    MaxCOP = Convert.ToDouble(cellDataList[3]),
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

        public void GetMaxForlauftemperatur(List<Cell> adressCells,Pump pump , IXLWorksheet _sheet)
        {
            var lastCell = adressCells.FirstOrDefault(x => x.Data == "55");
;           foreach (Cell cell in adressCells.Where(x => Convert.ToInt32(x.Data) > 55))
            {
                // Номер строки, содержащей данные
                int rowNumber = cell.Num;

                // Буква столбца, с которого начинаются данные
                string startColumnLetter = cell.Letter;

                // Получаем индекс столбца по его букве
                int startColumnIndex = XLHelper.GetColumnNumberFromLetter(startColumnLetter) + 1;


                for (int i = 0; i < 10; i++)
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
                    else
                    {
                        if (cellDataList[2] == "-" || cellDataList[2] == "" || cellDataList[2] == null)
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
    }
}
