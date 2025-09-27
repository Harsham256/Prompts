//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.StaticFiles;
//using TitleVerification.Api.Data;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DocumentController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly IWebHostEnvironment _env;
//        private readonly ILogger<DocumentController> _logger;

//        public DocumentController(ApplicationDbContext context, IWebHostEnvironment env, ILogger<DocumentController> logger)
//        {
//            _context = context;
//            _env = env;
//            _logger = logger;
//        }

//        // POST /api/document/upload
//        [HttpPost("upload")]
//        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int userId)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded");

//            // sanitize filename and ensure directory
//            var originalFileName = Path.GetFileName(file.FileName ?? "upload");
//            var safeFileName = $"{Guid.NewGuid()}_{originalFileName}";
//            var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//            var uploadPath = Path.Combine(rootPath, "Uploads");

//            try
//            {
//                if (!Directory.Exists(uploadPath))
//                    Directory.CreateDirectory(uploadPath);

//                var filePath = Path.Combine(uploadPath, safeFileName);
//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await file.CopyToAsync(stream);
//                }

//                var document = new Document
//                {
//                    UserId = userId,
//                    FilePath = safeFileName,
//                    Status = "Pending",
//                    UploadedAt = DateTime.UtcNow
//                };

//                _context.Documents.Add(document);
//                await _context.SaveChangesAsync();

//                return Ok(new { message = "✅ File uploaded successfully", documentId = document.DocumentID });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error uploading file for user {UserId}", userId);
//                return StatusCode(500, $"Internal Server Error: {ex.Message}");
//            }
//        }

//        // GET /api/document/view/{id}
//        [HttpGet("view/{id}")]
//        public async Task<IActionResult> ViewDocument(int id)
//        {
//            try
//            {
//                var document = await _context.Documents.FirstOrDefaultAsync(d => d.DocumentID == id);
//                if (document == null)
//                    return NotFound("Document not found");

//                var rootPath = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
//                var filePath = Path.Combine(rootPath, "Uploads", document.FilePath);

//                if (!System.IO.File.Exists(filePath))
//                {
//                    _logger.LogWarning("Requested file missing on disk: {FilePath} (DocumentId={DocumentId})", filePath, id);
//                    return NotFound("File not found");
//                }

//                var provider = new FileExtensionContentTypeProvider();
//                if (!provider.TryGetContentType(filePath, out var contentType))
//                {
//                    contentType = "application/octet-stream";
//                }

//                var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
//                // Setting a filename in the returned file helps downloads/opening
//                return File(fileBytes, contentType, document.FilePath);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error while streaming document {DocumentId}", id);
//                return StatusCode(500, $"Internal Server Error: {ex.Message}");
//            }
//        }

//        // GET /api/document/my?userId=123
//        [HttpGet("my")]
//        public async Task<IActionResult> GetUserDocuments([FromQuery] int userId)
//        {
//            var documents = await _context.Documents
//                .Where(d => d.UserId == userId)
//                .Select(d => new
//                {
//                    documentID = d.DocumentID,
//                    filePath = d.FilePath,
//                    status = d.Status,
//                    uploadedAt = d.UploadedAt
//                })
//                .ToListAsync();

//            return Ok(documents);
//        }
//    }
//}

//using System.Drawing;
//using System.Text;
//using System.Text.RegularExpressions;
//using Tesseract;
//using UglyToad.PdfPig;

//namespace TitleVerification.Api.Services
//{
//    public class DocumentService : IDocumentService
//    {
//        // FIXED: regex should look for LAND-<digits>
//        private static readonly Regex LandIdRegex = new Regex(@"LAND-\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);

//        private readonly string _tessDataPath;
//        private readonly string _tessLang;

//        public DocumentService() : this(Path.Combine(AppContext.BaseDirectory, "tessdata"), "eng") { }

//        public DocumentService(string tessDataPath, string tessLang)
//        {
//            _tessDataPath = tessDataPath;
//            _tessLang = tessLang;
//        }

//        public string ExtractLandId(byte[] fileBytes, string contentType)
//        {
//            string text;

//            if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
//                text = ExtractFromPdf(fileBytes);
//            else if (contentType.Contains("text", StringComparison.OrdinalIgnoreCase))
//                text = Encoding.UTF8.GetString(fileBytes);
//            else
//                throw new NotSupportedException($"Unsupported content type: {contentType}");

//            if (string.IsNullOrWhiteSpace(text))
//                return "NOT_FOUND";

//            var match = LandIdRegex.Match(text);
//            return match.Success ? match.Value : "NOT_FOUND";
//        }

//        private string ExtractFromPdf(byte[] fileBytes)
//        {
//            var sb = new StringBuilder();

//            using var ms = new MemoryStream(fileBytes);
//            using var pdf = PdfDocument.Open(ms);

