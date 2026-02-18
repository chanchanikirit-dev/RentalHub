using System.Net.Http.Headers;

namespace RentalHub.Services
{
    public class SupabaseStorageService
    {
        private readonly HttpClient _http;
        private readonly string _supabaseUrl;
        private readonly string _serviceKey;
        private readonly string _bucket;
        private readonly IConfiguration _config;
        public SupabaseStorageService(IConfiguration config)
        {
            _config = config;
            _http = new HttpClient();
            _supabaseUrl = _config["Supabase:BaseUrl"];
            _serviceKey = _config["Supabase:ServiceKey"];
            _bucket = _config["Supabase:Bucket"];

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceKey);

            _http.DefaultRequestHeaders.Add("apikey", _serviceKey);
        }
        public async Task DeleteImageByUrl(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return;

            var uri = new Uri(imageUrl);

            //Extract path after /public/
            var index = uri.AbsolutePath.IndexOf("/public/");
            if (index == -1) return;

            var filePath = uri.AbsolutePath[(index + 8)..]; // skip "/public/"

        var deleteUrl =
            $"{_supabaseUrl}/storage/v1/object/{filePath}";

        var response = await _http.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                //throw new Exception($"Failed to delete image: {err}");
            }
        }
    }
}
