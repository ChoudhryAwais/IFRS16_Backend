﻿using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System.Linq;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public class InitialRecognitionService(ApplicationDbContext context) : IInitialRecognitionService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<InitialRecognitionResult> PostInitialRecognitionForLease(LeaseFormData leaseSpecificData)
        {
            var (TotalInitialRecoDuration, _, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.EndDate, leaseSpecificData.Frequency);
            var startTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? TotalInitialRecoDuration - 1 : TotalInitialRecoDuration;
            endTable = (leaseSpecificData.GRV != null & leaseSpecificData.GRV != 0) ? endTable + 1 : endTable;
            decimal rental = (decimal)leaseSpecificData.Rental;
            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.Frequency);
            int incrementalFrequecnyFactor = 1;
            double IBR = leaseSpecificData.IBR / (12 / frequecnyFactor);
            decimal totalNPV = 0;
            decimal discountFactor = (1 + ((decimal)IBR / 100m));
            List<double> cashFlow = [];
            List<DateTime> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];
            decimal incremetPre = 1;
            if (leaseSpecificData.Increment != null && leaseSpecificData.Increment != 0)
            {
                incremetPre = (1 + ((decimal)leaseSpecificData.Increment / 100m));
                incrementalFrequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.IncrementalFrequency) / frequecnyFactor;
            }

            for (int i = startTable, incremetPeriod = incrementalFrequecnyFactor + ((leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1); i <= endTable; i++)
            {
                DateTime newDate = leaseSpecificData.CommencementDate.AddMonths(i * frequecnyFactor);
                var (PowerFactor, _, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, newDate, leaseSpecificData.Frequency);
                //if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                //    newDate = newDate.AddDays(-1);
                if (i == incremetPeriod && leaseSpecificData.Increment != null && leaseSpecificData.Increment != 0)
                {
                    rental *= incremetPre;
                    incremetPeriod += incrementalFrequecnyFactor;
                }
                if (leaseSpecificData.GRV != null && leaseSpecificData.GRV != 0 && i == endTable)
                {
                    rental = (int)leaseSpecificData.GRV;
                    newDate = leaseSpecificData.EndDate;
                }

                decimal NPV = rental / DecimalPower.DecimalPowerCal(discountFactor, (int)PowerFactor);
                totalNPV += NPV;

                InitialRecognitionTable tableObj = new()
                {
                    LeaseId = leaseSpecificData.LeaseId,
                    SerialNo = startTable == 0 ? i + 1 : i,
                    PaymentDate = newDate,
                    Rental = rental,
                    NPV = NPV
                };
                cashFlow.Add((double)rental);
                dates.Add(newDate);
                initialRecognition.Add(tableObj);
            }
            cashFlow.Insert(0, (double)-totalNPV);
            dates.Insert(0, leaseSpecificData.CommencementDate);
            _context.InitialRecognition.AddRange(initialRecognition);
            await _context.SaveChangesAsync();

            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognition,
                CashFlow = cashFlow,
                Dates = dates
            };
        }
        public async Task<InitialRecognitionResult> ModifyInitialRecognitionForLease(LeaseFormModification leaseSpecificData)
        {

            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.Frequency);
            var commencementDate = leaseSpecificData.CommencementDate;
            if (leaseSpecificData.Annuity == AnnuityType.Arrears)
            {
                commencementDate = commencementDate.AddMonths(-1 * frequecnyFactor);
            }
            var (TotalInitialRecoDuration, _, _) = CalculateLeaseDuration.GetLeaseDuration(commencementDate, leaseSpecificData.EndDate, leaseSpecificData.Frequency);
           
            var startTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? TotalInitialRecoDuration - 1 : TotalInitialRecoDuration;
            endTable = (leaseSpecificData.GRV != null & leaseSpecificData.GRV != 0) ? endTable + 1 : endTable;
            decimal rental = (decimal)leaseSpecificData.Rental;
            int incrementalFrequecnyFactor = 1;
            double IBR = leaseSpecificData.IBR / (12 / frequecnyFactor);
            decimal totalNPV = 0;
            decimal discountFactor = (1 + ((decimal)IBR / 100m));
            List<double> cashFlow = [];
            List<DateTime> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];
            decimal incremetPre = 1;
            if (leaseSpecificData.Increment != null && leaseSpecificData.Increment != 0)
            {
                incremetPre = (1 + ((decimal)leaseSpecificData.Increment / 100m));
                incrementalFrequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.IncrementalFrequency) / frequecnyFactor;
            }

            for (int i = startTable, incremetPeriod = incrementalFrequecnyFactor + ((leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1); i <= endTable; i++)
            {
                DateTime newDate = commencementDate.AddMonths(i * frequecnyFactor);
                var (_, _, PowerFactor) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.ModificationDate, newDate, leaseSpecificData.Frequency);
                //if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                //    newDate = newDate.AddDays(-1);
                if (i == incremetPeriod && leaseSpecificData.Increment != null && leaseSpecificData.Increment != 0)
                {
                    rental *= incremetPre;
                    incremetPeriod += incrementalFrequecnyFactor;
                }
                if (leaseSpecificData.GRV != null && leaseSpecificData.GRV != 0 && i == endTable)
                {
                    rental = (int)leaseSpecificData.GRV;
                    newDate = leaseSpecificData.EndDate;
                }

                decimal NPV = rental / (decimal)Math.Pow((double)discountFactor, (double)PowerFactor);
                totalNPV += NPV;

                InitialRecognitionTable tableObj = new()
                {
                    LeaseId = leaseSpecificData.LeaseId,
                    SerialNo = startTable == 0 ? i + 1 : i,
                    PaymentDate = newDate,
                    Rental = rental,
                    NPV = NPV
                };
                cashFlow.Add((double)rental);
                dates.Add(newDate);
                initialRecognition.Add(tableObj);
            }
            cashFlow.Insert(0, (double)-totalNPV);
            dates.Insert(0, commencementDate);
            //_context.InitialRecognition.AddRange(initialRecognition);
            //await _context.SaveChangesAsync();

            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognition,
                CashFlow = cashFlow,
                Dates = dates,
                TotalRecords = initialRecognition.Count
            };
        }
        public async Task<InitialRecognitionResult> GetInitialRecognitionForLease(int pageNumber, int pageSize, int leaseId, DateTime? startDate, DateTime? endDate)
        {
            LeaseFormData? leaseSpecificData = await _context.LeaseData.FirstOrDefaultAsync(item => item.LeaseId == leaseId) ?? throw new InvalidOperationException("No lease data found for the given LeaseId.");
            IEnumerable<InitialRecognitionTable> initialRecognitionTable = await _context.GetInitialRecognitionPaginatedAsync(pageNumber, pageSize, leaseId, startDate, endDate);
            List<InitialRecognitionTable> fullInitialRecognitionTable = await _context.InitialRecognition.Where(item => item.LeaseId == leaseId && (startDate == null || endDate == null || (item.PaymentDate >= startDate && item.PaymentDate <= endDate))).ToListAsync();
            decimal totalNPV = fullInitialRecognitionTable.Sum(item => item.NPV);
            List<DateTime> dates = [.. fullInitialRecognitionTable.Select(item => item.PaymentDate)];

            int totalRecord = fullInitialRecognitionTable.Count;
            //DateTime specificDate = new DateTime(2020, 06, 30); // 30 June 2020
            //DateTime endDates = new DateTime(2021, 12, 31); // 31 Dec 2020
            //var (_, _, TotalInitialDurationWithDecimal) = CalculateLeaseDuration.GetLeaseDuration(specificDate, endDates, leaseSpecificData.Frequency);

            //for (int i = 0; i <= fullInitialRecognitionTable.Count; i++)
            //{

            //    var (TotalInitialRecoDuration, _, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, fullInitialRecognitionTable[i].PaymentDate, leaseSpecificData.Frequency);
            //    var abc = TotalInitialRecoDuration;
            //}


            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognitionTable,
                TotalRecords = totalRecord,
                Dates = dates
            };
        }

    }
}
