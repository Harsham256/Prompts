using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TitleVerification.Api.Data;
using TitleVerification.Api.Models;
using TitleVerification.Api.Services;

namespace TitleVerification.Api.Controllers
{
    [ApiController]
    [Route("api/report")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _svc;
        private readonly ApplicationDbContext _db;
        public ReportController(IReportService svc, ApplicationDbContext db) { _svc = svc; _db = db; }

        [Authorize]
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] Dictionary<string, string> extracted)
        {
            var (conds, status, land) = await _svc.EvaluateAsync(extracted);

            var rep = new Report
            {
                LandRecordId = land?.Id,
                ConditionResults = JsonSerializer.Serialize(conds),
                TrafficLightStatus = status,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reports.Add(rep);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                rep.ReportID,
                rep.TrafficLightStatus,
                ConditionResults = conds,
                MatchedLand = land == null ? null : new
                {
                    land.Id,
                    land.LandID,
                    land.Latitude,
                    land.Longitude,
                    land.Name,
                    land.AadhaarNumber,
                    land.OwnershipType,
                    land.SiblingApproval,
                    land.LoanDisputeStatus,
                    land.LandType
                }
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> All() => Ok(await _db.Reports.ToListAsync());
    }
}
