using System.Text.RegularExpressions;
using GenAI.HelpDesk.Api.Models;
using GenAI.HelpDesk.Api.Services.Interfaces;
using GenAI.HelpDesk.Api.Utility;

namespace GenAI.HelpDesk.Api.Services.Implementations;

public class RagService : IRagService
{
    private readonly IEmbeddingService _embeddingService;
    private readonly ILlmService _llmService;
    private readonly IVectorStore _vectorStore;
    private readonly ILogger<RagService> _logger;
    private readonly IConfiguration _config;
    private static bool _initialized;

    public RagService(
        IEmbeddingService embedding,
        ILlmService llm,
        IVectorStore store,
        ILogger<RagService> logger,
        IConfiguration config)
    {
        _embeddingService = embedding;
        _llmService = llm;
        _vectorStore = store;
        _logger = logger;
        _config = config;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            _logger.LogInformation("RAG already initialized, skipping.");
            return;
        }

        var dataPath = _config["Data:Path"] ?? "Data/helpdocument";
        if (!Directory.Exists(dataPath))
        {
            _logger.LogWarning("Document directory {path} not found.", dataPath);
            return;
        }

        _logger.LogInformation("Initializing RAG vector store from {path}", dataPath);

        foreach (var file in Directory.GetFiles(dataPath, "*.*", SearchOption.AllDirectories))
        {
            var text = await File.ReadAllTextAsync(file);
            foreach (var chunk in TextChunker.ChunkText(text, 500, 50))
            {
                var emb = await _embeddingService.GetEmbeddingAsync(chunk);
                _vectorStore.Add(file, chunk, emb);
            }
        }

        _initialized = true;
        _logger.LogInformation("✅ RAG initialization complete.");
    }

    public async Task<AskQuestionResponse> AskQuestionAsync(string question)
    {
        if (!_initialized)
        {
            return new AskQuestionResponse
            {
                AnswerText = "The RAG service is still initializing. Please try again later.",
                SourceList = new List<string>()
            };
        }

        var queryEmbedding = await _embeddingService.GetEmbeddingAsync(question);
        var docs = _vectorStore.Search(queryEmbedding, 10);

        var context = string.Join("\n---\n", docs.Select(d => d.Content));
        var systemPrompt = "You are a CRM documentation assistant. Answer only using provided context.";
        var userPrompt = $"Context:\n{context}\n\nQuestion: {question}\nAnswer:";

        var answer = await _llmService.GetChatResponseAsync(systemPrompt, userPrompt);

        return new AskQuestionResponse
        {
            AnswerText = answer,
            SourceList = docs.Select(d => d.Source).Distinct().ToList()
        };
    }


}
