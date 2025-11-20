using Microsoft.AspNetCore.Mvc;
using CMCS.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CMCS.Services;

namespace CMCS.Controllers
{
    public class ClaimsController : Controller
    {
        // Mock storage just for prototype demo
        private static Dictionary<int, ClaimViewModel> _claims = new();
        private static int _nextId = 1;

        // GET: /Claims/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            var model = new ClaimViewModel
            {
                ClaimId = _nextId++,
                Lines = new List<ClaimLineViewModel> { new ClaimLineViewModel() }
            };
            _claims[model.ClaimId] = model;
            return View(model);
        }

        // POST: /Claims/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ClaimViewModel model)
        {
            // Run server-side validation and automation checks
            var issues = ClaimValidator.Validate(model);
            model.ValidationIssues = issues.ToList();

            if (issues.Length > 0 || !ModelState.IsValid)
            {
                // Keep the model in store so user can correct
                if (!_claims.ContainsKey(model.ClaimId))
                {
                    _claims[model.ClaimId] = model;
                }
                else
                {
                    _claims[model.ClaimId] = model;
                }
                TempData["Error"] = "Validation issues found. Please review and resubmit.";
                return View(model);
            }

            if (_claims.ContainsKey(model.ClaimId))
            {
                _claims[model.ClaimId] = model;
            }

            TempData["Message"] = "Claim submitted successfully.";
            return RedirectToAction("Track");
        }

        // GET: /Claims/Track
        [HttpGet]
        public IActionResult Track()
        {
            return View(_claims.Values.ToList());
        }

        // GET: /Claims/Details/5
        [HttpGet]
        public IActionResult Details(int id)
        {
            if (!_claims.ContainsKey(id)) return NotFound();
            return View(_claims[id]);
        }

        // ✅ NEW: Upload GET
        [HttpGet]
        public IActionResult Upload(int id)
        {
            if (!_claims.ContainsKey(id)) return NotFound();
            return View(_claims[id]);
        }

        // ✅ NEW: Upload POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(int id, ClaimViewModel model)
        {
            if (!_claims.ContainsKey(id)) return NotFound();

            var claim = _claims[id];

            if (model.NewFiles != null && model.NewFiles.Any())
            {
                foreach (var file in model.NewFiles)
                {
                    if (file.Length > 0)
                    {
                        // Prototype: keep filename only
                        claim.UploadedFiles.Add(file.FileName);

                        // Optional: Save file physically
                        // string path = Path.Combine("wwwroot/uploads", file.FileName);
                        // using var stream = new FileStream(path, FileMode.Create);
                        // file.CopyTo(stream);
                    }
                }
            }

            TempData["Message"] = "Files uploaded successfully.";
            return RedirectToAction("Upload", new { id = claim.ClaimId });
        }
        // ----------------- Coordinator Review -----------------
        [HttpGet]
        public IActionResult CoordinatorReview()
        {
            var claims = _claims.Values.ToList();
            var results = new Dictionary<int, string[]>();
            foreach (var c in claims)
            {
                results[c.ClaimId] = ClaimValidator.Validate(c);
            }
            ViewData["ValidationResults"] = results;
            return View(claims);
        }

        [HttpGet]
        public IActionResult CoordinatorApprove(int id)
        {
            if (_claims.ContainsKey(id))
            {
                var claim = _claims[id];
                // Automated verification before approving
                var issues = ClaimValidator.Validate(claim);
                claim.ValidationIssues = issues.ToList();
                if (issues.Length == 0)
                {
                    claim.Status = "CoordinatorApproved";
                    TempData["Message"] = $"Claim {id} automatically approved by Coordinator.";
                }
                else
                {
                    claim.Status = "CoordinatorFlagged";
                    TempData["Error"] = $"Claim {id} flagged: {string.Join("; ", issues)}";
                }
            }
            return RedirectToAction("CoordinatorReview");
        }

        [HttpGet]
        public IActionResult CoordinatorReject(int id)
        {
            if (_claims.ContainsKey(id))
            {
                _claims[id].Status = "CoordinatorRejected";
            }
            return RedirectToAction("CoordinatorReview");
        }

        // ----------------- Manager Approval -----------------
        [HttpGet]
        public IActionResult ManagerApproval()
        {
            var claims = _claims.Values.ToList();
            var results = new Dictionary<int, string[]>();
            foreach (var c in claims)
            {
                results[c.ClaimId] = ClaimValidator.Validate(c);
            }
            ViewData["ValidationResults"] = results;
            return View(claims);
        }

        [HttpGet]
        public IActionResult CoordinatorAutoVerify(int id)
        {
            if (!_claims.ContainsKey(id)) return NotFound();
            var claim = _claims[id];
            var issues = ClaimValidator.Validate(claim);
            claim.ValidationIssues = issues.ToList();
            if (issues.Length == 0)
            {
                // Auto-approve only when there are no validation issues AND total amount is greater than 1500
                if (claim.TotalAmount > 1500m)
                {
                    claim.Status = "CoordinatorApproved";
                    TempData["Message"] = "Claim auto-verified and approved by Coordinator.";
                }
                else
                {
                    TempData["Warning"] = "Claim did not meet auto-verify threshold for Coordinator.";
                }
            }
            else
            {
                TempData["Warning"] = string.Join("; ", issues);
            }
            return RedirectToAction("CoordinatorReview");
        }

        [HttpGet]
        public IActionResult ManagerAutoVerify(int id)
        {
            if (!_claims.ContainsKey(id)) return NotFound();
            var claim = _claims[id];
            var issues = ClaimValidator.Validate(claim);
            claim.ValidationIssues = issues.ToList();
            // Auto-approve only when there are no validation issues AND total amount is greater than 1500
            if (issues.Length == 0 && claim.TotalAmount > 1500m)
            {
                claim.Status = "ManagerApproved";
                TempData["Message"] = "Claim auto-verified and approved by Manager.";
            }
            else
            {
                TempData["Warning"] = string.Join("; ", issues);
            }
            return RedirectToAction("ManagerApproval");
        }

        [HttpGet]
        public IActionResult ManagerApprove(int id)
        {
            if (_claims.ContainsKey(id))
            {
                var claim = _claims[id];
                // Manager-level automated checks (stricter)
                var issues = ClaimValidator.Validate(claim);
                claim.ValidationIssues = issues.ToList();
                if (issues.Length == 0 && claim.TotalAmount <= 10000m)
                {
                    claim.Status = "ManagerApproved";
                    TempData["Message"] = $"Claim {id} approved by Manager.";
                }
                else
                {
                    claim.Status = "ManagerFlagged";
                    TempData["Error"] = $"Claim {id} requires manual review: {string.Join("; ", issues)}";
                }
            }
            return RedirectToAction("ManagerApproval");
        }

        [HttpGet]
        public IActionResult ManagerReject(int id)
        {
            if (_claims.ContainsKey(id))
            {
                _claims[id].Status = "ManagerRejected";
            }
            return RedirectToAction("ManagerApproval");
        }

    }
}
