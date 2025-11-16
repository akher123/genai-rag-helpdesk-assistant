using GenAI.HelpDesk.Api.Services.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace GenAI.HelpDesk.Api.Services.Implementations;

public class OpenAiChatService : ILlmService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public OpenAiChatService(IConfiguration config)
    {
        _apiKey = config["OpenAI:ApiKey"] ?? throw new Exception("Missing OpenAI key");
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetChatResponseAsync(string systemPrompt, string userPrompt)
    {
        var body = new
        {
            model = "gpt-4.1-mini",
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var resp = await _http.PostAsJsonAsync("https://models.github.ai/inference/v1/chat/completions", body);
        resp.EnsureSuccessStatusCode();
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("choices")[0]
                   .GetProperty("message")
                   .GetProperty("content")
                   .GetString() ?? "";
    }
}
