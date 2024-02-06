using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.StandartModels
{
    public class StandartPump
    {
        public string Name { get; set; }
        public Dictionary<int, List<StandartDataPump>> Data { get; set; }
    }
}
