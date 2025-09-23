//// File: Controllers\AuthController.cs
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Migrations;
//using Microsoft.IdentityModel.Tokens;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
//using System.Text.RegularExpressions;
//using Tesseract;
//using TitleVerification.Api.Data;
//using TitleVerification.Api.DTOs.Auth;
//using TitleVerification.Api.DTOs.Loan;
//using TitleVerification.Api.Helpers;
//using TitleVerification.Api.Models;
//using TitleVerification.Api.Services;
//using UglyToad.PdfPig;

//namespace TitleVerification.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class AuthController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly JwtSettings _jwt;

//        public AuthController(ApplicationDbContext context, JwtSettings jwt)
//        {
//            _context = context;
//            _jwt = jwt;
//        }

//        [HttpPost("register")]
//        public IActionResult Register([FromBody] RegisterRequest request)
//        {
//            var user = new User
//            {
//                FullName = request.FullName,
//                Email = request.Email,
//                PasswordHash = request.Password, // replace with hash in production
//                Aadhaar = request.Aadhaar,
//                Pan = request.Pan,
//                LandId = request.LandId,
//                Latitude = request.Latitude,
//                Longitude = request.Longitude,
//                Role = UserRole.User
//            };

//            _context.Users.Add(user);
//            _context.SaveChanges();

//            // Return fixed token for now
//            var fixedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."; // full token
//            return Ok(new { token = fixedToken });
//        }
//    }
//}

//// File: Controllers\DocumentController.cs
//using Microsoft.AspNetCore.Mvc;
//using TitleVerification.Api.Services;
//using System.IO;
//using System.Threading.Tasks;

//namespace TitleVerification.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DocumentController : ControllerBase
//    {
//        private readonly IDocumentService _documentService;

//        public DocumentController(IDocumentService documentService)
//        {
//            _documentService = documentService;
//        }

//        [HttpPost("extract")]
//        public async Task<IActionResult> ExtractLandId(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("No file uploaded.");

//            using var ms = new MemoryStream();
//            await file.CopyToAsync(ms);
//            var fileBytes = ms.ToArray();

//            var landId = _documentService.ExtractLandId(fileBytes, file.ContentType);

//            return Ok(new { LandId = landId });
//        }
//    }
//}

//// File: Controllers\LoanController.cs
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using TitleVerification.Api.Data;
//using TitleVerification.Api.DTOs.Loan;
//using TitleVerification.Api.Models;
//using TitleVerification.Api.Services;

//namespace TitleVerification.Api.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class LoanController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;
//        private readonly ILoanSanctionService _loanService;

//        public LoanController(ApplicationDbContext context, ILoanSanctionService loanService)
//        {
//            _context = context;
//            _loanService = loanService;
//        }

//        [HttpPost("apply")]
//        public async Task<IActionResult> ApplyLoan([FromBody] LoanRequest request)
//        {
//            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
//            if (user == null) return BadRequest(new { message = "User not found" });

//            var land = await _context.LandRecords.FirstOrDefaultAsync(l => l.LandId == request.LandId);
//            if (land == null) return BadRequest(new { message = "Land record not found" });

//            var result = _loanService.EvaluateLoan(request, land, user);

//            var loanApp = new LoanApplication
//            {
//                UserId = user.Id,
//                LandId = land.LandId,
//                Status = result.Status,
//                DecisionReason = result.Reason
//            };

//            _context.LoanApplications.Add(loanApp);
//            await _context.SaveChangesAsync();

//            return Ok(result);
//        }
//    }
//}

//// File: Data\ApplicationDbContext.cs
//using Microsoft.EntityFrameworkCore;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Data
//{
//    public class ApplicationDbContext : DbContext
//    {
//        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//            : base(options)
//        {
//        }

//        public DbSet<User> Users { get; set; } = null!;
//        public DbSet<LandRecord> LandRecords { get; set; } = null!;
//        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

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

//// File: DTOs\Auth\LoginRequest.cs
//namespace TitleVerification.Api.DTOs.Auth
//{
//    public class LoginRequest
//    {
//        public string Email { get; set; } = string.Empty;
//        public string Password { get; set; } = string.Empty;
//    }
//}

