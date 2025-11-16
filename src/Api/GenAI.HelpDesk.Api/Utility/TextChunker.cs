namespace GenAI.HelpDesk.Api.Utility;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class TextChunker
{

    /// <summary>
    /// Splits text into chunks with optional overlap.
    /// </summary>
    /// <param name="text">Input text</param>
    /// <param name="chunkSize">Maximum words per chunk</param>
    /// <param name="overlap">Number of overlapping words between chunks</param>
    /// <returns>List of text chunks</returns>
    public static List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
    {
        var sentences = Regex.Split(text, @"(?<=[.!?])\s+");
        var chunks = new List<string>();
        var currentChunk = new List<string>();
        int currentSize = 0;

        foreach (var sentence in sentences)
        {
            int sentenceSize = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            if (currentSize + sentenceSize > chunkSize)
            {
                // Add the current chunk
                chunks.Add(string.Join(" ", currentChunk));

                // Keep overlap words for next chunk
                currentChunk = currentChunk.Skip(Math.Max(0, currentChunk.Count - overlap)).ToList();
                currentSize = currentChunk.Sum(s => s.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length);
            }

            currentChunk.Add(sentence);
            currentSize += sentenceSize;
        }

        if (currentChunk.Any())
            chunks.Add(string.Join(" ", currentChunk));

        return chunks;
    }

    /// <summary>
    /// Splits text into chunks of approximately 'size' characters with 'overlap',
    /// ensuring words are not split.
    /// </summary>
    public static IEnumerable<string> ChunkTextByWords(string text, int size, int overlap)
    {
        if (string.IsNullOrWhiteSpace(text))
            yield break;

        if (size <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Chunk size must be greater than zero.");

        if (overlap < 0 || overlap >= size)
            throw new ArgumentOutOfRangeException(nameof(overlap), "Overlap must be non-negative and smaller than chunk size.");

        // Step 1: Normalize whitespace efficiently
        Span<char> buffer = stackalloc char[text.Length];
        int writeIndex = 0;
        bool inWhitespace = false;

        foreach (char c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                if (!inWhitespace)
                {
                    buffer[writeIndex++] = ' ';
                    inWhitespace = true;
                }
            }
            else
            {
                buffer[writeIndex++] = c;
                inWhitespace = false;
            }
        }

        if (writeIndex == 0)
            yield break;

        var clean = new string(buffer.Slice(0, writeIndex)).Trim();
        int start = 0;
        int step = size - overlap;

        // Step 2: Chunk text by words
        while (start < clean.Length)
        {
            int end = start + size;
            if (end >= clean.Length)
            {
                end = clean.Length;
            }
            else
            {
                // Move end back to previous space to avoid cutting a word
                int space = clean.LastIndexOf(' ', end);
                if (space > start)
                    end = space;
            }

            yield return clean.AsSpan(start, end - start).ToString();

            start += end - start - overlap;
            if (start < 0) start = 0; // safety check
        }
    }

}
