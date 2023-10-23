using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoExporter
{
    public class Data
    {
        public string ID { get; set; }
        public string CategoryOne { get; set; }
        public string CategoryTwo { get; set; }
        public DateTime Date { get; set; }
        public string Amount { get; set; }
        public bool Enabled { get; set; }
    }
}
