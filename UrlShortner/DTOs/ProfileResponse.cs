namespace UrlShortner.DTOs
{
    public class ProfileResponse
    {
        public string Email
        { get; set; } = string.Empty;

        public string Plan
        { get; set; } = string.Empty;

        public DateTime CreatedAt
        { get; set; }

        public int TotalUrls
        { get; set; }

        public int TotalClicks
        { get; set; }
    }
}