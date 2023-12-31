using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
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
        public List<StandartPump> Test(XLWorkbook workbook, int[] outTemps, int[] flowTemp)
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
                    StandartDataPump standartDataPump = new StandartDataPump();
                    if (oldDictionary.ContainsKey(outTemps[i]))
                    {
                        //код если есть значение такой температуры н улице
                        oldDictionary.TryGetValue(outTemps[i], out List<DataPump> oldDataPump);
                        if(oldDataPump.Any(x => x.Temp == flowTemp[i]))
                        {
                            var oldDataForThisOutAndFlowTemp = oldDataPump.FirstOrDefault(x => x.Temp == flowTemp[i]);
                            standartDataPump = CreateStandartDataPump(oldDataForThisOutAndFlowTemp);
                            if (!newDictionary.ContainsKey(outTemps[i]))
                                //если нет записи с таким ключом
                                newDictionary.Add(outTemps[i], new List<StandartDataPump> { standartDataPump });
                            else
                            {
                                //если есть запись с таким ключом
                                newDictionary.TryGetValue(outTemps[i], out List<StandartDataPump> newStandartDataPump);
                                newStandartDataPump.Add(standartDataPump);
                            }
                        }
                        else
                        {
                            //код если есть такая температра на улице в таблице но нет значения с такой температурой
                            //получаем даные при 55 градусов
                            var oldDataWithHighGrad = oldDataPump.FirstOrDefault(x => x.Temp == 55);
                            var oldDataWithLowGrad = oldDataPump.FirstOrDefault(x => x.Temp == 35);
                            standartDataPump = CreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp[i]);
                            if (!newDictionary.ContainsKey(outTemps[i]))
                                //если нет записи с таким ключом
                                newDictionary.Add(outTemps[i], new List<StandartDataPump> { standartDataPump });
                            else
                            {
                                //если есть запись с таким ключом
                                newDictionary.TryGetValue(outTemps[i], out List<StandartDataPump> newStandartDataPump);
                                newStandartDataPump.Add(standartDataPump);
                            }

                        }
                    }
                    else
                    {
                        //Код если нет такой температуры на улице в таблице
                        HandleNoDataForOutTemp();
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

        private StandartDataPump CreateStandartDataPump(DataPump dataPump)
        {
            return new StandartDataPump
            {
                Temp = dataPump.Temp,
                Climate = "Warm",
                MinHC = 0,
                MidHC = dataPump.HC,
                MaxHC = dataPump.HC,
                MinCOP = 0,
                MidCOP = dataPump.COP,
                MaxCOP = dataPump.COP
            };
        }
        private StandartDataPump CreateStandartDataPumpWannOtherTemp(DataPump oldDataWithHighGrad, DataPump oldDataWithLowGrad, int outTemp)
        {
            var dif = oldDataWithHighGrad.Temp - outTemp;
            return new StandartDataPump
            {
                Temp = outTemp,
                Climate = "Warm",
                MinHC = 0,
                MidHC = Math.Round(oldDataWithHighGrad.HC - dif * (oldDataWithHighGrad.HC - oldDataWithLowGrad.HC) / 20, 2),
                MaxHC = Math.Round(oldDataWithHighGrad.HC - dif * (oldDataWithHighGrad.HC - oldDataWithLowGrad.HC) / 20, 2),
                MinCOP = 0,
                MidCOP = Math.Round(oldDataWithHighGrad.COP - dif * (oldDataWithHighGrad.COP - oldDataWithLowGrad.COP) / 20, 2),
                MaxCOP = Math.Round(oldDataWithHighGrad.COP - dif * (oldDataWithHighGrad.COP - oldDataWithLowGrad.COP) / 20, 2)
            };
        }
        private void HandleNoDataForOutTemp()
        {
            // Код, если нет такой температуры
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
