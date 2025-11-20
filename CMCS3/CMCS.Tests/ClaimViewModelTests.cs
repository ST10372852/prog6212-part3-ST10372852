using System.Collections.Generic;
using CMCS.Models;
using Xunit;

namespace CMCS.Tests
{
    public class ClaimViewModelTests
    {
        [Fact]
        public void TotalAmount_ComputesSumOfLineTotals()
        {
            var model = new ClaimViewModel
            {
                Lines = new List<ClaimLineViewModel>
                {
                    new ClaimLineViewModel { Hours = 2, Rate = 10m },
                    new ClaimLineViewModel { Hours = 3, Rate = 20m }
                }
            };

            Assert.Equal(2*10m + 3*20m, model.TotalAmount);
        }

        [Fact]
        public void ClaimID_Alias_ReturnsSameAsClaimId()
        {
            var model = new ClaimViewModel { ClaimId = 42 };
            Assert.Equal(42, model.ClaimID);
        }
    }
}
