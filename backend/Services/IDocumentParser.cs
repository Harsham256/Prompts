namespace TitleVerification.Api.Services
{
    public interface IDocumentParser
    {
        // Returns a dictionary of extracted values, e.g. Name, Aadhaar, LandId, FathersName, Address, confidence
        Dictionary<string, string> Parse(byte[] fileBytes, string contentType);
    }
}
