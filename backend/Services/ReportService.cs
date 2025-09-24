using TitleVerification.Api.Data;
using TitleVerification.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace TitleVerification.Api.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IGeoService _geo;

        public ReportService(ApplicationDbContext db, IGeoService geo) { _db = db; _geo = geo; }

        public async Task<(Dictionary<string, string>, string, LandRecord?)> EvaluateAsync(Dictionary<string, string> extracted)
        {
            var conditions = new Dictionary<string, string>();
            string status = "Green";

            LandRecord? matched = await _db.LandRecords.FirstOrDefaultAsync(l => l.LandID == extracted.GetValueOrDefault("LandId"));

            if (matched == null && extracted.TryGetValue("Address", out var addr) && addr != "NotFound")
            {
                var coords = await _geo.GeocodeAddressAsync(addr);
                if (coords.HasValue)
                    matched = await _db.LandRecords.FirstOrDefaultAsync(l => Math.Abs(l.Latitude - coords.Value.lat) < 0.0005 && Math.Abs(l.Longitude - coords.Value.lon) < 0.0005);
            }

            if (matched == null) { status = "Yellow"; return (conditions, status, null); }

            conditions["NameMatch"] = matched.Name.Equals(extracted["Name"], StringComparison.OrdinalIgnoreCase) ? "Met" : "Failed";
            conditions["AadhaarMatch"] = matched.AadhaarNumber == extracted["AadhaarNumber"] ? "Met" : "Failed";
            conditions["LandIdMatch"] = matched.LandID == extracted["LandId"] ? "Met" : "Failed";
            conditions["SiblingApproval"] = matched.OwnershipType == "Inherited" ? (matched.SiblingApproval ? "Met" : "Failed") : "NotApplicable";
            conditions["LoanDispute"] = matched.LoanDisputeStatus ? "Failed" : "Met";
            conditions["RestrictedType"] = new[] { "Forest", "Military", "Government" }.Contains(matched.LandType) ? "Failed" : "Met";

            if (conditions.Values.Contains("Failed")) status = "Red";

            return (conditions, status, matched);
        }
    }
}
