using CMCS.Models;
using Xunit;

namespace CMCS.Tests
{
    public class ClaimLineViewModelTests
    {
        [Fact]
        public void LineTotal_CalculatesHoursTimesRate()
        {
            var line = new ClaimLineViewModel { Hours = 5, Rate = 12.5m };
            Assert.Equal(62.5m, line.LineTotal);
        }

        [Fact]
        public void Description_Alias_MapsToModule()
        {
            var line = new ClaimLineViewModel { Module = "Math" };
            Assert.Equal("Math", line.Description);
            line.Description = "Science";
            Assert.Equal("Science", line.Module);
        }
    }
}
