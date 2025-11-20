using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class ClaimLineViewModel
    {
        [Required(ErrorMessage = "Module/Activity is required")]
        public string Module { get; set; }

        [Range(0, 1000, ErrorMessage = "Hours must be 0 or greater")]
        public int Hours { get; set; }

        [Range(0.0, 10000.0, ErrorMessage = "Rate must be 0 or greater")]
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
