
namespace TitleVerification.Api.Models
{
    public class Document
    {
        public int DocumentID { get; set; }
        public int UserID { get; set; }
        public string FilePath { get; set; } = string.Empty; // stored path or S3 URL
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
        public string ExtractedData { get; set; } = "{}"; // JSON string of extracted fields
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public User? Uploader { get; set; }
    }
}
