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
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var firstCellsWithOutTemp = GetFirstCellsWithOutTemp("C",4,worksheet);
                foreach(var firstCellWithOutTemp in firstCellsWithOutTemp)
                {
                    var pump = new Pump(worksheet);
                    pump.Name = GetNamePump(firstCellWithOutTemp, worksheet);
                    

                    

                    GetData35ForPump(worksheet, firstCellWithOutTemp, pump);
                    GetData55ForPump(worksheet, firstCellWithOutTemp, pump);
                    if (pump != null && pump.Name != "")
                        pumps.Add(pump);

                }

                


            }
            RoundCOPAndP(pumps);
            return pumps;
        }

        //New
        private List<Cell> GetFirstCellsWithOutTemp (string letterFirstCell, int numFirtstCell, IXLWorksheet worksheet)
        {
            var cells = new List<Cell>();
            bool checkout = true;
            var firstCell = worksheet.Cell(letterFirstCell + numFirtstCell);
            cells.Add(new Cell(Letter: firstCell.Address.ColumnLetter, Num: firstCell.Address.RowNumber, Data: firstCell.GetString()));
            while (checkout)
            {
                firstCell = worksheet.Cell(letterFirstCell + numFirtstCell);
                var secondCell = worksheet.Cell(letterFirstCell + (numFirtstCell + 1));

                if (firstCell.GetString() == "" && secondCell.GetString() == "")
                    checkout = false;
                if(firstCell.GetString() == "" && secondCell.GetString() != "")
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
                var firstName = worksheet.Cell(cellsWithOutTemps.Num, startColumnIndex-2);
                var secondName = worksheet.Cell(cellsWithOutTemps.Num, startColumnIndex - 1);
                if (firstName.GetString() == "")
                    namePump = secondName.GetString();
                else
                    namePump = firstName.GetString() + " + " + secondName.GetString();
            }



            return namePump;
        }
        private void GetData35ForPump(IXLWorksheet _sheet,Cell cellWithOutTemp, Pump pump)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            bool chekout = true;
            var num = cellWithOutTemp.Num;
            while (chekout)
            {
                var cell = _sheet.Cell(cellWithOutTemp.Letter + num);
                
                pump.Data.TryGetValue(Convert.ToInt32(cell.GetString()), out var datasPump);
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

                var dataPump = new DataPump()
                {
                    MaxVorlauftemperatur = Convert.ToInt32(_sheet.Cell("M" + num).GetString()),
                    Temp = 35,
                    MinHC = 0,
                    MinCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MidHC = midHC == "" || midHC == "-" ? 0 : Convert.ToDouble(midHC),
                    MidCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                    MaxHC = maxHC == "" || maxHC == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxHCLetter, cellWithOutTemp.Letter, Convert.ToInt32(cell.GetString())) : Convert.ToDouble(maxHC),
                    MaxCOP = maxCOP == "" || maxCOP == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxCOPLetter, cellWithOutTemp.Letter, Convert.ToInt32(cell.GetString())) : Convert.ToDouble(maxCOP)

                };
                if(dataPump.MaxCOP < 1)
                    dataPump.MaxCOP = 1;
                if (dataPump.MidHC > 0)
                    dataPump.MinHC = dataPump.MaxHC / 2 < 1 ? 1.1 : dataPump.MaxHC / 2;
                datasPump.Add(dataPump);
                
                
                if (!pump.Data.Any(x => x.Key == Convert.ToInt32(cell.GetString())))
                    pump.Data.Add(Convert.ToInt32(cell.GetString()), datasPump);


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
            while (chekout)
            {
                var cell = _sheet.Cell(cellWithOutTemp.Letter + num);

                pump.Data.TryGetValue(Convert.ToInt32(cell.GetString()), out var datasPump);
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

               var dataPump = new DataPump()
                {
                    MaxVorlauftemperatur = Convert.ToInt32(_sheet.Cell("M" + num).GetString()),
                    Temp = 55,
                    MinHC = 0,
                   MinCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                   MidHC = midHC == "" || midHC == "-" ? 0 : Convert.ToDouble(midHC),
                   MidCOP = midCOP == "" || midCOP == "-" ? 0 : Convert.ToDouble(midCOP) > 1 ? Convert.ToDouble(midCOP) : 1,
                   MaxHC = maxHC == "" || maxHC == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxHCLetter, cellWithOutTemp.Letter, Convert.ToInt32(cell.GetString())) : Convert.ToDouble(maxHC),
                    MaxCOP = maxCOP == "" || maxCOP == "-" ? GetMaxDataWhenDataNull(_sheet, num, maxCOPLetter, cellWithOutTemp.Letter, Convert.ToInt32(cell.GetString())) : Convert.ToDouble(maxCOP)
                };

                if (dataPump.MaxCOP < 1)
                    dataPump.MaxCOP = 1;
                if (dataPump.MidHC > 0)
                    dataPump.MinHC = dataPump.MaxHC / 2 < 1 ? 1.1 : dataPump.MaxHC / 2;

                datasPump.Add(dataPump);



                if (!pump.Data.Any(x => x.Key == Convert.ToInt32(cell.GetString())))
                    pump.Data.Add(Convert.ToInt32(cell.GetString()), datasPump);


                num++;
                if (_sheet.Cell(cellWithOutTemp.Letter + num).GetString() == "")
                    chekout = false;
            }

        }

        private double GetMaxDataWhenDataNull(IXLWorksheet worksheet, int currentNum, string letterWithData, string letterWithOutTemp, int currentOutTemp )
        {
            var lowDataString = worksheet.Cell(currentNum-1, letterWithData).Value.ToString();
            var highDataString = worksheet.Cell(currentNum+1, letterWithData).Value.ToString();
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
             
            double lowData = Convert.ToDouble(lowDataString);
            double highData = Convert.ToDouble(highDataString);
            int lowOutTemp = Convert.ToInt32(lowOutTempString);
            int highOutTemp = Convert.ToInt32(highOutTempString);
            return lowData + ((highData - lowData) /  (highOutTemp - lowOutTemp))*(currentOutTemp - lowOutTemp);
            

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
        //Convert the data
        protected override void ConvertDataInStandart(List<DataPump> oldDataPump, int flowTemp, int outTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
        {
            var standartDataPump = new StandartDataPump();
            bool standartDataPumpChanged = false;
            if (oldDataPump.Any(x => x.Temp == flowTemp))
            {
                var oldDataForThisOutAndFlowTemp = oldDataPump.FirstOrDefault(x => x.Temp == flowTemp);
                standartDataPump = CreateStandartDataPump(oldDataForThisOutAndFlowTemp, climat);
                standartDataPumpChanged = true;
            }
            else
            {
                var oldDataWithHighGrad = oldDataPump.FirstOrDefault(x => x.Temp == 55);
                var oldDataWithLowGrad = oldDataPump.FirstOrDefault(x => x.Temp == 35);
                if (oldDataWithHighGrad == null && oldDataWithLowGrad != null && forTemp <= 35)
                {
                    var listWith55GradData = oldPump.Data.Where(kv => kv.Value.Any(dp => dp.Temp == 55)).Select(kv => kv.Key);
                    if (listWith55GradData.Count() >= 2)
                    {
                        var oldKeyWithLowTempOut = listWith55GradData.ElementAtOrDefault(0);
                        var oldKeyWithHighTempOut = listWith55GradData.ElementAtOrDefault(1);
                        oldPump.Data.TryGetValue(oldKeyWithLowTempOut, out List<DataPump> oldDataWithLowTempOutList);
                        oldPump.Data.TryGetValue(oldKeyWithHighTempOut, out List<DataPump> oldDataWithHighTempOutList);
                        var oldDataWithLowTempOut = oldDataWithLowTempOutList.FirstOrDefault(x => x.Temp == 55);
                        var oldDataWithHighTempOut = oldDataWithHighTempOutList.FirstOrDefault(x => x.Temp == 55);
                        oldDataWithHighGrad = new DataPump()
                        {
                            MaxVorlauftemperatur = oldDataWithLowGrad.MaxVorlauftemperatur,
                            Temp = 55,
                            MinHC = oldDataWithLowTempOut.MinHC + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MinHC - oldDataWithLowTempOut.MinHC) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut)),
                            MidHC = oldDataWithLowTempOut.MidHC + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MidHC - oldDataWithLowTempOut.MidHC) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut)),
                            MaxHC = oldDataWithLowTempOut.MaxHC + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MaxHC - oldDataWithLowTempOut.MaxHC) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut)),
                            MinCOP = oldDataWithLowTempOut.MinCOP + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MinCOP - oldDataWithLowTempOut.MinCOP) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut)),
                            MidCOP = oldDataWithLowTempOut.MidCOP + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MidCOP - oldDataWithLowTempOut.MidCOP) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut)),
                            MaxCOP = oldDataWithLowTempOut.MaxCOP + (outTemp - oldKeyWithLowTempOut) * ((oldDataWithHighTempOut.MaxCOP - oldDataWithLowTempOut.MaxCOP) / (oldKeyWithHighTempOut - oldKeyWithLowTempOut))
                        };




                    }



                }

                if (oldDataWithHighGrad != null && oldDataWithLowGrad != null)
                {
                    standartDataPump = CreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp, forTemp, climat);

                    standartDataPumpChanged = true;
                }
            }

            ZeroCheckForCOPAndHC(standartDataPump);
            //Сheck whether data has been added, if not, then there is no data and there is no need to add it
            if (standartDataPumpChanged)
            {
                if (!newDictionary.TryGetValue(outTemp, out var newStandartDataPump))
                {

                    newStandartDataPump = new List<StandartDataPump>();
                    newDictionary.Add(outTemp, newStandartDataPump);
                }

                newStandartDataPump.Add(standartDataPump);
            }
        }

        //Old
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
