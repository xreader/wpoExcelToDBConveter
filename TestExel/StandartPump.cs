using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel
{
    internal class StandartPump
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public Dictionary<int, List<StandartDataPump>> Data { get; set; }
    }
}
