using GenAI.HelpDesk.Api.Models;

namespace GenAI.HelpDesk.Api.Services.Interfaces;

public interface IRagService
{
    /// <summary>
    /// Performs a semantic RAG-based question answering operation.
    /// </summary>
    Task<AskQuestionResponse> AskQuestionAsync(string question);

    /// <summary>
    /// Initializes the vector store by loading, chunking, and embedding documents.
    /// Should be called once during application startup.
    /// </summary>
    Task InitializeAsync();
}
