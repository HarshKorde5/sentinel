namespace Sentinel.Common;

public class OllamaOptions
{
    public const string Section = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public int TimeoutSeconds { get; set; } = 30;
}

public class RedisOptions
{
    public const string Section = "Redis";
    public string ConnectionString { get; set; } = "localhost:6379";
}

public class CacheOptions
{
    public const string Section = "Cache";
    public int TtlHours { get; set; } = 1;
}
