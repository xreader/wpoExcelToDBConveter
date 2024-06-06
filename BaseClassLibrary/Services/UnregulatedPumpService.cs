using BaseClassLibrary.Models;
using BaseClassLibrary.StandartModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClassLibrary.Services
{
    public class UnregulatedPumpService
    {
        public List<UnregulatedStandartPump> CreateListUnregulatedStandartPumps() => new List<UnregulatedStandartPump>();
        //Creating a new data object according to the standard when it is in the table
        protected UnregulatedStandartDataPump UnregulatedCreateStandartDataPump(UnregulatedDataPump dataPump, string climat)
        {
            return new UnregulatedStandartDataPump
            {
                ForTemp = dataPump.Temp,
                FlowTemp = dataPump.Temp,
                Climate = climat,
                HC = dataPump.HC,
                COP = dataPump.COP == 0 ? 0 : dataPump.COP == 0 ? 0 : dataPump.COP < 1 ? 1 : dataPump.COP,
                MaxVorlauftemperatur = dataPump.MaxVorlauftemperatur
            };
        }
        //Creating a new data object according to the standard when it is not in the table
        protected UnregulatedStandartDataPump UnregulatedCreateStandartDataPumpWannOtherTemp(UnregulatedDataPump oldDataWithHighGrad, UnregulatedDataPump oldDataWithLowGrad, int flowTemp, int forTemp, string climat)
        {
            var dif = oldDataWithHighGrad.Temp - flowTemp;
            var Cop = Math.Round(oldDataWithHighGrad.COP - dif * (oldDataWithHighGrad.COP - oldDataWithLowGrad.COP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);

            return new UnregulatedStandartDataPump
            {
                ForTemp = forTemp,
                FlowTemp = flowTemp,
                Climate = climat,
                HC = Math.Round(oldDataWithHighGrad.HC - dif * (oldDataWithHighGrad.HC - oldDataWithLowGrad.HC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                COP = Cop == 0 ? 0 : Cop == 0 ? 0 : Cop < 1 ? 1 : Cop,
                MaxVorlauftemperatur = oldDataWithLowGrad.MaxVorlauftemperatur
            };
        }
        //Calculates data for the pump when we do not have data at this temperature outside for unregulated pumps
        protected List<UnregulatedDataPump> UnregulatedFindDataWhenNoDatainThisOutTemp(Dictionary<int, List<UnregulatedDataPump>> oldDictionary, int outTemp)
        {
            var maxKeyBeforeTarget = oldDictionary.Keys.Where(key => key < outTemp).DefaultIfEmpty(int.MinValue).Max();
            var minKeyBeforeTarget = oldDictionary.Keys.Where(key => key > outTemp).DefaultIfEmpty(int.MaxValue).Min();

            if (!oldDictionary.TryGetValue(maxKeyBeforeTarget, out var minDataPump) ||
                !oldDictionary.TryGetValue(minKeyBeforeTarget, out var maxDataPump))
            {
                return new List<UnregulatedDataPump>();
            }
            //Calculation of data for the pump, provided that there was no such temperature outside
            var oldDataPump = minDataPump.Zip(maxDataPump, (minElement, maxElement) => new UnregulatedDataPump
            {
                Temp = minElement.Temp,
                HC = Math.Round(minElement.HC + (outTemp - maxKeyBeforeTarget) * (maxElement.HC - minElement.HC) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                COP = Math.Round(minElement.COP + (outTemp - maxKeyBeforeTarget) * (maxElement.COP - minElement.COP) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2)
            }).ToList();

            return oldDataPump;
        }
        //Convert the data for unregulated pumps
        protected void UnregulatedConvertDataInStandart(List<UnregulatedDataPump> oldDataPump, int flowTemp, int outTemp, int forTemp, string climat, Dictionary<int, List<UnregulatedStandartDataPump>> newDictionary)
        {
            var standartDataPump = new UnregulatedStandartDataPump();
            bool standartDataPumpChanged = false;
            if (oldDataPump.Any(x => x.Temp == flowTemp))
            {
                var oldDataForThisOutAndFlowTemp = oldDataPump.FirstOrDefault(x => x.Temp == flowTemp);
                standartDataPump = UnregulatedCreateStandartDataPump(oldDataForThisOutAndFlowTemp, climat);
                standartDataPumpChanged = true;
            }
            else
            {
                var oldDataWithHighGrad = oldDataPump.FirstOrDefault(x => x.Temp == 55);
                var oldDataWithLowGrad = oldDataPump.FirstOrDefault(x => x.Temp == 35);

                if (oldDataWithHighGrad != null && oldDataWithLowGrad != null)
                {
                    standartDataPump = UnregulatedCreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp, forTemp, climat);

                    standartDataPumpChanged = true;
                }
            }


            //Сheck whether data has been added, if not, then there is no data and there is no need to add it
            if (standartDataPumpChanged)
            {
                if (!newDictionary.TryGetValue(outTemp, out var newStandartDataPump))
                {
                    newStandartDataPump = new List<UnregulatedStandartDataPump>();
                    newDictionary.Add(outTemp, newStandartDataPump);
                }

                newStandartDataPump.Add(standartDataPump);
            }
        }
        //Data rounding
        protected static void RoundCOPAndP_InUnregulatedPumps(List<UnregulatedPump> pumps)
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
        //Get already converted data
        protected virtual void UnregulatedGetConvertData(int[] outTemps, int[] flowTemp, int forTemp, string climat, Dictionary<int, List<UnregulatedStandartDataPump>> newDictionary, Dictionary<int, List<UnregulatedDataPump>> oldDictionary)
        {
            for (int i = 0; i < outTemps.Length; i++)
            {

                if (oldDictionary.ContainsKey(outTemps[i]))
                {
                    //Сode if there is a value for this temperature outside
                    oldDictionary.TryGetValue(outTemps[i], out List<UnregulatedDataPump> oldDataPump);
                    //Convert values
                    UnregulatedConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary);

                }
                else
                {
                    //Code if there is no such temperature outside in the table
                    //Search for data for a temperature outside when there is none
                    var oldDataPump = UnregulatedFindDataWhenNoDatainThisOutTemp(oldDictionary, outTemps[i]);
                    //Convert values
                    UnregulatedConvertDataInStandart(oldDataPump, flowTemp[i], outTemps[i], forTemp, climat, newDictionary);
                }
            }
        }
    
    
    }
}
