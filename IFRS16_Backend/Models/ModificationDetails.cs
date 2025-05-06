namespace IFRS16_Backend.Models
{
    public class ModificationDetails
    {
        public double? LeaseLiability { get; set; }
        public double? Rou { get; set; }
        public double? ModificationLoss { get; set; }
        public double? ModificationAdjustment { get; set; }


        public ModificationDetails(double? leaseLiability, double? rou, double modificationAdjustment = 0)
        {
            LeaseLiability = leaseLiability;
            Rou = rou;
            ModificationLoss = (rou ?? 0) - (leaseLiability ?? 0);
            ModificationAdjustment = modificationAdjustment;
        }
    }
}
