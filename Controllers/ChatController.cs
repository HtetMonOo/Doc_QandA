using Microsoft.AspNetCore.Mvc;
using DocQandA.Models;
using DocQandA.Services;

namespace DocQandA.Controllers
{
    public class ChatController : Controller
    {
        private readonly RagService _rag;
        private readonly IWebHostEnvironment _env;

        public ChatController(RagService rag, IWebHostEnvironment env)
        {
            _rag = rag;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View("Upload", new UploadViewModel());
        }

        [HttpPost]
        [RequestSizeLimit(2_000_000)] 
        public async Task<IActionResult> Upload(UploadViewModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                model.ErrorMessage = "Please select a valid .txt file.";
                return View("Upload", model);
            }

            var filePath = Path.Combine(_env.WebRootPath, "uploads", Guid.NewGuid() + ".txt");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using (var stream = System.IO.File.Create(filePath))
            {
                await model.File.CopyToAsync(stream);
            }

            var success = await _rag.LoadDocumentAsync(filePath);

            if (!success)
            {
                model.ErrorMessage = "Failed to process document. Try again later.";
                return View("Upload", model);
            }

            TempData["FilePath"] = filePath;
            return RedirectToAction("Chat");
        }

        [HttpGet]
        public IActionResult Chat()
        {
            return View(new ChatViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Chat(ChatViewModel model)
        {
            if (TempData["FilePath"] == null)
            {
                model.Answer = "No document loaded. Upload a file first.";
                return View(model);
            }

            model.Answer = await _rag.AskAsync(model.Question);
            TempData.Keep("FilePath"); 
            return View(model);
        }
    }
}