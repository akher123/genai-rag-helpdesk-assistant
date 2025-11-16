namespace GenAI.HelpDesk.Api.Services.Interfaces;

public interface IEmbeddingService
{
    Task<float[]> GetEmbeddingAsync(string text);
}
