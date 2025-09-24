namespace TitleVerification.Api.Services
{
    public interface IReportService
    {
        Task<(Dictionary<string, string> conditions, string trafficLight, Models.LandRecord? matched)> EvaluateAsync(Dictionary<string, string> extracted);
    }
}
