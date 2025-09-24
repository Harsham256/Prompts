using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using Tesseract;

namespace TitleVerification.Api.Services
{
    public class DocumentParser : IDocumentParser
    {
        public Dictionary<string, string> Parse(byte[] fileBytes, string contentType)
        {
            string text = contentType.Contains("pdf") ? ExtractFromPdf(fileBytes) : ExtractFromImage(fileBytes);

            var result = new Dictionary<string, string> { ["RawText"] = text };

            result["Name"] = Regex.Match(text, @"Name[:\s]*([A-Z][A-Za-z\s]{2,})", RegexOptions.IgnoreCase).Groups[1].Value ?? "NotFound";
            result["AadhaarNumber"] = Regex.Match(text, @"\b(\d{12})\b").Groups[1].Value ?? "NotFound";
            result["LandId"] = Regex.Match(text, @"LAND\d+", RegexOptions.IgnoreCase).Value ?? "NotFound";
            result["FathersName"] = Regex.Match(text, @"Father'?s\s*Name[:\s]*([A-Z][A-Za-z\s]{2,})", RegexOptions.IgnoreCase).Groups[1].Value ?? "NotFound";
            result["Address"] = Regex.Match(text, @"Address[:\s]*(.+)", RegexOptions.IgnoreCase).Groups[1].Value ?? "NotFound";

            return result;
        }

        private string ExtractFromPdf(byte[] pdfBytes)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var pdf = PdfDocument.Open(ms);
            var sb = new StringBuilder();
            foreach (var pg in pdf.GetPages()) sb.AppendLine(pg.Text);
            return sb.ToString();
        }

        private string ExtractFromImage(byte[] imageBytes)
        {
            using var pix = Pix.LoadFromMemory(imageBytes);
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var page = engine.Process(pix);
            return page.GetText();
        }
    }
}
