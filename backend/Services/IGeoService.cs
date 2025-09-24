namespace TitleVerification.Api.Services
{
    public interface IGeoService
    {
        Task<(double lat, double lon)?> GeocodeAddressAsync(string address);
    }
}
