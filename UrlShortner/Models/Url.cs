namespace UrlShortner.Models
{
    public class Url
    {
        public long Id { get; set; }

        public string OriginalUrl { get; set; }
            = string.Empty;

        public string ShortCode { get; set; }
            = string.Empty;

        public int ClickCount { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        // =========================
        // Foreign Key
        // =========================

        public int UserId { get; set; }

        public User User { get; set; }
            = null!;
    }
}