using IFRS16_Backend.enums;
using IFRS16_Backend.Models;

namespace IFRS16_Backend.Helper
{
    public class IRhelper
    {
        public static List<DateTime> InitialRecognitionHelper(LeaseFormData leaseSpecificData)
        {
            var (startTable, endTable, frequecnyFactor) = DurationOfIRhelper(leaseSpecificData);
            List<DateTime> incrementalDates = [];
            for (int i = startTable; i <= endTable; i++)
            {
                DateTime newDate = leaseSpecificData.CommencementDate.AddMonths(i * frequecnyFactor);
                if (leaseSpecificData.Annuity == AnnuityType.Arrears)
                    newDate = newDate.AddDays(-1);
                incrementalDates.Add(newDate);
            }
            incrementalDates.RemoveAt(0);
            return incrementalDates;
        }

        public static (int startTable, double endTable, int frequecnyFactor) DurationOfIRhelper(LeaseFormData leaseSpecificData)
        {
            var (TotalInitialRecoDuration, _, _) = CalculateLeaseDuration.GetLeaseDuration(leaseSpecificData.CommencementDate, leaseSpecificData.EndDate, leaseSpecificData.IncrementalFrequency);
            var startTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? 0 : 1;
            var endTable = (leaseSpecificData.Annuity == AnnuityType.Advance) ? TotalInitialRecoDuration - 1 : TotalInitialRecoDuration;
            int frequecnyFactor = CalFrequencyFactor.FrequencyFactor(leaseSpecificData.IncrementalFrequency);

            return (startTable, endTable, frequecnyFactor);
        }

    }
}
