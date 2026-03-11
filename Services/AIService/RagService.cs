using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace DocQandA.Services
{
    public class RagService
    {
        private readonly Kernel _kernel;
        private readonly ITextEmbeddingGenerationService _embedding;

        private List<(string Text, ReadOnlyMemory<float> Vector)> _vectors = new();
        private readonly ILogger<RagService> _logger;

        public RagService(AiKernelService aiService, ILogger<RagService> logger)
        {
            _kernel = aiService.Kernel;
            _embedding = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            _logger = logger;
        }

        public async Task<bool> LoadDocumentAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                Console.WriteLine("file exits.");
                _vectors.Clear();

                var document = await File.ReadAllTextAsync(filePath);
                var chunks = document.Split("\n\n");

                foreach (var chunk in chunks)
                {
                    Console.WriteLine("chunk");
                    var vector = await GetEmbeddingAsync(chunk);
                    _vectors.Add((chunk, vector));
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading document.");
                return false;
            }
        }

        private async Task<ReadOnlyMemory<float>> GetEmbeddingAsync(string text)
        {
            try
            {
                var vector = await _embedding.GenerateEmbeddingAsync(text);
                if (vector.Length == 0)
                    throw new Exception("Empty embedding returned.");

                return vector;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Embedding service unavailable.");
                throw new InvalidOperationException(
                    "Embedding service unavailable. Try again later.");
            }
        }

        public async Task<string> AskAsync(string question)
        {
            if (_vectors.Count == 0)
                return "No document loaded. Please upload a file first.";

            ReadOnlyMemory<float> questionVector;
            try
            {
                questionVector = await GetEmbeddingAsync(question);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            var bestMatch = _vectors
                .OrderByDescending(e => CosineSimilarity(e.Vector, questionVector))
                .First();

            var prompt = $@"
                Use the following document to answer the question clearly.

                Document:
                {bestMatch.Text}

                Question:
                {question}
            ";

            try
            {
                var result = await _kernel.InvokePromptAsync(prompt);
                return result?.ToString() ?? "No response from LLM.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM service unavailable.");
                return "LLM service unavailable. Try again later.";
            }
        }

        private double CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
        {
            var v1 = a.Span;
            var v2 = b.Span;

            double dot = 0, mag1 = 0, mag2 = 0;

            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += v1[i] * v1[i];
                mag2 += v2[i] * v2[i];
            }

            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }
    }
}