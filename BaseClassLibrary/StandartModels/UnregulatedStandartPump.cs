using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestExel.StandartModels;

namespace BaseClassLibrary.StandartModels
{
    public class UnregulatedStandartPump
    {
        public string Name { get; set; }
        public Dictionary<int, List<UnregulatedStandartDataPump>> Data { get; set; }
    }
}
