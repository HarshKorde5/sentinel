namespace Sentinel.Common;

public static class SentinelConstants
{
    public static class Headers
    {
        public const string ApiKey = "X-Api-Key";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";
    }

    public static class Providers
    {
        public const string Ollama = "Ollama";
        public const string OpenAI = "OpenAI";
    }

    public static class Chache
    {
        public const string KeyPrefix = "sentinel:cache:";
        public const string RateLimitPrefix = "sentinel:ratelimit:";
        public const int DefaultTtlHours = 1;

    }

    public static class ErrorCodes
    {
        public const string Timeout = "TIMEOUT";
        public const string ModelUnavailable = "MODEL_UNAVAILABLE";
        public const string InvalidResponse = "INVALID_RESPONSE";
        public const string CacheError = "CACHE_ERROR";
        public const string RateLimitError = "RATE_LIMIT_ERROR";
        public const string DatabaseError = "DATABASE_ERROR";

        public const string BothModelsFailed = "BOTH_MODELS_FAILED";
    }

    public static class HttpContextKeys
    {
        public const string Product = "Product";
        public const string ProductId = "ProductId";
    }

    public static class SkippedPaths
    {
        public static readonly string[] Prefixes = ["/dashboard", "/health", "/swagger"];
    }


    public static class Seed
    {
        public const string AdminPasswordKey = "Seed:AdminPassword";
        public const string ProductAKeyName = "Seed:ProductAKey";
        public const string DefaultAdminPassword = "admin@123";
        public const string DefaultProductAKey = "sentinel-key-producta-2025";
    }
}