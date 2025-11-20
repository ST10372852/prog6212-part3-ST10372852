using Microsoft.AspNetCore.Mvc;
using CMCS.Controllers;
using CMCS.Models;
using System.Linq;

namespace CMCS.Controllers
{
    public class ApprovalsController : Controller
    {
        // Reuse in-memory list from ClaimsController for prototype
        private static System.Reflection.FieldInfo claimsField =
            typeof(ClaimsController).GetField("_claims", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

        private static System.Collections.IList GetClaimsList()
        {
            var val = claimsField?.GetValue(null);
            return val as System.Collections.IList;
        }

        public IActionResult Index()
        {
            var list = GetClaimsList();
            return View(list);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var list = GetClaimsList() as System.Collections.Generic.List<ClaimViewModel>;
            var c = list?.FirstOrDefault(x => x.ClaimID == id);
            if (c != null) c.Status = "Approved";
            TempData["Message"] = $"Claim {id} approved (prototype).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(int id, string reason)
        {
            var list = GetClaimsList() as System.Collections.Generic.List<ClaimViewModel>;
            var c = list?.FirstOrDefault(x => x.ClaimID == id);
            if (c != null) c.Status = "Rejected";
            TempData["Message"] = $"Claim {id} rejected (prototype).";
            return RedirectToAction("Index");
        }
    }
}
