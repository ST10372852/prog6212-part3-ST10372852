using Microsoft.AspNetCore.Mvc;
using CMCS.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            if (_claims.ContainsKey(model.ClaimId))
            {
                _claims[model.ClaimId] = model;
            }

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
            return View(_claims.Values.ToList());
        }

        [HttpGet]
        public IActionResult CoordinatorApprove(int id)
        {
            if (_claims.ContainsKey(id))
            {
                _claims[id].Status = "CoordinatorApproved";
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
            return View(_claims.Values.ToList());
        }

        [HttpGet]
        public IActionResult ManagerApprove(int id)
        {
            if (_claims.ContainsKey(id))
            {
                _claims[id].Status = "ManagerApproved";
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
