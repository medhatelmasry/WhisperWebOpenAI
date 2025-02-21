using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenAI;

namespace WhisperWebOpenAI.Pages;

public class TranslationModel : PageModel {
  private readonly ILogger<TranslationModel> _logger;
  private readonly OpenAIClient _openAIClient;
  private readonly IConfiguration _configuration;
  public List<SelectListItem>? AudioFiles { get; set; }

  public TranslationModel(ILogger<TranslationModel> logger,
      OpenAIClient client,
      IConfiguration configuration
  )
  {
    _logger = logger;
    _openAIClient = client;
    _configuration = configuration;

    // create wwroot/audio folder if it doesn't exist
    string? folder = _configuration["OpenAI:Translation:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    if (!Directory.Exists(path)) {
      Directory.CreateDirectory(path);
    }
  }

  public void OnGet() {
      AudioFiles = GetAudioFiles();
  }

  public async Task<IActionResult> OnPostAsync(string? audioFile) {
    if (string.IsNullOrEmpty(audioFile)) {
      return Page();
    }

    string? modelName = _configuration["OpenAI:Translation:Model"];
    var audioClient = _openAIClient.GetAudioClient(modelName);

    var result = await audioClient.TranslateAudioAsync(audioFile);

    if (result is null) {
      return Page();
    }

    string? folder = _configuration["OpenAI:Translation:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    ViewData["AudioFile"] = audioFile.StartsWith(path) ? audioFile.Substring(path.Length + 1) : audioFile;
    ViewData["Transcription"] = result.Value.Text;
    AudioFiles = GetAudioFiles();

    return Page();
  }

  public List<SelectListItem> GetAudioFiles() {
    List<SelectListItem> items = new List<SelectListItem>();
    string? folder = _configuration["OpenAI:Translation:Folder"];
    string? path = $"wwwroot/audio/{folder}";

    // Get files with .wav or .mp3 extensions
    string[] wavFiles = Directory.GetFiles(path, "*.wav");
    string[] mp3Files = Directory.GetFiles(path, "*.mp3");

    // Combine the arrays
    string[] list = wavFiles.Concat(mp3Files).ToArray();

    foreach (var item in list) {
      items.Add(new SelectListItem {
        Value = item.ToString(),
        Text = item.StartsWith(path) ? item.Substring(path.Length + 1) : item
      });
    }

    return items;
  }
}
