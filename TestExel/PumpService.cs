using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel
{
    internal class PumpService
    {
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
                pump.GetData();
                
                if (pump != null && pump.Name != "")
                    pumps.Add(pump);

            }
            RoundCOP(pumps);
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
        public static void RoundCOP(List<Pump> pumps)
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
