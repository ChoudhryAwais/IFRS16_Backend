using IFRS16_Backend.Helper;
using IFRS16_Backend.Models;
using IFRS16_Backend.Services.InitialRecognition;
using System.Globalization;

namespace IFRS16_Backend.Services.ROUSchedule
{
    public class ROUScheduleService() : IROUScheduleService
    {
        public IEnumerable<ROUScheduleTable> GetROUSchedule(double totalNPV, LeaseFormData leaseData)
        {
            // Calculate amortization
            var (_, TotalDays) = CalculateLeaseDuration.GetLeaseDuration(leaseData.CommencementDate, leaseData.EndDate);
            double amortization = ((totalNPV / TotalDays) + double.Epsilon) * 100 / 100;
            double opening = totalNPV;
            double closing = ((totalNPV - amortization) + double.Epsilon) * 100 / 100;

            var rouSchedule = new List<ROUScheduleTable>();
            DateTime currentDate = leaseData.CommencementDate;

            for (int i = 1; i <= TotalDays; i++)
            {
                // Add the ROU schedule entry
                rouSchedule.Add(new ROUScheduleTable
                {
                    Date = currentDate,
                    Opening = opening,
                    Amortization = amortization,
                    Closing = closing
                });

                // Update values for the next iteration
                currentDate = currentDate.AddDays(1);
                opening = closing;
                closing = ((opening - amortization) + double.Epsilon) * 100 / 100;
            }

            return rouSchedule;
        }
    }
}
