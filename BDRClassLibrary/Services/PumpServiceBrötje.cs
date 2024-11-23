using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.Services;
using TestExel.StandartModels;

namespace BrötjeClassLibrary.Services
{
    internal class PumpServiceBrötje : PumpService
    {
        private readonly XLWorkbook workbook;
        public record Cell(string Letter, int Num, string Data);
        public record Vel(string Letter, int Num, string VelData); //Pump power percentage

        public PumpServiceBrötje(string excelFilePath)
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
                if(worksheet.Name == "Heating performance data")
                {
                    var cellsWithNamePumps = GetListCellsWithNamePumps("B", 10, worksheet);
                    var cellsWithTempsPumps = GetListCellsWithTempsPumps("E", 8, worksheet);

                    foreach (var cellWithNamePump in cellsWithNamePumps)
                    {
                        var pump = new Pump(worksheet);
                        pump.Name = cellWithNamePump.Data;

                        var cellsWithPumpPower = GetListCellsWithPower(cellWithNamePump, "C",worksheet);


                        GetDataForPump(worksheet, pump, cellsWithPumpPower, cellsWithTempsPumps);

                        GetMaxForlauftTemp(pump);

                        //Check Method
                        LeaveDataOnlyFor35AND55(pump);


                        if (pump != null && pump.Name != "")
                            pumps.Add(pump);

                    }
                }         

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        //Method to remove unnecessary data and insert data only for 35 and 55 degrees
        private void LeaveDataOnlyFor35AND55(Pump pump)
        {
            foreach (var data in pump.Data)
            {
                pump.Data.TryGetValue(data.Key, out var datasPump);
                List<short> removeIndexes = new List<short>();
                for (short i = 0; i < datasPump.Count; i++)
                {
                    if (datasPump[i].Temp != 35 && datasPump[i].Temp != 55)
                    {
                        removeIndexes.Add(i);
                        
                    }
                }
                if(datasPump.Count > 2) {
                    datasPump.RemoveRange(removeIndexes[0], removeIndexes.Count);
                }
                
            }
        }

        private List<Cell> GetListCellsWithNamePumps(string letterFirstCell, int numCellWithFirstName, IXLWorksheet worksheet)
        {
            return worksheet.Column(letterFirstCell)
                    .CellsUsed()
                    .Where(cell => cell.Address.RowNumber >= numCellWithFirstName)
                    .Select(cell => new Cell(Letter: cell.Address.ColumnLetter,
                                             Num: cell.Address.RowNumber,
                                             Data: cell.GetString()))
                    .ToList();
        }

        private List<Cell> GetListCellsWithTempsPumps(string letterFirstCell, int numCellWithFirstName, IXLWorksheet worksheet)
        {
            int startColumnIndex = XLHelper.GetColumnNumberFromLetter(letterFirstCell);

            return worksheet.Row(numCellWithFirstName)
                            .CellsUsed()
                            .Where(cell => cell.Address.ColumnNumber >= startColumnIndex)
                            .Select(cell => new Cell(Letter: cell.Address.ColumnLetter,
                                                     Num: cell.Address.RowNumber,
                                                     Data: cell.GetString()))
                            .ToList();
        
        }

        private List<Cell> GetListCellsWithPower(Cell cellWithNamePump, string latterWithData ,IXLWorksheet worksheet)
        {
            return worksheet.Column(latterWithData)
                    .CellsUsed()
                    .Where(cell => cell.Address.RowNumber >= cellWithNamePump.Num)
                    .Take(3)
                    .Select(cell => new Cell(Letter: cell.Address.ColumnLetter,
                                             Num: cell.Address.RowNumber,
                                             Data: cell.GetString()))
                    .ToList();
        }

        private void GetDataForPump(IXLWorksheet worksheet, Pump pump, List<Cell> cellsWithPumpPower, List<Cell> cellsWithTempsPumps)
        {
            if (pump.Data == null)
                pump.Data = new Dictionary<int, List<DataPump>>();
            
            var indexHC = 1;
            var indexCOP = 3;
            foreach (var cellWithTempsPumps in cellsWithTempsPumps)
            {
                var tempOutIndex = 0;
                for (int i = 0; i< cellsWithPumpPower[1].Num - cellsWithPumpPower[0].Num; i++)
                {
                    var typePower = 0;

                    var dataPump = new DataPump()
                    {
                         MaxVorlauftemperatur = 55,
                         Temp = Convert.ToInt32(cellWithTempsPumps.Data)                         
                    };

                    var outTempCurrent = worksheet.Cell(cellsWithPumpPower[0].Num + tempOutIndex, XLHelper.GetColumnNumberFromLetter(cellsWithPumpPower[0].Letter) + 1);
                    pump.Data.TryGetValue(Convert.ToInt32(outTempCurrent.GetString()), out var datasPump);
                    if (datasPump == null)
                        datasPump = new List<DataPump>();

                    foreach (var cellWithPumpPower in cellsWithPumpPower)
                    {
                        int letterColumnWithOutTemps = XLHelper.GetColumnNumberFromLetter(cellWithPumpPower.Letter) + 1;
                        var cellOutTemps = worksheet.Cell(cellWithPumpPower.Num + tempOutIndex, letterColumnWithOutTemps);


                        double HC;
                        var cellValueHC = worksheet.Cell(cellOutTemps.Address.RowNumber, cellOutTemps.Address.ColumnNumber + indexHC).CachedValue.ToString();

                        if (!double.TryParse(cellValueHC, out HC))
                        {
                            HC = 0;
                        }
                        double COP;
                        var cellValueCOP = worksheet.Cell(cellOutTemps.Address.RowNumber, cellOutTemps.Address.ColumnNumber + indexCOP).CachedValue.ToString();

                        if (!double.TryParse(cellValueCOP, out COP))
                        {
                            COP = 0;
                        }
                        

                        if(typePower == 2)
                        {
                            dataPump.MinHC = HC;
                            dataPump.MinCOP = COP;
                        }
                        if (typePower == 1)
                        {
                            dataPump.MidHC = HC;
                            dataPump.MidCOP = COP;
                        }
                        if (typePower == 0)
                        {
                            dataPump.MaxHC = HC;
                            dataPump.MaxCOP = COP;
                        }
                        typePower++;
                    }

                    tempOutIndex++;
                    datasPump.Add(dataPump);


                    if (!pump.Data.Any(x => x.Key == Convert.ToInt32(outTempCurrent.GetString())))
                        pump.Data.Add(Convert.ToInt32(outTempCurrent.GetString()), datasPump);
                }                

                indexHC += 3;
                indexCOP += 3;
            }


        }
        
        private void GetMaxForlauftTemp(Pump pump)
        {
            var maxForlaufttemperatur = 35;
            foreach(var data in pump.Data)
            {
                pump.Data.TryGetValue(data.Key, out var datasPump);

                for(int i = datasPump.Count - 1; i > 0; i--)
                {
                    var d = datasPump[i];
                    if (d.MaxCOP != 0 && d.MaxHC != 0)
                    {
                        maxForlaufttemperatur = d.Temp;
                        break;
                    }
                    else
                    {
                        datasPump.Remove(d);
                    }
                }
                foreach (var dat in datasPump)
                {
                    dat.MaxVorlauftemperatur = maxForlaufttemperatur;   
                    
                }
            }
        }


        public List<StandartPump> GetDataInListStandartPumpsForLuftBrötje(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
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

    }
}