//// File: DTOs\Auth\RegisterRequest.cs
//namespace TitleVerification.Api.DTOs.Auth
//{
//    public class RegisterRequest
//    {
//        public string FullName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string Password { get; set; } = string.Empty;
//        public string Aadhaar { get; set; } = string.Empty;
//        public string Pan { get; set; } = string.Empty;
//        public string LandId { get; set; } = string.Empty;
//        public double Latitude { get; set; }
//        public double Longitude { get; set; }
//    }
//}

//// File: DTOs\Loan\LoanRequest.cs
//namespace TitleVerification.Api.DTOs.Loan
//{
//    public class LoanRequest
//    {
//        public string LandId { get; set; } = string.Empty;
//        public string OwnerName { get; set; } = string.Empty;
//        public bool HasSiblingApproval { get; set; }
//        public int UserId { get; set; }
//    }
//}

//// File: DTOs\Loan\LoanResponse.cs
//namespace TitleVerification.Api.DTOs.Loan
//{
//    public class LoanResponse
//    {
//        public string Status { get; set; } = string.Empty;
//        public string Reason { get; set; } = string.Empty;
//    }
//}

//// File: Helpers\GeoApiClient.cs
//namespace TitleVerification.Api.Helpers
//{
//    public class GeoApiClient
//    {
//        public bool ValidateLocation(double latitude, double longitude)
//        {
//            return true;
//        }
//    }
//}

//// File: Helpers\JwtSettings.cs
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//namespace TitleVerification.Api.Helpers
//{
//    public class JwtSettings
//    {
//        public string Key { get; set; } = string.Empty;
//        public string Issuer { get; set; } = string.Empty;
//        public string Audience { get; set; } = string.Empty;
//        public int ExpiryMinutes { get; set; } = 60;

//        public string GenerateToken(int userId, string email)
//        {
//            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Key));
//            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

//            var claims = new[]
//            {
//                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
//                new Claim(JwtRegisteredClaimNames.Email, email),
//                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//            };

//            var token = new JwtSecurityToken(
//                Issuer,
//                Audience,
//                claims,
//                expires: DateTime.UtcNow.AddMinutes(ExpiryMinutes),
//                signingCredentials: credentials
//            );

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }
//    }
//}

//// File: Helpers\PDFParser.cs
//using System.Text;
//using UglyToad.PdfPig;
//using UglyToad.PdfPig.Content;

//public class PDFParser
//{
//    public string ExtractText(byte[] pdfBytes)
//    {
//        using var memoryStream = new MemoryStream(pdfBytes);
//        using var document = PdfDocument.Open(memoryStream);

//        var text = new StringBuilder();

//        foreach (Page page in document.GetPages())
//        {
//            text.AppendLine(page.Text);
//        }

//        return text.ToString();
//    }
//}

//// File: Migrations\20250920161539_InitialCreate.cs
//using Microsoft.EntityFrameworkCore.Migrations;

//#nullable disable

//namespace TitleVerification.Api.Migrations
//{
//    public partial class InitialCreate : Migration
//    {
//        protected override void Up(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.CreateTable(
//                name: "LandRecords",
//                columns: table => new
//                {
//                    Id = table.Column<int>(type: "int", nullable: false)
//                        .Annotation("SqlServer:Identity", "1, 1"),
//                    LandId = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    OwnerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    OwnershipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    LandType = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    HasOngoingLoan = table.Column<bool>(type: "bit", nullable: false),
//                    HasDispute = table.Column<bool>(type: "bit", nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_LandRecords", x => x.Id);
//                });

//            migrationBuilder.CreateTable(
//                name: "Users",
//                columns: table => new
//                {
//                    Id = table.Column<int>(type: "int", nullable: false)
//                        .Annotation("SqlServer:Identity", "1, 1"),
//                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Aadhaar = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Pan = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    LandId = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Latitude = table.Column<double>(type: "float", nullable: false),
//                    Longitude = table.Column<double>(type: "float", nullable: false),
//                    Role = table.Column<int>(type: "int", nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_Users", x => x.Id);
//                });

