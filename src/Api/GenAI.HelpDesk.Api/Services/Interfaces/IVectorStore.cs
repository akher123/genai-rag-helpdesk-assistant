namespace GenAI.HelpDesk.Api.Services.Interfaces;

public interface IVectorStore
{
    void Add(string source, string content, float[] embedding);
    List<(string Source, string Content)> Search(float[] query, int topK = 5);
}
