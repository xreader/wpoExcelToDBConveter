using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.StandartModels
{
    internal class StandartDataPump
    {
        public int Temp { get; set; }
        public string Climate { get; set; }
        public double MinHC { get; set; }
        public double MidHC { get; set; }

        public double MaxHC { get; set; }

        public double MinCOP { get; set; }
        public double MidCOP { get; set; }

        public double MaxCOP { get; set; }

    }
}
