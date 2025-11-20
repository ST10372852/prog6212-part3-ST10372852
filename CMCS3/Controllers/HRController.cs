using Microsoft.AspNetCore.Mvc;
using CMCS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMCS.Controllers
{
    public class HRController : Controller
    {
        // Reuse ClaimsController in-memory store via reflection (prototype)
        private static System.Reflection.FieldInfo claimsField =
            typeof(ClaimsController).GetField("_claims", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        private static Dictionary<int, ClaimViewModel> GetClaims()
        {
            var val = claimsField?.GetValue(null);
            return val as Dictionary<int, ClaimViewModel> ?? new Dictionary<int, ClaimViewModel>();
        }

        // GET: /HR
        public IActionResult Index()
        {
            var claims = GetClaims().Values.ToList();
            return View(claims);
        }

        // GET: /HR/ExportApprovedCsv
        public IActionResult ExportApprovedCsv()
        {
            var claims = GetClaims().Values.Where(c => c.Status == "ManagerApproved" || c.Status == "Approved").ToList();
            var sb = new StringBuilder();
            sb.AppendLine("ClaimId, LecturerName, Month, Year, TotalAmount, Status");
            foreach (var c in claims)
            {
                sb.AppendLine($"{c.ClaimId},\"{c.LecturerName}\",{c.Month},{c.Year},{c.TotalAmount},\"{c.Status}\"");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "approved_claims.csv");
        }
    }
}
