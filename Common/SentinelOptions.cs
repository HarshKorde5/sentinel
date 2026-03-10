namespace Sentinel.Common;

public class OllamaOptions
{
    public const string Section = "Ollama";
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public int TimeoutSeconds { get; set; } = 30;
}
