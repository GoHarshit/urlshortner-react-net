namespace UrlShortner.DTOs
{
    public class CreateUrlResponse
    {
        public string OriginalUrl { get; set; }
            = string.Empty;

        public string ShortCode { get; set; }
            = string.Empty;

        public string ShortUrl { get; set; }
            = string.Empty;
    }
}