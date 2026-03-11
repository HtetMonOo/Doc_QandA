using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DocQandA.Models;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel;
using DocQandA.Services;

namespace DocQandA.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AiKernelService _aiService;

    public HomeController(ILogger<HomeController> logger, AiKernelService aiService)
    {
        _logger = logger;
        _aiService = aiService;
    }

    public double CosineSimilarity(ReadOnlyMemory<float> vectorA, ReadOnlyMemory<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vectors must be the same length");

        double dot = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        var spanA = vectorA.Span;
        var spanB = vectorB.Span;

        for (int i = 0; i < spanA.Length; i++)
        {
            dot += spanA[i] * spanB[i];
            normA += spanA[i] * spanA[i];
            normB += spanB[i] * spanB[i];
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    public async Task<IActionResult> Index()
    {
         
        //string document = System.IO.File.ReadAllText("./wwwroot/policy.txt");
        //var chunks = document.Split("\n\n");

        //var embeddingService = _aiService.Kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        //var embeddings = new List<(string Text, ReadOnlyMemory<float> Vector)>();

        //foreach (var chunk in chunks)
        //{
        //    var vector = await embeddingService.GenerateEmbeddingAsync(chunk);
        //    embeddings.Add((chunk, vector));
        //}

        //string question = "How many annual leave days are allowed?";

        //var questionVector = await embeddingService.GenerateEmbeddingAsync(question);

        //var bestMatch = embeddings
        //    .OrderByDescending(e => CosineSimilarity(e.Vector, questionVector))
        //    .First();

        //var prompt = $@"
        //    Use the document below to answer the question.

        //    Document:
        //    {bestMatch.Text}

        //    Question:
        //    {question}
        //";
        //var result = await _aiService.Kernel.InvokePromptAsync(prompt);

        //Console.WriteLine(result);
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
