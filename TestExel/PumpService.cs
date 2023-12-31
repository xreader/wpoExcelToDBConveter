using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
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

        //метод сейчас только копирует значения и переносит их в стандарт,
        //при условии что температура на улице уже есть такая в старой моделе и температура внутри тоже есть при такой температуре на улице
        //и пока только для теплого климата
        public List<StandartPump> Test(XLWorkbook workbook, int[] outTemps, int[] flowTemp, string climat)
        {
            List<Pump> oldPumps = GetAllPumpsWithBasicTemp(workbook); // Предположим, у вас есть метод для получения данных
            List<StandartPump> standartPumps = new List<StandartPump>();
            //var pump = oldPumps[10];
            foreach(var oldPump in oldPumps)
            {
                //получаем словарь даных насоса
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
                // Новая коллекция данных
                Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
               
                //Перебор маисва с даными внешней температуры
                for (int i = 0; i < outTemps.Length; i++)
                {
                    
                    if (oldDictionary.ContainsKey(outTemps[i]))
                    {
                        //код если есть значение такой температуры н улице
                        oldDictionary.TryGetValue(outTemps[i], out List<DataPump> oldDataPump);
                        //Конвертируем значения
                        ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], climat, newDictionary);
                        
                    }
                    else
                    {
                        //Код если нет такой температуры на улице в таблице

                        //Поиск даных для такой температуры на улице когда их нет
                        var oldDataPump = FindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                        //Конвертируем значения
                        ConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], climat, newDictionary);
                    }
                }
                var standartPump = new StandartPump()
                {
                    Name = oldPump.Name,
                    Type = oldPump.Type,
                    Data = newDictionary
                };
                standartPumps.Add(standartPump);
            }

            return standartPumps;


        }
        //Создание обьекта новых даных под стандарт когда они есть в таблице
        private StandartDataPump CreateStandartDataPump(DataPump dataPump, string climat)
        {
            return new StandartDataPump
            {
                Temp = dataPump.Temp,
                Climate = climat,
                MinHC = 0,
                MidHC = dataPump.HC,
                MaxHC = dataPump.HC,
                MinCOP = 0,
                MidCOP = dataPump.COP,
                MaxCOP = dataPump.COP
            };
        }
        //Создание обьекта новых даных под стандарт когда их нет в таблице
        private StandartDataPump CreateStandartDataPumpWannOtherTemp(DataPump oldDataWithHighGrad, DataPump oldDataWithLowGrad, int outTemp, string climat)
        {
            var dif = oldDataWithHighGrad.Temp - outTemp;
            return new StandartDataPump
            {
                Temp = outTemp,
                Climate = climat,
                MinHC = 0,
                MidHC = Math.Round(oldDataWithHighGrad.HC - dif * (oldDataWithHighGrad.HC - oldDataWithLowGrad.HC) / 20, 2),
                MaxHC = Math.Round(oldDataWithHighGrad.HC - dif * (oldDataWithHighGrad.HC - oldDataWithLowGrad.HC) / 20, 2),
                MinCOP = 0,
                MidCOP = Math.Round(oldDataWithHighGrad.COP - dif * (oldDataWithHighGrad.COP - oldDataWithLowGrad.COP) / 20, 2),
                MaxCOP = Math.Round(oldDataWithHighGrad.COP - dif * (oldDataWithHighGrad.COP - oldDataWithLowGrad.COP) / 20, 2)
            };
        }
        //Расчитывает даные для насоса когда у нас нед даных при такой температуре на улице
        private List<DataPump> FindDataWhenNoDatainThisOutTemp(Dictionary<int, List<DataPump>> oldDictionary, int outTemp)
        {
            var maxKeyBeforeTarget = oldDictionary.Keys.Where(key => key < outTemp).DefaultIfEmpty(int.MinValue).Max();
            var minKeyBeforeTarget = oldDictionary.Keys.Where(key => key > outTemp).DefaultIfEmpty(int.MaxValue).Min();

            if (!oldDictionary.TryGetValue(maxKeyBeforeTarget, out var minDataPump) ||
                !oldDictionary.TryGetValue(minKeyBeforeTarget, out var maxDataPump))
            {
                return new List<DataPump>(); // Или другой вариант обработки, если ключи не найдены
            }
            //Расчет даных для насоса при условии что такой температуры на улице не было
            var oldDataPump = minDataPump.Zip(maxDataPump, (minElement, maxElement) => new DataPump
            {
                Temp = minElement.Temp,
                HC = Math.Round(minElement.HC + ((outTemp - maxKeyBeforeTarget) * (maxElement.HC - minElement.HC) / (maxKeyBeforeTarget - minKeyBeforeTarget)), 2),
                COP = Math.Round(minElement.COP + ((outTemp - maxKeyBeforeTarget) * (maxElement.COP - minElement.COP) / (maxKeyBeforeTarget - minKeyBeforeTarget)), 2)
            }).ToList();

            return oldDataPump;
        }

        private void ConvertDataInStandart(List<DataPump> oldDataPump, int flowTemp, int outTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary)
        {
            var standartDataPump = new StandartDataPump();

            if (oldDataPump.Any(x => x.Temp == flowTemp))
            {
                var oldDataForThisOutAndFlowTemp = oldDataPump.FirstOrDefault(x => x.Temp == flowTemp);
                standartDataPump = CreateStandartDataPump(oldDataForThisOutAndFlowTemp, climat);
            }
            else if (oldDataPump.Any(x => x.Temp == 55) && oldDataPump.Any(x => x.Temp == 35))
            {
                var oldDataWithHighGrad = oldDataPump.FirstOrDefault(x => x.Temp == 55);
                var oldDataWithLowGrad = oldDataPump.FirstOrDefault(x => x.Temp == 35);
                standartDataPump = CreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp, climat);
            }

            if (standartDataPump != null)
            {
                if (!newDictionary.TryGetValue(outTemp, out var newStandartDataPump))
                {
                    newStandartDataPump = new List<StandartDataPump>();
                    newDictionary.Add(outTemp, newStandartDataPump);
                }

                newStandartDataPump.Add(standartDataPump);
            }
        }

        public List<Pump> GetAllPumpsFromExel(XLWorkbook workbook)
        {
            List<Pump> pumps = new List<Pump>();
            var sheetsCount = workbook.Worksheets.Count;
            for (int i = 1; i <= sheetsCount; i++)
            {
                var worksheet = workbook.Worksheet(i);
                var pump = new Pump(worksheet);
                pump.GetNamePumpInExel("H1");
                pump.GetTypePumpInExel("A1");
                pump.GetData(6,"A","B","AC");
                
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOPAndP(pumps);
            return pumps;
        }

        //Getting all pumps but only with 30 and 50 degree data
        public List<Pump> GetAllPumpsWithBasicTemp(XLWorkbook workbook)
        {
            var pumps = GetAllPumpsFromExel(workbook);
            var filteredData = pumps
                .Select(pump => new Pump
                {
                    Name = pump.Name,
                    Type = pump.Type,
                    Data = pump.Data
                        .ToDictionary(
                            pair => pair.Key,
                            pair => pair.Value.Where(data => data.Temp == 35 || data.Temp == 55).ToList()
                        )
                }).ToList();
            
            return filteredData;
        }
        public static void RoundCOPAndP(List<Pump> pumps)
        {
            foreach (var pump in pumps)
            {
                foreach (var dataPair in pump.Data)
                {
                    foreach (var data in dataPair.Value)
                    {
                        data.COP = Math.Round(data.COP * 100) / 100;
                        data.HC = Math.Round(data.HC * 100) / 100;
                    }
                }
            }
        }

    }


}
