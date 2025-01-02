﻿using System.ComponentModel.DataAnnotations;

namespace IFRS16_Backend.Models
{
    public class InitialRecognitionTable
    {
        [Key]
        public int ID { get; set; }
        public int LeaseId { get; set; }
        public int SerialNo { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Rental { get; set; }
        public decimal NPV { get; set; }
    }

    public class InitialRecognitionResult
    {
        public decimal TotalNPV { get; set; }
        public List<InitialRecognitionTable> InitialRecognition { get; set; }
        public List<double> CashFlow { get; set; }
        public List<DateTime> Dates { get; set; }
    }
}
