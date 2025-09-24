using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TitleVerification.Api.Data;
using TitleVerification.Api.Models;
using System.Text.Json;

namespace TitleVerification.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db) { _db = db; }

        [HttpGet("users")] public async Task<IActionResult> GetUsers() => Ok(await _db.Users.ToListAsync());

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id) { var u = await _db.Users.FindAsync(id); if (u == null) return NotFound(); _db.Users.Remove(u); await _db.SaveChangesAsync(); return NoContent(); }

        [HttpGet("documents")] public async Task<IActionResult> GetDocs() => Ok(await _db.Documents.ToListAsync());

        [HttpPost("documents/{id}/approve")]
        public async Task<IActionResult> Approve(int id) { var d = await _db.Documents.FindAsync(id); if (d == null) return NotFound(); d.Status = "Approved"; await _db.SaveChangesAsync(); return Ok(d); }

        [HttpPost("documents/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string reason) { var d = await _db.Documents.FindAsync(id); if (d == null) return NotFound(); d.Status = "Rejected"; d.ExtractedData = JsonSerializer.Serialize(new { reason }); await _db.SaveChangesAsync(); return Ok(d); }

        [HttpPost("land/create")]
        public async Task<IActionResult> CreateLand(LandRecord land) { _db.LandRecords.Add(land); await _db.SaveChangesAsync(); return Ok(land); }
    }
}
