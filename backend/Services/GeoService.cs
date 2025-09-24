using System.Net.Http.Json;

namespace TitleVerification.Api.Services
{
    public class GeoService : IGeoService
    {
        private readonly HttpClient _http;
        public GeoService(HttpClient http) { _http = http; }

        public async Task<(double lat, double lon)?> GeocodeAddressAsync(string address)
        {
            var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";
            var res = await _http.GetFromJsonAsync<List<NominatimResponse>>(url);
            if (res?.Count > 0 && double.TryParse(res[0].lat, out var lat) && double.TryParse(res[0].lon, out var lon))
                return (lat, lon);
            return null;
        }

        private class NominatimResponse { public string lat { get; set; } = ""; public string lon { get; set; } = ""; }
    }
}
