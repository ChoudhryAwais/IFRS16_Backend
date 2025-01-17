using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public class InitialRecognitionService(ApplicationDbContext context) : IInitialRecognitionService
    {
        private readonly ApplicationDbContext _context = context;
        public async Task<InitialRecognitionResult> PostInitialRecognitionForLease(LeaseFormData leaseSpecificData)
        {
            var (TotalInitialRecoDuration, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.EndDate, leaseSpecificData.Frequency);
            var startTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? TotalInitialRecoDuration - 1 : TotalInitialRecoDuration;
            endTable = (leaseSpecificData.GRV != null & leaseSpecificData.GRV != 0) ? endTable + 1 : endTable   ;
            decimal rental = leaseSpecificData.Rental;
            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.Frequency);
            int IBR = leaseSpecificData.IBR / (12 / frequecnyFactor);
            decimal totalNPV = 0;
            decimal discountFactor = (1 + (IBR / 100m));
            List<double> cashFlow = [];
            List<DateTime> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];
            decimal incremetPre = 0;
            if (leaseSpecificData.Increment != null && leaseSpecificData.Increment != 0)
            {
                incremetPre = (1 + ((decimal)leaseSpecificData.Increment / 100m));
            }

            for (int i = startTable; i <= endTable; i++)
            {
                DateTime newDate = leaseSpecificData.CommencementDate.AddMonths(i * frequecnyFactor);
                if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                    newDate = newDate.AddDays(-1);
                if (leaseSpecificData.GRV != null && leaseSpecificData.GRV != 0 && i == endTable)
                {
                    rental = (int)leaseSpecificData.GRV;
                    newDate = leaseSpecificData.EndDate;
                }

                decimal NPV = rental / DecimalPower.DecimalPowerCal(discountFactor, i);
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
                rental = rental * incremetPre;
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

        public async Task<InitialRecognitionResult> GetInitialRecognitionForLease(int pageNumber, int pageSize, int leaseId)
        {
            LeaseFormData? leaseSpecificData = await _context.LeaseData.FirstOrDefaultAsync(item => item.LeaseId == leaseId) ?? throw new InvalidOperationException("No lease data found for the given LeaseId.");
            IEnumerable<InitialRecognitionTable> initialRecognitionTable = await _context.GetInitialRecognitionPaginatedAsync(pageNumber, pageSize, leaseId);
            List<InitialRecognitionTable> fullInitialRecognitionTable = await _context.InitialRecognition.Where(item => item.LeaseId == leaseId).ToListAsync();
            decimal totalNPV = fullInitialRecognitionTable.Sum(item => item.NPV);
            int totalRecord = fullInitialRecognitionTable.Count;


            return new InitialRecognitionResult
            {
                TotalNPV = totalNPV,
                InitialRecognition = initialRecognitionTable,
                TotalRecords = totalRecord
            };
        }

    }
}
