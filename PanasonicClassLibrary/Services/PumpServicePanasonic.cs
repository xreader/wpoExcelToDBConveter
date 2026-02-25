using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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

        // Helper: Safe Int32 conversion with detailed error message
        private int SafeToInt32(string value, string sheetName, string cellRef, string context = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException(
                    $"Leere Zelle gefunden wo ein Integer-Wert erwartet wird.\n" +
                    $"  Sheet: '{sheetName}'\n" +
                    $"  Zelle: {cellRef}\n" +
                    $"  Kontext: {context}\n" +
                    $"  Bitte Excel-Datei prüfen und korrigieren.");
            if (!int.TryParse(value, out int result))
                throw new FormatException(
                    $"Ungültiger Integer-Wert: '{value}'\n" +
                    $"  Sheet: '{sheetName}'\n" +
                    $"  Zelle: {cellRef}\n" +
                    $"  Kontext: {context}\n" +
                    $"  Bitte Excel-Datei prüfen und korrigieren.");
            return result;
        }

        // Helper: Safe Double conversion with detailed error message
        private double SafeToDouble(string value, string sheetName, string cellRef, string context = "")
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException(
                    $"Leere Zelle gefunden wo ein Zahlenwert erwartet wird.\n" +
                    $"  Sheet: '{sheetName}'\n" +
                    $"  Zelle: {cellRef}\n" +
                    $"  Kontext: {context}\n" +
                    $"  Bitte Excel-Datei prüfen und korrigieren.");
            if (!double.TryParse(value, out double result))
                throw new FormatException(
                    $"Ungültiger Zahlenwert: '{value}'\n" +
                    $"  Sheet: '{sheetName}'\n" +
                    $"  Zelle: {cellRef}\n" +
                    $"  Kontext: {context}\n" +
                    $"  Bitte Excel-Datei prüfen und korrigieren.");
            return result;
        }
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                // Skip template sheets
                if (worksheet.Name.Contains("TEMPLATE", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine($"  Sheet '{worksheet.Name}' übersprungen (Template).");
                    continue;
                }
                var firstCellsWithOutTemp = GetFirstCellsWithOutTemp("C", 4, worksheet);
                foreach (var firstCellWithOutTemp in firstCellsWithOutTemp)
                {
                    try
                    {
                        var pump = new Pump(worksheet);
                        pump.Name = GetNamePump(firstCellWithOutTemp, worksheet);

                        // Read backup heater capacity from column X (same row as pump name)
                        // Name might be in same row or one row above temp data
                        int nameRow = firstCellWithOutTemp.Num;
                        var nameCheck = worksheet.Cell(nameRow, 1); // Column A
                        if (nameCheck.GetString() == "" && nameRow > 1)
                            nameRow = nameRow - 1;

                        var heaterCell = worksheet.Cell(nameRow, 24); // Column X = 24
                        if (heaterCell != null && !heaterCell.IsEmpty())
                        {
                            var heaterStr = heaterCell.GetString();
                            if (double.TryParse(heaterStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double heaterKW))
                                pump.BackupHeaterKW = heaterKW;
                        }

                        GetData35ForPump(worksheet, firstCellWithOutTemp, pump);
                        GetData55ForPump(worksheet, firstCellWithOutTemp, pump);
                        if (pump != null && pump.Name != "")
                            pumps.Add(pump);
                    }
                    catch (FormatException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\n*** FEHLER beim Einlesen von Sheet '{worksheet.Name}' ***");
                        Console.WriteLine(ex.Message);
                        Console.ResetColor();
                        Console.WriteLine($"\nPump-Name: {GetNamePump(firstCellWithOutTemp, worksheet)}");
                        Console.WriteLine($"StartZelle: {firstCellWithOutTemp.Letter}{firstCellWithOutTemp.Num}");
                        Console.WriteLine("\nBitte Excel-Datei korrigieren und erneut versuchen.");
                        throw; // Re-throw damit der Aufrufer (LogicPanasonic) den Retry machen kann
                    }
                }
            }
            RoundCOPAndP(pumps);
            return pumps;
        }

        //New
        private List<Cell> GetFirstCellsWithOutTemp(string letterFirstCell, int numFirtstCell, IXLWorksheet worksheet)
        {
            var cells = new List<Cell>();
            bool checkout = true;
            var firstCell = worksheet.Cell(letterFirstCell + numFirtstCell);
            if (firstCell.GetString() != "")
                cells.Add(new Cell(Letter: firstCell.Address.ColumnLetter, Num: firstCell.Address.RowNumber, Data: firstCell.GetString()));
            int emptyCount = 0;
            while (checkout)
            {
                firstCell = worksheet.Cell(letterFirstCell + numFirtstCell);
                var secondCell = worksheet.Cell(letterFirstCell + (numFirtstCell + 1));

                if (firstCell.GetString() == "")
                {
                    emptyCount++;
                    // Stop after 3 consecutive empty cells in column C
                    if (emptyCount >= 3)
                        checkout = false;
                }
                else
                {
                    emptyCount = 0;
                }

                if (firstCell.GetString() == "" && secondCell.GetString() != "")
                {
                    cells.Add(new Cell(Letter: secondCell.Address.ColumnLetter, Num: secondCell.Address.RowNumber, Data: secondCell.GetString()));
                }


                numFirtstCell++;
            }



            return cells;
        }
        private string GetNamePump(Cell cellsWithOutTemps, IXLWorksheet worksheet)
        {
            string namePump = "";

            if (cellsWithOutTemps != null)
            {
                int startColumnIndex = XLHelper.GetColumnNumberFromLetter(cellsWithOutTemps.Letter);
                var firstName = worksheet.Cell(cellsWithOutTemps.Num, startColumnIndex - 2);
                var secondName = worksheet.Cell(cellsWithOutTemps.Num, startColumnIndex - 1);

                // If name not in same row as temp data, check one row above
                // (some sheets have pump name in a separate row above the temperature data)
                if (firstName.GetString() == "" && secondName.GetString() == "" && cellsWithOutTemps.Num > 1)
                {
                    firstName = worksheet.Cell(cellsWithOutTemps.Num - 1, startColumnIndex - 2);
                    secondName = worksheet.Cell(cellsWithOutTemps.Num - 1, startColumnIndex - 1);
                }

                if (firstName.GetString() == "")
                    namePump = secondName.GetString();
                else
                    namePump = firstName.GetString() + " + " + secondName.GetString();
            }



            return namePump;
        }
        private void GetData35ForPump(IXLWorksheet _sheet, Cell cellWithOutTemp, Pump pump)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            bool chekout = true;
            var num = cellWithOutTemp.Num;
            var sheetName = _sheet.Name;
            while (chekout)
            {
                var cell = _sheet.Cell(cellWithOutTemp.Letter + num);
                var cellRef = cellWithOutTemp.Letter + num;
                var cellString = cell.GetString();

                int outTemp = SafeToInt32(cellString, sheetName, cellRef, "Außentemperatur (35°C Block)");
                pump.Data.TryGetValue(outTemp, out var datasPump);
                if (datasPump == null)
                    datasPump = new List<DataPump>();
                var midHCLetter = "Z";
                var maxHCLetter = "G";
                var midCOPLetter = "AA";
                var maxCOPLetter = "K";
                var midHC = _sheet.Cell(num, midHCLetter).Value.ToString();
                var midCOP = _sheet.Cell(num, midCOPLetter).Value.ToString();
                var maxHC = _sheet.Cell(num, maxHCLetter).Value.ToString();
                var maxCOP = _sheet.Cell(num, maxCOPLetter).Value.ToString();

                var maxVLString = _sheet.Cell("M" + num).GetString();
                var dataPump = new DataPump()
                {
                    MaxVorlauftemperatur = string.IsNullOrWhiteSpace(maxVLString) ? 0 : SafeToInt32(maxVLString, sheetName, "M" + num, "Max. Vorlauftemperatur (35°C Block)"),
                    Temp = 35,
                    MinHC = 0,
                    MinCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MidHC = midHC == "" || midHC == "-" ? 0 : Convert.ToDouble(midHC),
                    MidCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MaxHC = maxHC == "" || maxHC == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxHCLetter, cellWithOutTemp.Letter, outTemp) : Convert.ToDouble(maxHC),
                    MaxCOP = maxCOP == "" || maxCOP == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxCOPLetter, cellWithOutTemp.Letter, outTemp) : Convert.ToDouble(maxCOP)

                };
                if (dataPump.MaxCOP < 1)
                    dataPump.MaxCOP = 1;
                if (dataPump.MidHC > 0)
                    dataPump.MinHC = dataPump.MaxHC / 2 < 1 ? 1.1 : dataPump.MaxHC / 2;
                datasPump.Add(dataPump);


                if (!pump.Data.Any(x => x.Key == outTemp))
                    pump.Data.Add(outTemp, datasPump);


                num++;
                if (_sheet.Cell(cellWithOutTemp.Letter + num).GetString() == "")
                    chekout = false;
            }

        }
        private void GetData55ForPump(IXLWorksheet _sheet, Cell cellWithOutTemp, Pump pump)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            bool chekout = true;
            var num = cellWithOutTemp.Num;
            var sheetName = _sheet.Name;
            while (chekout)
            {
                var cell = _sheet.Cell(cellWithOutTemp.Letter + num);
                var cellRef = cellWithOutTemp.Letter + num;
                var cellString = cell.GetString();

                int outTemp = SafeToInt32(cellString, sheetName, cellRef, "Außentemperatur (55°C Block)");
                pump.Data.TryGetValue(outTemp, out var datasPump);
                if (datasPump == null)
                    datasPump = new List<DataPump>();
                var midHCLetter = "AB";
                var maxHCLetter = "S";
                var midCOPLetter = "AC";
                var maxCOPLetter = "W";

                var midHC = _sheet.Cell(num, midHCLetter).Value.ToString();
                var midCOP = _sheet.Cell(num, midCOPLetter).Value.ToString();
                var maxHC = _sheet.Cell(num, maxHCLetter).Value.ToString();
                var maxCOP = _sheet.Cell(num, maxCOPLetter).Value.ToString();

                var maxVLString55 = _sheet.Cell("M" + num).GetString();
                var dataPump = new DataPump()
                {
                    MaxVorlauftemperatur = string.IsNullOrWhiteSpace(maxVLString55) ? 0 : SafeToInt32(maxVLString55, sheetName, "M" + num, "Max. Vorlauftemperatur (55°C Block)"),
                    Temp = 55,
                    MinHC = 0,
                    MinCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MidHC = midHC == "" || midHC == "-" ? 0 : Convert.ToDouble(midHC),
                    MidCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MaxHC = maxHC == "" || maxHC == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxHCLetter, cellWithOutTemp.Letter, outTemp) : Convert.ToDouble(maxHC),
                    MaxCOP = maxCOP == "" || maxCOP == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxCOPLetter, cellWithOutTemp.Letter, outTemp) : Convert.ToDouble(maxCOP)
                };

                if (dataPump.MaxCOP < 1)
                    dataPump.MaxCOP = 1;
                if (dataPump.MidHC > 0)
                    dataPump.MinHC = dataPump.MaxHC / 2 < 1 ? 1.1 : dataPump.MaxHC / 2;

                datasPump.Add(dataPump);



                if (!pump.Data.Any(x => x.Key == outTemp))
                    pump.Data.Add(outTemp, datasPump);


                num++;
                if (_sheet.Cell(cellWithOutTemp.Letter + num).GetString() == "")
                    chekout = false;
            }

        }

        private double GetMaxDataWhenDataNull(IXLWorksheet worksheet, int currentNum, string letterWithData, string letterWithOutTemp, int currentOutTemp)
        {
            var sheetName = worksheet.Name;
            var lowDataString = worksheet.Cell(currentNum - 1, letterWithData).Value.ToString();
            var highDataString = worksheet.Cell(currentNum + 1, letterWithData).Value.ToString();
            var lowOutTempString = worksheet.Cell(currentNum - 1, letterWithOutTemp).Value.ToString();
            var highOutTempString = worksheet.Cell(currentNum + 1, letterWithOutTemp).Value.ToString();
            if (lowDataString.Contains("Max") || lowDataString == "")
            {
                lowDataString = highDataString;
                highDataString = worksheet.Cell(currentNum + 2, letterWithData).Value.ToString();
                lowOutTempString = highOutTempString;
                highOutTempString = worksheet.Cell(currentNum + 2, letterWithOutTemp).Value.ToString();
            }
            if (highDataString == "")
            {
                highDataString = lowDataString;
                lowDataString = worksheet.Cell(currentNum - 2, letterWithData).Value.ToString();
                highOutTempString = lowOutTempString;
                lowOutTempString = worksheet.Cell(currentNum - 2, letterWithOutTemp).Value.ToString();
            }

            // Wenn keine gültigen Nachbarn für Interpolation gefunden → 0 zurückgeben (TOL-Bereich ohne Leistungsdaten)
            if (string.IsNullOrWhiteSpace(lowDataString) || string.IsNullOrWhiteSpace(highDataString) ||
                string.IsNullOrWhiteSpace(lowOutTempString) || string.IsNullOrWhiteSpace(highOutTempString) ||
                lowDataString == "-" || highDataString == "-")
            {
                return 0;
            }

            double lowData = SafeToDouble(lowDataString, sheetName, $"{letterWithData}{currentNum - 1}", $"Interpolation lowData für Außentemp {currentOutTemp}");
            double highData = SafeToDouble(highDataString, sheetName, $"{letterWithData}{currentNum + 1}", $"Interpolation highData für Außentemp {currentOutTemp}");
            int lowOutTemp = SafeToInt32(lowOutTempString, sheetName, $"{letterWithOutTemp}{currentNum - 1}", $"Interpolation lowOutTemp für Außentemp {currentOutTemp}");
            int highOutTemp = SafeToInt32(highOutTempString, sheetName, $"{letterWithOutTemp}{currentNum + 1}", $"Interpolation highOutTemp für Außentemp {currentOutTemp}");
            return lowData + ((highData - lowData) / (highOutTemp - lowOutTemp)) * (currentOutTemp - lowOutTemp);


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
                                .DefaultIfEmpty()
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

        //Get already converted data
        protected override void GetConvertData(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Dictionary<int, List<DataPump>> oldDictionary, Pump oldPump)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {

                if (oldDictionary.ContainsKey(outTemps[i]))
                {
                    //Сode if there is a value for this temperature outside
                    oldDictionary.TryGetValue(outTemps[i], out List<DataPump> oldDataPump);
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);

                }
                else
                {
                    //Code if there is no such temperature outside in the table
                    //Search for data for a temperature outside when there is none
                    var oldDataPump = FindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary, oldPump);
                }
            }
        }
    }
}