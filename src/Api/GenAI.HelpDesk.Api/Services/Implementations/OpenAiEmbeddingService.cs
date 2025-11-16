using GenAI.HelpDesk.Api.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace GenAI.HelpDesk.Api.Services.Implementations;

public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAiEmbeddingService(IConfiguration config)
    {
        _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("Missing OpenAI key");
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        var body = new { input = text, model = "openai/text-embedding-3-small" };
        var response = await _http.PostAsJsonAsync("https://models.github.ai/inference/embeddings", body);
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        return data.GetProperty("data")[0].GetProperty("embedding")
                   .EnumerateArray().Select(x => (float)x.GetDouble()).ToArray();
    }
}
