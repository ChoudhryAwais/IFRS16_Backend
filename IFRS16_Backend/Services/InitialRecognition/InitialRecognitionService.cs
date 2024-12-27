﻿using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using Microsoft.SqlServer.Server;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public class InitialRecognitionService(ApplicationDbContext context) : IInitialRecognitionService
    {
        private readonly ApplicationDbContext _context = context;

        public InitialRecognitionResult GetInitialRecognitionForLease(int leaseId)
        {
            var leaseSpecificData = _context.LeaseData.FirstOrDefault(l => l.LeaseId == leaseId);
            if (leaseSpecificData == null)
            {
                throw new ArgumentException($"Lease with ID {leaseId} not found.");
            }
            var (TotalYears, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.EndDate);
            var startTable = (leaseSpecificData.Annuity == "advance") ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == "advance") ? TotalYears -1 : TotalYears;
            decimal rental = leaseSpecificData.Rental;
            int IBR = leaseSpecificData.IBR;    
            decimal totalNPV = 0;
            decimal discountFactor = 1 + (IBR / 100m);
            List<double> cashFlow = [];
            List<string> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];

            for (int i = startTable; i <= endTable; i++)
            {
                // Ensure IBR is in decimal for precision
                decimal NPV = rental / DecimalPower.DecimalPowerCal(discountFactor, i);
                DateTime newDate = leaseSpecificData.CommencementDate.AddYears(i);
                if (leaseSpecificData.Annuity == "Arrears")
                    newDate = newDate.AddDays(-1);
                string formattedDate = newDate.ToString("yyyy-MM-dd");
                string formattedDateForXirr = newDate.ToString("yyyyMMdd");
                totalNPV += NPV;


                InitialRecognitionTable tableObj = new()
                {
                    SerialNo = startTable == 0 ? i + 1 : i,
                    PaymentDate = formattedDate,
                    Rental = rental,
                    NPV = NPV
                };
                cashFlow.Add((double)rental);
                dates.Add(formattedDateForXirr);
                initialRecognition.Add(tableObj);

            }

            cashFlow.Insert(0, (double)-totalNPV);
            dates.Insert(0, leaseSpecificData.CommencementDate.ToString("yyyyMMdd"));

            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognition,
                CashFlow = cashFlow,
                Dates = dates
            };
        }
    }
}