//            migrationBuilder.CreateTable(
//                name: "LoanApplications",
//                columns: table => new
//                {
//                    Id = table.Column<int>(type: "int", nullable: false)
//                        .Annotation("SqlServer:Identity", "1, 1"),
//                    UserId = table.Column<int>(type: "int", nullable: false),
//                    LandId = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
//                    DecisionReason = table.Column<string>(type: "nvarchar(max)", nullable: false)
//                },
//                constraints: table =>
//                {
//                    table.PrimaryKey("PK_LoanApplications", x => x.Id);
//                    table.ForeignKey(
//                        name: "FK_LoanApplications_Users_UserId",
//                        column: x => x.UserId,
//                        principalTable: "Users",
//                        principalColumn: "Id",
//                        onDelete: ReferentialAction.Cascade);
//                });

//            migrationBuilder.InsertData(
//                table: "LandRecords",
//                columns: new[] { "Id", "HasDispute", "HasOngoingLoan", "LandId", "LandType", "OwnerName", "OwnershipType" },
//                values: new object[] { 1, false, false, "LAND123", "Agriculture", "John Doe", "Self" });

//            migrationBuilder.CreateIndex(
//                name: "IX_LoanApplications_UserId",
//                table: "LoanApplications",
//                column: "UserId");
//        }

//        protected override void Down(MigrationBuilder migrationBuilder)
//        {
//            migrationBuilder.DropTable(
//                name: "LandRecords");

//            migrationBuilder.DropTable(
//                name: "LoanApplications");

//            migrationBuilder.DropTable(
//                name: "Users");
//        }
//    }
//}

//// File: Models\LandRecord.cs
//namespace TitleVerification.Api.Models
//{
//    public class LandRecord
//    {
//        public int Id { get; set; }
//        public string LandId { get; set; } = string.Empty;
//        public string OwnerName { get; set; } = string.Empty;
//        public string OwnershipType { get; set; } = string.Empty;
//        public string LandType { get; set; } = string.Empty;
//        public bool HasOngoingLoan { get; set; }
//        public bool HasDispute { get; set; }
//    }
//}

//// File: Models\LoanApplication.cs
//namespace TitleVerification.Api.Models
//{
//    public class LoanApplication
//    {
//        public int Id { get; set; }
//        public int UserId { get; set; }
//        public string LandId { get; set; } = string.Empty;
//        public string Status { get; set; } = string.Empty;
//        public string DecisionReason { get; set; } = string.Empty;
//    }
//}

//// File: Models\User.cs
//using System.Collections.Generic;

//namespace TitleVerification.Api.Models
//{
//    public enum UserRole
//    {
//        User,
//        Admin
//    }

//    public class User
//    {
//        public int Id { get; set; }
//        public string FullName { get; set; } = string.Empty;
//        public string Email { get; set; } = string.Empty;
//        public string PasswordHash { get; set; } = string.Empty;
//        public string Aadhaar { get; set; } = string.Empty;
//        public string Pan { get; set; } = string.Empty;
//        public string LandId { get; set; } = string.Empty;
//        public double Latitude { get; set; }
//        public double Longitude { get; set; }
//        public UserRole Role { get; set; } = UserRole.User;

//        public List<LoanApplication> LoanApplications { get; set; } = new();
//    }
//}

//// File: Program.cs
//using Microsoft.EntityFrameworkCore;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using TitleVerification.Api.Data;
//using TitleVerification.Api.Helpers;
//using TitleVerification.Api.Services;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.Configure<JwtSettings>(
//    builder.Configuration.GetSection("JwtSettings")
//);

//var jwtSettings = new JwtSettings
//{
//    Key = builder.Configuration["JwtSettings:Key"] ?? string.Empty,
//    Issuer = builder.Configuration["JwtSettings:Issuer"] ?? string.Empty,
//    Audience = builder.Configuration["JwtSettings:Audience"] ?? string.Empty,
//    ExpiryMinutes = int.TryParse(builder.Configuration["JwtSettings:ExpiryMinutes"], out var minutes) ? minutes : 60
//};

//builder.Services.AddSingleton(jwtSettings);

//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);

//builder.Services.AddScoped<IDocumentService, DocumentService>();
//builder.Services.AddScoped<IAadhaarPanService, AadhaarPanService>();
//builder.Services.AddScoped<IGeoLocationService, GeoLocationService>();
//builder.Services.AddScoped<ILoanSanctionService, LoanSanctionService>();

//builder.Services.AddSingleton<PDFParser>();
//builder.Services.AddSingleton<GeoApiClient>();

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyHeader()
//              .AllowAnyMethod();
//    });
//});

