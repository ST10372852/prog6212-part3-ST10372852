using CMCS.Models;
using System.Collections.Generic;

namespace CMCS.Services
{
    public static class ClaimValidator
    {
        // Prototype rules - adjust as policy changes
        private const int MaxHoursPerLine = 200;
        private const decimal MinRate = 0m;
        private const decimal MaxRate = 1000m;

        public static string[] Validate(ClaimViewModel claim)
        {
            var issues = new List<string>();

            if (claim == null)
            {
                issues.Add("Claim is null");
                return issues.ToArray();
            }

            if (string.IsNullOrWhiteSpace(claim.LecturerName))
            {
                issues.Add("Lecturer name missing");
            }

            if (claim.Lines == null || claim.Lines.Count == 0)
            {
                issues.Add("No claim lines provided");
            }
            else
            {
                for (int i = 0; i < claim.Lines.Count; i++)
                {
                    var line = claim.Lines[i];
                    if (line == null)
                    {
                        issues.Add($"Line {i+1}: empty");
                        continue;
                    }

                    if (line.Hours < 0 || line.Hours > MaxHoursPerLine)
                    {
                        issues.Add($"Line {i+1}: Hours must be between 0 and {MaxHoursPerLine}");
                    }

                    if (line.Rate < MinRate || line.Rate > MaxRate)
                    {
                        issues.Add($"Line {i+1}: Rate must be between {MinRate} and {MaxRate}");
                    }
                }
            }

            // Example policy: claims over a threshold require manager review
            if (claim.TotalAmount > 5000m)
            {
                issues.Add("Total amount exceeds normal threshold and requires manager review");
            }

            return issues.ToArray();
        }
    }
}
