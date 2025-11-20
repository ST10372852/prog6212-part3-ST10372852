namespace CMCS.Models
{
    public class ClaimLineViewModel
    {
        public string Module { get; set; }
        public int Hours { get; set; }
        public decimal Rate { get; set; }

        // Alias so Views using Description don’t break
        public string Description
        {
            get => Module;
            set => Module = value;
        }

        public decimal LineTotal => Hours * Rate;
    }
}
