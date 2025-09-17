using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using System.Linq;
using EFCore.BulkExtensions;

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
            double IBR = leaseSpecificData.IBR; /// (12 / frequecnyFactor); // Now IBR is always as is. (24/3/2025)
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
                var (_, _, PowerFactor) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, newDate, leaseSpecificData.Frequency, true);
                if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                    newDate = newDate.AddDays(-1);
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
                    NPV = NPV,
                    IsActive = true
                };
                cashFlow.Add((double)rental);
                dates.Add(newDate);
                initialRecognition.Add(tableObj);
            }
            cashFlow.Insert(0, (double)-totalNPV);
            dates.Insert(0, leaseSpecificData.CommencementDate);
            try
            {
                await _context.BulkInsertAsync(initialRecognition);

                return new InitialRecognitionResult
                {
                    TotalNPV = totalNPV,
                    InitialRecognition = initialRecognition,
                    CashFlow = cashFlow,
                    Dates = dates
                };
            }
            catch (Exception ex)
            {
                // Log and handle exceptions appropriately
                Console.WriteLine(ex);
                throw;
            }

        }
        public async Task<InitialRecognitionResult> PostCustomInitialRecognitionForLease(LeaseFormData leaseSpecificData)
        {
            decimal rental = (decimal)leaseSpecificData.Rental;
            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.Frequency);
            double IBR = leaseSpecificData.IBR; // / (12 / frequecnyFactor);
            decimal totalNPV = 0;
            decimal discountFactor = (1 + ((decimal)IBR / 100m));
            List<double> cashFlow = [];
            List<DateTime> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];
            for (int i = 0; i < leaseSpecificData.CustomIRTable.Count; i++)
            {
                var (_, _, PowerFactor) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.CustomIRTable[i].PaymentDate, leaseSpecificData.Frequency, true);
                decimal NPV = leaseSpecificData.CustomIRTable[i].Rental / (decimal)Math.Pow((double)discountFactor, (double)PowerFactor);
                totalNPV += NPV;

                InitialRecognitionTable tableObj = new()
                {
                    LeaseId = leaseSpecificData.LeaseId,
                    SerialNo = leaseSpecificData.CustomIRTable[i].SerialNo,
                    PaymentDate = leaseSpecificData.CustomIRTable[i].PaymentDate,
                    Rental = leaseSpecificData.CustomIRTable[i].Rental,
                    NPV = NPV,
                    IsActive = true
                };
                cashFlow.Add((double)leaseSpecificData.CustomIRTable[i].Rental);
                dates.Add(leaseSpecificData.CustomIRTable[i].PaymentDate);
                initialRecognition.Add(tableObj);
            }
            cashFlow.Insert(0, (double)-totalNPV);
            dates.Insert(0, leaseSpecificData.CommencementDate);
            try
            {
                await _context.BulkInsertAsync(initialRecognition);
                return new InitialRecognitionResult
                {
                    TotalNPV = totalNPV,
                    InitialRecognition = initialRecognition,
                    CashFlow = cashFlow,
                    Dates = dates
                };
            }
            catch (Exception ex)
            {
                // Log and handle exceptions appropriately
                Console.WriteLine(ex);
                throw;
            }

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
            List<InitialRecognitionTable> fullInitialRecognitionTable = await _context.InitialRecognition.Where(item => item.LeaseId == leaseId && (item.IsActive == true) && (startDate == null || endDate == null || (item.PaymentDate >= startDate && item.PaymentDate <= endDate))).ToListAsync();
            decimal totalNPV = fullInitialRecognitionTable.Sum(item => item.NPV);
            List<DateTime> dates = [.. fullInitialRecognitionTable.Select(item => item.PaymentDate)];
            int totalRecord = fullInitialRecognitionTable.Count;

            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognitionTable,
                TotalRecords = totalRecord,
                Dates = dates
            };
        }

        public async Task<List<InitialRecognitionTable>> GetAllInitialRecognitionForLease(int leaseId)
        {

            List<InitialRecognitionTable> fullInitialRecognitionTable = await _context.InitialRecognition.Where(item => item.LeaseId == leaseId && item.IsActive == true).ToListAsync();

            return fullInitialRecognitionTable;
        }
    }
}
