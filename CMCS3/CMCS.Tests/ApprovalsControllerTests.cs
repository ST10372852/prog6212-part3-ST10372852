using Microsoft.AspNetCore.Mvc;
using CMCS.Controllers;
using CMCS.Models;
using Xunit;

namespace CMCS.Tests
{
    public class ApprovalsControllerTests
    {
        [Fact]
        public void Index_ReturnsView()
        {
            var controller = new ApprovalsController();
            var result = controller.Index() as ViewResult;
            Assert.NotNull(result);
        }

        [Fact]
        public void Approve_SetsStatusAndRedirects()
        {
            var claimsController = new ClaimsController();
            var get = claimsController.Submit() as ViewResult;
            var model = get.Model as ClaimViewModel;

            var controller = new ApprovalsController();
            var res = controller.Approve(model.ClaimId) as RedirectToActionResult;
            Assert.Equal("Index", res.ActionName);

            // Ensure status changed
            var details = claimsController.Details(model.ClaimId) as ViewResult;
            var updated = details.Model as ClaimViewModel;
            Assert.Equal("Approved", updated.Status);
        }

        [Fact]
        public void Reject_SetsStatusAndRedirects()
        {
            var claimsController = new ClaimsController();
            var get = claimsController.Submit() as ViewResult;
            var model = get.Model as ClaimViewModel;

            var controller = new ApprovalsController();
            var res = controller.Reject(model.ClaimId, "reason") as RedirectToActionResult;
            Assert.Equal("Index", res.ActionName);

            var details = claimsController.Details(model.ClaimId) as ViewResult;
            var updated = details.Model as ClaimViewModel;
            Assert.Equal("Rejected", updated.Status);
        }
    }
}
