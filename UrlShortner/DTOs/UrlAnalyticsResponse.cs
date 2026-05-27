namespace UrlShortner.DTOs
{
    public class UrlAnalyticsResponse
    {
        public string OriginalUrl
        { get; set; } = string.Empty;

        public string ShortCode
        { get; set; } = string.Empty;

        public int ClickCount
        { get; set; }

        public DateTime CreatedAt
        { get; set; }

        public DateTime? ExpiresAt
        { get; set; }
    }
}