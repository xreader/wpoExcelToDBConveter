using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    Console.WriteLine(worksheet.Name);
                    var pump = new Pump(worksheet);
                    var cellsWithNamePump = GetCellWithNamePump(worksheet);
                    foreach(var cellWithNamePump in cellsWithNamePump)
                    {
                        var cellWithDataPump = GetCellWithDataForPump(worksheet, cellWithNamePump);
                        foreach (var address in cellWithDataPump)
                        {
                            Console.WriteLine(address);
                        }
                    }                                       

                }

                //pump.Name = worksheet.Name;
                //pump.GetData(2, "B", "C", "I", 35);
                //pump.GetData(13, "B", "C", "I", 55);
                //if (pump != null && pump.Name != "")
                //    pumps.Add(pump);

            }
            //RoundCOPAndP(pumps);
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
            //Получаем список с ячейками где есть название насоса

            // Select cells by range
            var range = _sheet.Range(cellWithNamePump.Letter+cellWithNamePump.Num + ":" + cellWithNamePump.Letter + 300);
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
    }
}