//var key = Encoding.ASCII.GetBytes(jwtSettings.Key);
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtSettings.Issuer,
//        ValidAudience = jwtSettings.Audience,
//        IssuerSigningKey = new SymmetricSecurityKey(key)
//    };
//});

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//var app = builder.Build();

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();
//app.UseCors("AllowAll");
//app.UseAuthentication();
//app.UseAuthorization();
//app.MapControllers();
//app.Run();

//// File: Services\IDocumentService.cs
//namespace TitleVerification.Api.Services
//{
//    public interface IDocumentService
//    {
//        string ExtractLandId(byte[] fileBytes, string contentType);
//    }
//}

//// File: Services\DocumentService.cs
//using System;
//using System.Drawing;
//using System.Drawing.Imaging;
//using System.IO;
//using System.Text;
//using System.Text.RegularExpressions;
//using UglyToad.PdfPig;
//using Tesseract;
//using PdfPage = UglyToad.PdfPig.Content.Page;

//namespace TitleVerification.Api.Services
//{
//    public class DocumentService : IDocumentService
//    {
//        public string ExtractLandId(byte[] fileBytes, string contentType)
//        {
//            try
//            {
//                if (contentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
//                    return ExtractFromPdf(fileBytes);

//                if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase) ||
//                    contentType.Contains("jpeg", StringComparison.OrdinalIgnoreCase) ||
//                    contentType.Contains("png", StringComparison.OrdinalIgnoreCase))
//                    return ExtractFromImage(fileBytes);

//                return "Unsupported Format";
//            }
//            catch (Exception ex)
//            {
//                return $"Error processing file: {ex.Message}";
//            }
//        }

//        private string ExtractFromPdf(byte[] pdfBytes)
//        {
//            try
//            {
//                using var ms = new MemoryStream(pdfBytes);
//                using var pdf = PdfDocument.Open(ms);
//                var allText = new StringBuilder();

//                foreach (PdfPage page in pdf.GetPages())
//                {
//                    allText.AppendLine(page.Text);
//                }

//                return ExtractLandIdFromText(allText.ToString());
//            }
//            catch (Exception ex)
//            {
//                return $"PDF parsing failed: {ex.Message}";
//            }
//        }

//        private string ExtractFromImage(byte[] imageBytes)
//        {
//            try
//            {
//                using var bitmap = new Bitmap(new MemoryStream(imageBytes));
//                using var gray = new Bitmap(bitmap.Width, bitmap.Height);
//                using (var g = Graphics.FromImage(gray))
//                {
//                    var cm = new ColorMatrix(
//                        new float[][]
//                        {
//                            new float[] {0.3f,0.3f,0.3f,0,0},
//                            new float[] {0.59f,0.59f,0.59f,0,0},
//                            new float[] {0.11f,0.11f,0.11f,0,0},
//                            new float[] {0,0,0,1,0},
//                            new float[] {0,0,0,0,1}
//                        });
//                    var ia = new ImageAttributes();
//                    ia.SetColorMatrix(cm);
//                    g.DrawImage(bitmap, new Rectangle(0, 0, gray.Width, gray.Height),
//                        0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, ia);
//                }

//                using var ms = new MemoryStream();
//                gray.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
//                ms.Position = 0;

//                using var pixImage = Pix.LoadFromMemory(ms.ToArray());
//                using var ocrEngine = new TesseractEngine("./tessdata_best", "eng", EngineMode.Default);
//                using var page = ocrEngine.Process(pixImage);

//                string extractedText = page.GetText();
//                Console.WriteLine("OCR Output: " + extractedText);

//                return ExtractLandIdFromText(extractedText);
//            }
//            catch (Exception ex)
//            {
//                return $"OCR failed: {ex.Message}";
//            }
//        }

//        private string ExtractLandIdFromText(string text)
//        {
//            if (string.IsNullOrWhiteSpace(text))
//                return "NotFound";

//            var match = Regex.Match(text, @"(?:L[A-Z0-9]{3}[-]?\d{5}|\b\d{5}\b)", RegexOptions.IgnoreCase);

//            return match.Success ? match.Value : "NotFound";
//        }
//    }
//}

