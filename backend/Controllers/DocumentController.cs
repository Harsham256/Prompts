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
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IDocumentParser _parser;

        public DocumentController(ApplicationDbContext db, IDocumentParser parser)
        {
            _db = db;
            _parser = parser;
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var extracted = _parser.Parse(ms.ToArray(), file.ContentType);

            var userId = int.Parse(User.Claims.First(c => c.Type == "sub").Value);

            var doc = new Document
            {
                UserID = userId,
                FilePath = file.FileName,
                ExtractedData = JsonSerializer.Serialize(extracted),
                Status = "Pending",
                UploadedAt = DateTime.UtcNow
            };

            _db.Documents.Add(doc);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Document uploaded successfully",
                documentId = doc.DocumentID,
                status = doc.Status,
                extracted
            });
        }

        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> MyDocuments()
        {
            var userId = int.Parse(User.Claims.First(c => c.Type == "sub").Value);

            var docs = await _db.Documents
                .Where(d => d.UserID == userId)
                .OrderByDescending(d => d.UploadedAt)
                .ToListAsync();

            return Ok(docs);
        }
    }
}

