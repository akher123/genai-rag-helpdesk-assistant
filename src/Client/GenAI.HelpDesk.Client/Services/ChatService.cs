using GenAI.HelpDesk.Client.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GenAI.HelpDesk.Client.Services;

public class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubModel _config;

    public ChatService(HttpClient httpClient, IOptions<GitHubModel> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _config.ApiKey);
    }

    public async Task<string> AskQuestionAsync(string question, string context = "")
    {
        var messages = new[]
        {
        new { role = "system", content = "You are a helpful assistant." },
        new { role = "user", content = $"Context:\n{context}\n\nQuestion: {question}" }
    };

        var requestBody = new
        {
            model = _config.Model,
            messages,
            max_completion_tokens = 500,
            temperature =1
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("https://models.github.ai/inference/v1/chat/completions", content);
  
        response.EnsureSuccessStatusCode();
        var responseString = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseString);
        var answer = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return answer.Trim();
    }
}
