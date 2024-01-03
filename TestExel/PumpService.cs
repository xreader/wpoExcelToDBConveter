using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TestExel.StandartModels;

namespace TestExel
{
    internal class PumpService
    {
        private readonly XLWorkbook workbook;

        public PumpService(string excelFilePath)
        {
            workbook = new XLWorkbook(excelFilePath);
        }
        public List<StandartPump> CreateListStandartPumps() => new List<StandartPump>();
        //the method now only copies the values and transfers them to the standard,
        //provided that the temperature outside is already the same as in the old model and the temperature inside is also at the same temperature outside
        //and so far only for warm climates
        
        public List<StandartPump> GetDataInListStandartPumps(List<StandartPump> standartPumps, int[] outTemps, int[] flowTemps, string climat)
        {
            List<Pump> oldPumps = GetAllPumpsWithBasicTemp();
            //var oldPump = oldPumps[16];

            foreach(var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
               
                var result = AddMinOutTempWhenPumpWorked(oldPump, outTemps, flowTemps);
                var outTemps2 = result.Item1;
                var flowTemps2 = result.Item2;
                if (standartPumps.Any(x=>x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x=>x.Name == oldPump.Name).Data;
                    GetConvertData(outTemps2, flowTemps2, climat, newDictionary, oldDictionary);
                    
                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    GetConvertData(outTemps2, flowTemps2, climat, newDictionary, oldDictionary);
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
        //Тестовый метод добавляет минимальные температры когда рабоатет насос, нужно модифицировать
        private (int[], int[]) AddMinOutTempWhenPumpWorked(Pump pump, int[] outTepms, int[] flowTemps)
        {
            Dictionary<int, List<DataPump>> data = pump.Data;
            int minOutTemp = outTepms.Min();
            var minKeyBeforeTargetFor35 = data.Keys
                .Where(key => key < minOutTemp)
                .Where(key => data.TryGetValue(key, out var dataList) && dataList.Any(item => item.Temp == 35))
                .DefaultIfEmpty()
                .Min();
            var minKeyBeforeTargetFor55 = data.Keys
                .Where(key => key < minOutTemp)
                .Where(key => data.TryGetValue(key, out var dataList) && dataList.Any(item => item.Temp == 55))
                .DefaultIfEmpty()
                .Min();
            if (minKeyBeforeTargetFor35 != default(int))
            {
                outTepms = outTepms.Concat(new[] { minKeyBeforeTargetFor35}).ToArray();
                flowTemps = flowTemps.Concat(new[] { 35}).ToArray();
            }
            if (minKeyBeforeTargetFor55 != default(int))
            {
                outTepms = outTepms.Concat(new[] { minKeyBeforeTargetFor55 }).ToArray();
                flowTemps = flowTemps.Concat(new[] { 55 }).ToArray();
            }
            return (outTepms, flowTemps);
        }
        //Creating a new data object according to the standard when it is in the table
        private StandartDataPump CreateStandartDataPump(DataPump dataPump, string climat)
        {
            return new StandartDataPump
            {
                Temp = dataPump.Temp,
                Climate = climat,
                MinHC = 0,
                MidHC = dataPump.MidHC,
                MaxHC = dataPump.MidHC,
                MinCOP = 0,
                MidCOP = dataPump.MidCOP < 1 ? 1 : dataPump.MidCOP,
                MaxCOP = dataPump.MidCOP < 1 ? 1 : dataPump.MidCOP
            };
        }
        //Creating a new data object according to the standard when it is not in the table
        private StandartDataPump CreateStandartDataPumpWannOtherTemp(DataPump oldDataWithHighGrad, DataPump oldDataWithLowGrad, int outTemp, string climat)
        {
            var dif = oldDataWithHighGrad.Temp - outTemp;
            var midCop = Math.Round(oldDataWithHighGrad.MidCOP - dif * (oldDataWithHighGrad.MidCOP - oldDataWithLowGrad.MidCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            return new StandartDataPump
            {
                Temp = outTemp,
                Climate = climat,
                MinHC = 0,
                MidHC = Math.Round(oldDataWithHighGrad.MidHC - dif * (oldDataWithHighGrad.MidHC - oldDataWithLowGrad.MidHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                MaxHC = Math.Round(oldDataWithHighGrad.MidHC - dif * (oldDataWithHighGrad.MidHC - oldDataWithLowGrad.MidHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                MinCOP = 0,
                MidCOP = midCop < 1 ? 1 : midCop,
                MaxCOP = midCop < 1 ? 1 : midCop
            };
        }
        //Calculates data for the pump when we do not have data at this temperature outside
        private List<DataPump> FindDataWhenNoDatainThisOutTemp(Dictionary<int, List<DataPump>> oldDictionary, int outTemp)
        {
            var maxKeyBeforeTarget = oldDictionary.Keys.Where(key => key < outTemp).DefaultIfEmpty(int.MinValue).Max();
            var minKeyBeforeTarget = oldDictionary.Keys.Where(key => key > outTemp).DefaultIfEmpty(int.MaxValue).Min();

            if (!oldDictionary.TryGetValue(maxKeyBeforeTarget, out var minDataPump) ||
                !oldDictionary.TryGetValue(minKeyBeforeTarget, out var maxDataPump))
            {
                return new List<DataPump>(); 
            }
            //Calculation of data for the pump, provided that there was no such temperature outside
            var oldDataPump = minDataPump.Zip(maxDataPump, (minElement, maxElement) => new DataPump
            {
                Temp = minElement.Temp,
                MidHC = Math.Round(minElement.MidHC + ((outTemp - maxKeyBeforeTarget) * (maxElement.MidHC - minElement.MidHC) / (maxKeyBeforeTarget - minKeyBeforeTarget)), 2),
                MidCOP = Math.Round(minElement.MidCOP + ((outTemp - maxKeyBeforeTarget) * (maxElement.MidCOP - minElement.MidCOP) / (maxKeyBeforeTarget - minKeyBeforeTarget)), 2)
            }).ToList();

            return oldDataPump;
        }
        //Convert the data
        private void ConvertDataInStandart(List<DataPump> oldDataPump, int flowTemp, int outTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary)
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
                var maxKeyBeforeTarget = oldDataPump
                        .Where(x => x.Temp < flowTemp)
                        .Select(x => x.Temp)
                        .DefaultIfEmpty()
                        .Max();
                var minKeyBeforeTarget = oldDataPump
                       .Where(x => x.Temp > flowTemp)
                       .Select(x => x.Temp)
                       .DefaultIfEmpty()
                       .Min();
                if(maxKeyBeforeTarget != (int)default && minKeyBeforeTarget != (int)default)
                {
                    var oldDataWithHighGrad = oldDataPump.FirstOrDefault(x => x.Temp == minKeyBeforeTarget);
                    var oldDataWithLowGrad = oldDataPump.FirstOrDefault(x => x.Temp == maxKeyBeforeTarget);

                    if (maxKeyBeforeTarget != null && minKeyBeforeTarget != null)
                    {
                        standartDataPump = CreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp, climat);
                        standartDataPumpChanged = true;
                    }
                }                
            }
            
            
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
        //Get already converted data
        private void GetConvertData(int[] outTemps, int[] flowTemp,string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Dictionary<int, List<DataPump>> oldDictionary)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {

                if (oldDictionary.ContainsKey(outTemps[i]))
                {
                    //Сode if there is a value for this temperature outside
                    oldDictionary.TryGetValue(outTemps[i], out List<DataPump> oldDataPump);
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], climat, newDictionary);

                }
                else
                {
                    //Code if there is no such temperature outside in the table
                    //Search for data for a temperature outside when there is none
                    var oldDataPump = FindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                    //Convert values
                    ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], climat, newDictionary);
                }
            }
        }
        //Get all pumps from Exel
        public List<Pump> GetAllPumpsFromExel()
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var pump = new Pump(worksheet);
                pump.Name = worksheet.Name;
                pump.GetData(2,"B","C","H",35);
                pump.GetData(15, "B", "C", "H", 55);
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }
        //Getting all pumps but only with 30 and 50 degree data
        private List<Pump> GetAllPumpsWithBasicTemp()
        {
            var pumps = GetAllPumpsFromExel();
            var filteredData = pumps
                .Select(pump => new Pump
                {
                    Name = pump.Name,
                    Data = pump.Data
                        .ToDictionary(
                            pair => pair.Key,
                            pair => pair.Value.Where(data => data.Temp == 35 || data.Temp == 55).ToList()
                        )
                }).ToList();
            
            return filteredData;
        }
        //Data rounding
        private static void RoundCOPAndP(List<Pump> pumps)
        {
            foreach (var pump in pumps)
            {
                foreach (var dataPair in pump.Data)
                {
                    foreach (var data in dataPair.Value)
                    {
                        data.MinCOP = Math.Round(data.MinCOP * 100) / 100;
                        data.MidCOP = Math.Round(data.MidCOP * 100) / 100;
                        data.MaxCOP = Math.Round(data.MaxCOP * 100) / 100;
                        
                        data.MinHC = Math.Round(data.MinHC * 100) / 100;
                        data.MidHC = Math.Round(data.MidHC * 100) / 100;
                        data.MaxHC = Math.Round(data.MaxHC * 100) / 100;
                    }
                }
            }
        }

    }


}
