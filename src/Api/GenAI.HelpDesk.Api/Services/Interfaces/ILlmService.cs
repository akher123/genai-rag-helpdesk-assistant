namespace GenAI.HelpDesk.Api.Services.Interfaces;

public interface ILlmService
{
    Task<string> GetChatResponseAsync(string systemPrompt, string userPrompt);
}
