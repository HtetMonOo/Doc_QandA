using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;

namespace DocQandA.Services
{
    public class AiKernelService
    {
        public Kernel Kernel { get; }

        public AiKernelService(IConfiguration config)
        {
            var groqKey = config["GROQ_API_KEY"];
            var hfKey = config["HF_API_KEY"];

            if (string.IsNullOrWhiteSpace(groqKey) || string.IsNullOrWhiteSpace(hfKey))
                throw new InvalidOperationException("API keys are not configured properly.");

            var builder = Kernel.CreateBuilder();

            // LLM via GROQ
            builder.AddOpenAIChatCompletion(
                modelId: "llama3-70b-8192",
                apiKey: groqKey,
                endpoint: new Uri("https://api.groq.com/openai/v1")
            );

            // Hugging Face embeddings
            builder.AddHuggingFaceTextEmbeddingGeneration(
                model: "sentence-transformers/all-MiniLM-L6-v2",
                apiKey: hfKey
            );

            Kernel = builder.Build();
        }
    }
}