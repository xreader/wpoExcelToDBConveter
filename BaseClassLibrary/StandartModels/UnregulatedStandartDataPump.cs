using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClassLibrary.StandartModels
{
    public class UnregulatedStandartDataPump
    {
        public int ForTemp { get; set; }
        public int FlowTemp { get; set; }
        public string Climate { get; set; }
        public int MaxVorlauftemperatur { get; set; }
        public double HC { get; set; }
        public double COP { get; set; }
    }
}
