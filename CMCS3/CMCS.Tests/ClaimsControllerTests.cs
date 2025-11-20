using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CMCS.Controllers;
using CMCS.Models;
using Xunit;

namespace CMCS.Tests
{
    public class ClaimsControllerTests
    {
        [Fact]
        public void Submit_Get_ReturnsViewWithNewClaim()
        {
            var controller = new ClaimsController();
            var result = controller.Submit() as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ClaimViewModel;
            Assert.NotNull(model);
            Assert.True(model.ClaimId > 0);
            Assert.NotEmpty(model.Lines);
        }

        [Fact]
        public void Submit_Post_UpdatesExistingClaimAndRedirectsToTrack()
        {
            var controller = new ClaimsController();
            // create initial claim
            var get = controller.Submit() as ViewResult;
            var model = get.Model as ClaimViewModel;
            model.Month = 1;
            // post updated
            var postResult = controller.Submit(model) as RedirectToActionResult;
            Assert.NotNull(postResult);
            Assert.Equal("Track", postResult.ActionName);
        }

        [Fact]
        public void Track_ReturnsListView()
        {
            var controller = new ClaimsController();
            var result = controller.Track() as ViewResult;
            Assert.NotNull(result);
            var list = result.Model as List<ClaimViewModel>;
            Assert.NotNull(list);
        }

        [Fact]
        public void Details_ReturnsNotFoundForMissingId()
        {
            var controller = new ClaimsController();
            var result = controller.Details(9999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Upload_Get_ReturnsNotFoundForMissing()
        {
            var controller = new ClaimsController();
            var result = controller.Upload(9999);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void CoordinatorApprove_SetsStatusAndRedirects()
        {
            var controller = new ClaimsController();
            var get = controller.Submit() as ViewResult;
            var model = get.Model as ClaimViewModel;
            var res = controller.CoordinatorApprove(model.ClaimId) as RedirectToActionResult;
            Assert.Equal("CoordinatorReview", res.ActionName);
            // verify status changed by retrieving track/details
            var details = controller.Details(model.ClaimId) as ViewResult;
            var updated = details.Model as ClaimViewModel;
            Assert.Equal("CoordinatorApproved", updated.Status);
        }

        [Fact]
        public void ManagerApprove_SetsStatusAndRedirects()
        {
            var controller = new ClaimsController();
            var get = controller.Submit() as ViewResult;
            var model = get.Model as ClaimViewModel;
            var res = controller.ManagerApprove(model.ClaimId) as RedirectToActionResult;
            Assert.Equal("ManagerApproval", res.ActionName);
            var details = controller.Details(model.ClaimId) as ViewResult;
            var updated = details.Model as ClaimViewModel;
            Assert.Equal("ManagerApproved", updated.Status);
        }
    }
}