//// File: Services\AadhaarPanService.cs
//namespace TitleVerification.Api.Services
//{
//    public class AadhaarPanService : IAadhaarPanService
//    {
//        public bool ValidateAadhaar(string aadhaar)
//        {
//            return !string.IsNullOrEmpty(aadhaar) && aadhaar.Length == 12;
//        }

//        public bool ValidatePan(string pan)
//        {
//            return !string.IsNullOrEmpty(pan) && pan.Length == 10;
//        }
//    }
//}

//// File: Services\GeoLocationService.cs
//using TitleVerification.Api.Helpers;

//namespace TitleVerification.Api.Services
//{
//    public class GeoLocationService : IGeoLocationService
//    {
//        private readonly GeoApiClient _client;

//        public GeoLocationService(GeoApiClient client)
//        {
//            _client = client;
//        }

//        public bool ValidateLandCoordinates(double latitude, double longitude)
//        {
//            return _client.ValidateLocation(latitude, longitude);
//        }
//    }
//}

//// File: Services\IGeoLocationService.cs
//namespace TitleVerification.Api.Services
//{
//    public interface IGeoLocationService
//    {
//        bool ValidateLandCoordinates(double latitude, double longitude);
//    }
//}

//// File: Services\ILoanSanctionService.cs
//using TitleVerification.Api.DTOs.Loan;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Services
//{
//    public interface ILoanSanctionService
//    {
//        LoanResponse EvaluateLoan(LoanRequest request, LandRecord landRecord, User user);
//    }
//}

//// File: Services\LoanSanctionService.cs
//using TitleVerification.Api.DTOs.Loan;
//using TitleVerification.Api.Models;

//namespace TitleVerification.Api.Services
//{
//    public class LoanSanctionService : ILoanSanctionService
//    {
//        public LoanResponse EvaluateLoan(LoanRequest request, LandRecord landRecord, User user)
//        {
//            var reasons = new List<string>();

//            if (!string.Equals(request.OwnerName, landRecord.OwnerName, StringComparison.OrdinalIgnoreCase))
//                reasons.Add("Owner name mismatch");

//            if (landRecord.OwnershipType.Equals("Inherited", StringComparison.OrdinalIgnoreCase) &&
//                !request.HasSiblingApproval)
//                reasons.Add("Sibling approval required for inherited land");

//            if (landRecord.HasOngoingLoan)
//                reasons.Add("Land has ongoing loan");

//            var unauthorizedTypes = new[] { "Forest", "Military", "Government" };
//            if (unauthorizedTypes.Contains(landRecord.LandType))
//                reasons.Add($"Land type '{landRecord.LandType}' is unauthorized");

//            if (landRecord.HasDispute)
//                reasons.Add("Land has legal disputes/encumbrances");

//            string decision = reasons.Count == 0 ? "Approved" : "Rejected";

//            return new LoanResponse
//            {
//                Status = decision,
//                Reason = reasons.Count == 0 ? "All checks passed" : string.Join("; ", reasons)
//            };
//        }
//    }
//}

//// File: appsettings.json
///*
//{
//  "Logging": {
//    "LogLevel": {
//      "Default": "Information",
//      "Microsoft.AspNetCore": "Warning"
//    }
//  },
//  "AllowedHosts": "*",
//  "JwtSettings": {
//    "Key": "your_secret_key_here",
//    "Issuer": "TitleVerification.Api",
//    "Audience": "TitleVerification.Client",
//    "ExpiryMinutes": 60
//  },
//  "ConnectionStrings": {
//    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TitleVerificationDb;Trusted_Connection=True;MultipleActiveResultSets=true"
//  }
//}
//*/

//// File: appsettings.Development.json
///*
//{
//  "Logging": {
//    "LogLevel": {
//      "Default": "Debug",
//      "Microsoft.AspNetCore": "Debug"
//    }
//  }
//}
//*/

//// File: Properties\launchSettings.json
///*
//{
//  "profiles": {
//    "TitleVerification.Api": {
//      "commandName": "Project",
//      "dotnetRunMessages": true,
//      "launchBrowser": true,
//      "applicationUrl": "https://localhost:5001;http://localhost:5000",
//      "environmentVariables": {
//        "ASPNETCORE_ENVIRONMENT": "Development"
//      }
//    }
//  }
//}
//*/