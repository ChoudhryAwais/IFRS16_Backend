namespace IFRS16_Backend.Models
{
    public class ModificationDetails
    {
        public double? LeaseLiability { get; set; }
        public double? Rou { get; set; }
        public double? ModificationAdjustment { get; set; }

        public ModificationDetails(double? leaseLiability, double? rou, double modificationAdjustment = 0)
        {
            LeaseLiability = leaseLiability;
            Rou = rou;
            ModificationAdjustment = modificationAdjustment != 0 ? modificationAdjustment : (rou ?? 0) - (leaseLiability ?? 0);
        }
    }
}
