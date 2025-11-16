using GenAI.HelpDesk.Api.Services.Interfaces;

namespace GenAI.HelpDesk.Api.Services.Implementations;

public class InMemoryVectorStore : IVectorStore
{
    private readonly List<(string Source, string Content, float[] Embedding)> _entries = new();

    public void Add(string source, string content, float[] embedding) =>
        _entries.Add((source, content, embedding));

    public List<(string Source, string Content)> Search(float[] query, int topK = 5)
    {
        static double Cosine(float[] a, float[] b)
        {
            double dot = 0, na = 0, nb = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                na += a[i] * a[i];
                nb += b[i] * b[i];
            }
            return dot / (Math.Sqrt(na) * Math.Sqrt(nb) + 1e-9);
        }

        return _entries
            .Select(e => (e.Source, e.Content, score: Cosine(query, e.Embedding)))
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => (x.Source, x.Content))
            .ToList();
    }
}
