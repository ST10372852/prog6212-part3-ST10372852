using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace CMCS.Models
{
    public class ClaimViewModel
    {
        public int ClaimId { get; set; }

        // Alias to prevent errors if some Views still use ClaimID
        public int ClaimID => ClaimId;

        public int Month { get; set; }
        public int Year { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        public string LecturerName { get; set; } = "LecturerName";

        public DateTime SubmissionDate { get; set; } = DateTime.Now;

        // Status workflow: Submitted → CoordinatorApproved/Rejected → ManagerApproved/Rejected
        public string Status { get; set; } = "Submitted";

        public List<ClaimLineViewModel> Lines { get; set; } = new List<ClaimLineViewModel>();

        public decimal TotalAmount => Lines.Sum(l => l.LineTotal);

        // Upload support
        public List<string> UploadedFiles { get; set; } = new List<string>();

        // Detailed document info with stored paths
        public List<DocumentViewModel> Documents { get; set; } = new List<DocumentViewModel>();
        public List<IFormFile> NewFiles { get; set; }
        // Validation/automation feedback stored on the claim for review
        public List<string> ValidationIssues { get; set; } = new List<string>();
    }
}
