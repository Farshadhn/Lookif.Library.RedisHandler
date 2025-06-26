namespace Lookif.Library.RedisHandler
{
    public class RedisOptions
    {
        public string Configuration { get; set; } = string.Empty;
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int Database { get; set; } = 0;
        public bool SSL { get; set; } = false;
    }
} 