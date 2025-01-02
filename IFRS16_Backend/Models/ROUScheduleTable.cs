﻿using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class ROUScheduleTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public DateTime ROU_Date { get; set; }
        public double Opening { get; set; }
        public double Amortization { get; set; }
        public double Closing { get; set; }
    }
    public class ROUScheduleRequest
    {
        public LeaseFormData LeaseData { get; set; }
        public double TotalNPV { get; set; }
    }
}
