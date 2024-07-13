using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.Models;
using TestExel.StandartModels;

namespace TestExel.Services
{
    public class PumpService
    {
        public List<StandartPump> CreateListStandartPumps() => new List<StandartPump>();       
        //Creating a new data object according to the standard when it is in the table
        protected StandartDataPump CreateStandartDataPump(DataPump dataPump, string climat)
        {
            return new StandartDataPump
            {
                ForTemp = dataPump.Temp,
                FlowTemp = dataPump.Temp,
                Climate = climat,
                MinHC = dataPump.MinHC,
                MidHC = dataPump.MidHC,
                MaxHC = dataPump.MaxHC,
                MinCOP = dataPump.MinCOP == 0 ? 0:dataPump.MinCOP == 0 ? 0 : dataPump.MinCOP < 1 ? 1 : dataPump.MinCOP,
                MidCOP = dataPump.MidCOP == 0 ? 0 : dataPump.MidCOP == 0 ? 0 : dataPump.MidCOP < 1 ? 1 : dataPump.MidCOP,
                MaxCOP = dataPump.MaxCOP == 0 ? 0 : dataPump.MaxCOP == 0 ? 0 : dataPump.MaxCOP < 1 ? 1 : dataPump.MaxCOP,
                MaxVorlauftemperatur = dataPump.MaxVorlauftemperatur
            };
        }
        //Creating a new data object according to the standard when it is not in the table
        protected StandartDataPump CreateStandartDataPumpWannOtherTemp(DataPump oldDataWithHighGrad, DataPump oldDataWithLowGrad, int flowTemp, int forTemp, string climat)
        {
            var dif = oldDataWithHighGrad.Temp - flowTemp;
            double minCop = 0;
            double midCop = 0;
            double maxCop = 0;
            double minHC = 0;
            double midHC = 0;
            double maxHC = 0;
            if (oldDataWithHighGrad.MinCOP != 0 && oldDataWithLowGrad.MinCOP != 0)
                minCop = Math.Round(oldDataWithHighGrad.MinCOP - dif * (oldDataWithHighGrad.MinCOP - oldDataWithLowGrad.MinCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            if (oldDataWithHighGrad.MidCOP != 0 && oldDataWithLowGrad.MidCOP != 0)
                midCop = Math.Round(oldDataWithHighGrad.MidCOP - dif * (oldDataWithHighGrad.MidCOP - oldDataWithLowGrad.MidCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            if (oldDataWithHighGrad.MaxCOP != 0 && oldDataWithLowGrad.MaxCOP != 0)
                maxCop = Math.Round(oldDataWithHighGrad.MaxCOP - dif * (oldDataWithHighGrad.MaxCOP - oldDataWithLowGrad.MaxCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            if (oldDataWithHighGrad.MinHC != 0 && oldDataWithLowGrad.MinHC != 0)
                minHC = Math.Round(oldDataWithHighGrad.MinHC - dif * (oldDataWithHighGrad.MinHC - oldDataWithLowGrad.MinHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            if (oldDataWithHighGrad.MidHC != 0 && oldDataWithLowGrad.MidHC != 0)
                midHC = Math.Round(oldDataWithHighGrad.MidHC - dif * (oldDataWithHighGrad.MidHC - oldDataWithLowGrad.MidHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            if (oldDataWithHighGrad.MaxHC != 0 && oldDataWithLowGrad.MaxHC != 0)
                maxHC = Math.Round(oldDataWithHighGrad.MaxHC - dif * (oldDataWithHighGrad.MaxHC - oldDataWithLowGrad.MaxHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            var standartDataPump = new StandartDataPump
            {
                ForTemp = forTemp,
                FlowTemp = flowTemp,
                Climate = climat,
                MinHC = minHC,
                MidHC = midHC,
                MaxHC = maxHC,
                MinCOP = minCop == 0 ? 0 : minCop < 1 ? 1 : minCop,
                MidCOP = midCop == 0 ? 0 : midCop < 1 ? 1 : midCop,
                MaxCOP = maxCop == 0 ? 0 : maxCop < 1 ? 1 : maxCop,
                MaxVorlauftemperatur = oldDataWithLowGrad.MaxVorlauftemperatur
            };
            
            return standartDataPump;

        }
        protected List<DataPump> FindDataWhenNoDatainThisOutTemp(Dictionary<int, List<DataPump>> oldDictionary, int outTemp)
        {
            // Определение ближайших ключей
            var lowerTemp = oldDictionary.Keys.Where(key => key < outTemp).DefaultIfEmpty(int.MinValue).Max();
            var highTemp = oldDictionary.Keys.Where(key => key > outTemp).DefaultIfEmpty(int.MaxValue).Min();

            // Проверка наличия ключей
            if (lowerTemp == int.MinValue || highTemp == int.MaxValue || lowerTemp == highTemp)
            {
                // Выбор первых двух ключей
                if (oldDictionary.Count < 2)
                {
                    // Если меньше двух элементов в словаре, невозможно интерполировать
                    return new List<DataPump>();
                }

                highTemp = lowerTemp;
                lowerTemp = oldDictionary.Keys.Where(key => key < highTemp).DefaultIfEmpty(int.MinValue).Max();
            }

            // Извлечение данных для найденных ключей
            if (!oldDictionary.TryGetValue(lowerTemp, out var minDataPump) ||
                !oldDictionary.TryGetValue(highTemp, out var maxDataPump))
            {
                return new List<DataPump>();
            }

            // Интерполяция данных
            var interpolatedData = new List<DataPump>();

            for (int i = 0; i < Math.Min(minDataPump.Count, maxDataPump.Count); i++)
            {
                var minElement = minDataPump[i];
                var maxElement = maxDataPump[i];

                var newDataPump = new DataPump
                {
                    Temp = maxElement.Temp,
                    MaxVorlauftemperatur = maxElement.MaxVorlauftemperatur,
                    MinHC = Interpolate(minElement.MinHC, maxElement.MinHC, lowerTemp, highTemp, outTemp),
                    MidHC = Interpolate(minElement.MidHC, maxElement.MidHC, lowerTemp, highTemp, outTemp),
                    MaxHC = Interpolate(minElement.MaxHC, maxElement.MaxHC, lowerTemp, highTemp, outTemp),
                    MinCOP = Interpolate(minElement.MinCOP, maxElement.MinCOP, lowerTemp, highTemp, outTemp),
                    MidCOP = Interpolate(minElement.MidCOP, maxElement.MidCOP, lowerTemp, highTemp, outTemp),
                    MaxCOP = Interpolate(minElement.MaxCOP, maxElement.MaxCOP, lowerTemp, highTemp, outTemp)
                };

                interpolatedData.Add(newDataPump);
            }

            return interpolatedData;
        }

        // Метод для линейной интерполяции
        private double Interpolate(double minValue, double maxValue, int minTemp, int maxTemp, int targetTemp)
        {
            var result = Math.Round(minValue + (targetTemp - minTemp) * (maxValue - minValue) / (maxTemp - minTemp), 2);
            return result < 0 ? 0 : result;
        }
        //Convert the data
        protected void ConvertDataInStandart(List<DataPump> oldDataPump, int flowTemp, int outTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Pump oldPump)
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
                if(oldDataWithHighGrad == null && oldDataWithLowGrad != null && forTemp <= 35)
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

        private void ZeroCheckForCOPAndHC(StandartDataPump standartDataPump)
        {
            if (standartDataPump != null)
            {
                if (standartDataPump.MinHC == 0 || standartDataPump.MinCOP == 0)
                {
                    standartDataPump.MinHC = 0;
                    standartDataPump.MinCOP = 0;
                }

                if (standartDataPump.MidHC == 0 || standartDataPump.MidCOP == 0)
                {
                    standartDataPump.MidHC = 0;
                    standartDataPump.MidCOP = 0;
                }
  
                if (standartDataPump.MaxHC == 0 || standartDataPump.MaxCOP == 0)
                {
                    standartDataPump.MaxHC = 0;
                    standartDataPump.MaxCOP = 0;
                }
            }
        }

        //Data rounding
        protected static void RoundCOPAndP(List<Pump> pumps)
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
        //the method now only copies the values and transfers them to the standard,
        //provided that the temperature outside is already the same as in the old model and the temperature inside is also at the same temperature outside
        //and so far only for warm climates

        public virtual List<StandartPump> GetDataInListStandartPumps(List<StandartPump> standartPumps, List<Pump> oldPumps, int[] outTemps, int[] flowTemps, int forTemp, string climat)
        {
            foreach (var oldPump in oldPumps)
            {
                //Get the pump data dictionary
                Dictionary<int, List<DataPump>> oldDictionary = oldPump.Data;
                if (standartPumps.Any(x => x.Name == oldPump.Name))
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = standartPumps.FirstOrDefault(x => x.Name == oldPump.Name).Data;
                    GetConvertData(outTemps, flowTemps, forTemp, climat, newDictionary, oldDictionary, oldPump);

                }
                else
                {
                    Dictionary<int, List<StandartDataPump>> newDictionary = new Dictionary<int, List<StandartDataPump>>();
                    GetConvertData(outTemps, flowTemps, forTemp, climat, newDictionary, oldDictionary, oldPump);
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
        protected virtual void GetConvertData(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary, Dictionary<int, List<DataPump>> oldDictionary,Pump oldPump)
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
