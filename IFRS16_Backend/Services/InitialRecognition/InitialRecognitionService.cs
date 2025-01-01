using IFRS16_Backend.enums;
using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.LeaseData;
using Microsoft.SqlServer.Server;

namespace IFRS16_Backend.Services.InitialRecognition
{
    public class InitialRecognitionService() : IInitialRecognitionService
    {
        public InitialRecognitionResult GetInitialRecognitionForLease(LeaseFormData leaseSpecificData)
        {
            var (TotalInitialRecoDuration, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.EndDate,leaseSpecificData.Frequency);
            var startTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? TotalInitialRecoDuration - 1: TotalInitialRecoDuration;
            decimal rental = leaseSpecificData.Rental;
            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.Frequency);
            int IBR = leaseSpecificData.IBR/ (12 / frequecnyFactor);
            decimal totalNPV = 0;
            decimal discountFactor = (1 + (IBR / 100m));
            List<double> cashFlow = [];
            List<DateTime> dates = [];
            List<InitialRecognitionTable> initialRecognition = [];

            for (int i = startTable; i <= endTable; i++)
            {
                // Ensure IBR is in decimal for precision
                decimal NPV = rental / DecimalPower.DecimalPowerCal(discountFactor, i);
                DateTime newDate = leaseSpecificData.CommencementDate.AddMonths(i * frequecnyFactor);
                if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                    newDate = newDate.AddDays(-1);
                string formattedDate = newDate.ToString("yyyy-MM-dd");
                DateTime formattedDateForXirr = newDate;
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
            dates.Insert(0, leaseSpecificData.CommencementDate);

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
