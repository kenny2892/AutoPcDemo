using System.ComponentModel.DataAnnotations;
using System;

namespace AutoPcDemoWebsite.Models
{
    public class TestData
    {
        [Key]
        public string ID { get; set; }
        public string CategoryOne { get; set; }
        public string CategoryTwo { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public string AmountDisplay { get { return Amount.ToString("C2"); } }
        public bool Enabled { get; set; }
    }
}
