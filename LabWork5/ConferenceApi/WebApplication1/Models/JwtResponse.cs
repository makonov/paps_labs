namespace ConferenceApi.Models
{
    public class JwtResponse
    {
        public string AccessToken { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}