//            foreach (var page in pdf.GetPages())
//            {
//                if (!string.IsNullOrWhiteSpace(page.Text))
//                    sb.AppendLine(page.Text);
//            }

//            if (sb.Length > 0)
//                return sb.ToString();

//            // fallback OCR
//            return ExtractFromPdfWithOcr(fileBytes);
//        }

//        private string ExtractFromPdfWithOcr(byte[] fileBytes)
//        {
//            var sb = new StringBuilder();

//            using var engine = new TesseractEngine(_tessDataPath, _tessLang, EngineMode.Default);
//            using var ms = new MemoryStream(fileBytes);
//            using var pdf = PdfDocument.Open(ms);

//            foreach (var page in pdf.GetPages())
//            {
//                var images = page.GetImages();
//                foreach (var img in images)
//                {
//                    if (!img.TryGetPng(out var png)) continue;

//                    using var imgStream = new MemoryStream(png.ToArray());
//                    using var bmp = new Bitmap(imgStream);

//                    using var pix = BitmapToPix(bmp);

//                    using var ocrPage = engine.Process(pix);
//                    sb.AppendLine(ocrPage.GetText());
//                }
//            }

//            return sb.ToString();
//        }

//        private Pix BitmapToPix(Bitmap bitmap)
//        {
//            using var ms = new MemoryStream();
//            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//            ms.Position = 0;
//            return Pix.LoadFromMemory(ms.ToArray());
//        }
//    }
//}



//using Microsoft.EntityFrameworkCore;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options) { }

//        public DbSet<User> Users { get; set; } = null!;
//        public DbSet<LandRecord> LandRecords { get; set; } = null!;
//        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;
//        public DbSet<Document> Documents { get; set; } = null!;

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            modelBuilder.Entity<Document>()
//                .HasOne(d => d.User)
//                .WithMany(u => u.Documents)
//                .HasForeignKey(d => d.UserId);

//            modelBuilder.Entity<LandRecord>().HasData(
//                new LandRecord
//                {
//                    Id = 1,
//                    LandId = "LAND123",
//                    OwnerName = "John Doe",
//                    OwnershipType = "Self",
//                    LandType = "Agriculture",
//                    HasOngoingLoan = false,
//                    HasDispute = false
//                }
//            );
//        }
//    }
//}



//using TitleVerification.Api.Models;

//public class Document
//{
//    public int DocumentID { get; set; }
//    public int UserId { get; set; }

//    public string FilePath { get; set; } = string.Empty;
//    public string Status { get; set; } = "Pending";
//    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
//    public string? ExtractedLandId { get; set; }

//    // Navigation property
//    public User User { get; set; } = null!;
//}


//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using TitleVerification.Api.Data;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Controllers
//{
//    [ApiController]
//    [Route("api/admin")]
//    public class AdminController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILogger<AdminController> _logger;

//        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        [HttpGet("users")]
//        public async Task<IActionResult> GetUsers()
//        {
//            var users = await _context.Users
//                .Select(u => new
//                {
//                    userID = u.Id,
//                    name = u.Name,
//                    username = u.Username,
//                    aadhaarNumber = u.AadhaarNumber,
//                    address = u.Address
//                })
//                .ToListAsync();

//            return Ok(users);
//        }

//        [HttpGet("documents")]
//        public async Task<IActionResult> GetDocuments()
//        {
//            try
//            {
//                var docs = await _context.Documents
//                    .Include(d => d.User)
//                    .Select(d => new
//                    {
//                        documentID = d.DocumentID,
//                        userID = d.UserId,
//                        userName = d.User != null ? d.User.Name : "Unknown",
//                        filePath = d.FilePath,
//                        status = d.Status,
//                        uploadedAt = d.UploadedAt
//                    })
//                    .OrderByDescending(d => d.uploadedAt)
//                    .ToListAsync();

//                return Ok(docs);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error fetching documents for admin");
//                return StatusCode(500, "Internal Server Error");
//            }
//        }

//        [HttpPost("documents/{id}/approve")]
//        public async Task<IActionResult> ApproveDocument(int id)
//        {
//            var doc = await _context.Documents.FindAsync(id);
//            if (doc == null) return NotFound();

//            doc.Status = "Approved";
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Approved" });
//        }

//        [HttpPost("documents/{id}/reject")]
//        public async Task<IActionResult> RejectDocument(int id)
//        {
//            var doc = await _context.Documents.FindAsync(id);
//            if (doc == null) return NotFound();

//            doc.Status = "Rejected";
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Rejected" });
//        }

//        [HttpDelete("users/{id}")]
//        public async Task<IActionResult> DeleteUser(int id)
//        {
//            var user = await _context.Users.FindAsync(id);
//            if (user == null) return NotFound();

//            _context.Users.Remove(user);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "User removed" });
//        }
//    }
//}

