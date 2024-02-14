using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestExel.DBModels
{
    public class Text
    {
        public int textid { get; set; }    
        public int version { get; set; }
        public int stand { get; set; }
        public string? flags { get; set; }
        public string? info { get; set; }
        public string? ger { get; set; }
        public string? eng { get; set; }
        public string? da_DK { get; set; }
        public string? hu_HU { get; set; }
        public string? nl_NL { get; set; }
        public string? nn_NO { get; set; }
        public string? pl_PL { get; set; }
        public string? sr_RS { get; set; }
        public string? sl_SI { get; set; }
        public string? fr_FR { get; set; }
        public string? fi_FI { get; set; }
        public string? sv_SE { get; set; }
        public string? bg_BG { get; set; }
        public string? tr_TR { get; set; }
        public string? hr_HR { get; set; }
        public string? it_IT { get; set; }
        public string? cs_CZ { get; set; }
        public string? es_ES { get; set; }
        public string? lt_LT { get; set; }
        public string? et_EE { get; set; }
        public string? el_GR { get; set; }
        public string? lv_LV { get; set; }
        public string? ru_RU { get; set; }
        public string? pt_PT { get; set; }
        public string? ro_RO { get; set; }
        public string? sk_SK { get; set; }
    }
}
