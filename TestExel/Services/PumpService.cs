using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.StandartModels;

namespace TestExel.Services
{
    class PumpService
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
                MinCOP = dataPump.MinCOP < 1 ? 1 : dataPump.MinCOP,
                MidCOP = dataPump.MidCOP < 1 ? 1 : dataPump.MidCOP,
                MaxCOP = dataPump.MaxCOP < 1 ? 1 : dataPump.MaxCOP,
                MaxVorlauftemperatur = dataPump.MaxVorlauftemperatur
            };
        }
        //Creating a new data object according to the standard when it is not in the table
        protected StandartDataPump CreateStandartDataPumpWannOtherTemp(DataPump oldDataWithHighGrad, DataPump oldDataWithLowGrad, int flowTemp, int forTemp, string climat)
        {
            var dif = oldDataWithHighGrad.Temp - flowTemp;
            var minCop = Math.Round(oldDataWithHighGrad.MinCOP - dif * (oldDataWithHighGrad.MinCOP - oldDataWithLowGrad.MinCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            var midCop = Math.Round(oldDataWithHighGrad.MidCOP - dif * (oldDataWithHighGrad.MidCOP - oldDataWithLowGrad.MidCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            var maxCop = Math.Round(oldDataWithHighGrad.MaxCOP - dif * (oldDataWithHighGrad.MaxCOP - oldDataWithLowGrad.MaxCOP) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2);
            return new StandartDataPump
            {
                ForTemp = forTemp,
                FlowTemp = flowTemp,
                Climate = climat,
                MinHC = Math.Round(oldDataWithHighGrad.MinHC - dif * (oldDataWithHighGrad.MinHC - oldDataWithLowGrad.MinHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                MidHC = Math.Round(oldDataWithHighGrad.MidHC - dif * (oldDataWithHighGrad.MidHC - oldDataWithLowGrad.MidHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                MaxHC = Math.Round(oldDataWithHighGrad.MaxHC - dif * (oldDataWithHighGrad.MaxHC - oldDataWithLowGrad.MaxHC) / (oldDataWithHighGrad.Temp - oldDataWithLowGrad.Temp), 2),
                MinCOP = minCop < 1 ? 1 : minCop,
                MidCOP = midCop < 1 ? 1 : midCop,
                MaxCOP = maxCop < 1 ? 1 : maxCop,
                MaxVorlauftemperatur = oldDataWithLowGrad.MaxVorlauftemperatur
            };
        }
        //Calculates data for the pump when we do not have data at this temperature outside
        protected List<DataPump> FindDataWhenNoDatainThisOutTemp(Dictionary<int, List<DataPump>> oldDictionary, int outTemp)
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
                MinHC = Math.Round(minElement.MinHC + (outTemp - maxKeyBeforeTarget) * (maxElement.MinHC - minElement.MinHC) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                MidHC = Math.Round(minElement.MidHC + (outTemp - maxKeyBeforeTarget) * (maxElement.MidHC - minElement.MidHC) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                MaxHC = Math.Round(minElement.MaxHC + (outTemp - maxKeyBeforeTarget) * (maxElement.MaxHC - minElement.MaxHC) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                MinCOP = Math.Round(minElement.MinCOP + (outTemp - maxKeyBeforeTarget) * (maxElement.MinCOP - minElement.MinCOP) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                MidCOP = Math.Round(minElement.MidCOP + (outTemp - maxKeyBeforeTarget) * (maxElement.MidCOP - minElement.MidCOP) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2),
                MaxCOP = Math.Round(minElement.MaxCOP + (outTemp - maxKeyBeforeTarget) * (maxElement.MaxCOP - minElement.MaxCOP) / (maxKeyBeforeTarget - minKeyBeforeTarget), 2)
            }).ToList();

            return oldDataPump;
        }
        //Convert the data
        protected void ConvertDataInStandart(List<DataPump> oldDataPump, int flowTemp, int outTemp, int forTemp, string climat, Dictionary<int, List<StandartDataPump>> newDictionary)
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

                if (oldDataWithHighGrad != null && oldDataWithLowGrad != null)
                {
                    standartDataPump = CreateStandartDataPumpWannOtherTemp(oldDataWithHighGrad, oldDataWithLowGrad, flowTemp, forTemp, climat);

                    standartDataPumpChanged = true;
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
       
    }
}
