using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using OpenAI;

namespace WhisperWebOpenAI.Pages;

public class Audio2TextModel : PageModel {
  private readonly ILogger<Audio2TextModel> _logger;
  private readonly OpenAIClient _openAIClient;
  private readonly IConfiguration _configuration;
  public List<SelectListItem>? AudioFiles { get; set; }
  public Audio2TextModel(ILogger<Audio2TextModel> logger,
    OpenAIClient client,
    IConfiguration configuration
  )
  {
    _logger = logger;
    _openAIClient = client;
    _configuration = configuration;

    // create wwroot/audio folder if it doesn't exist
    string? folder = _configuration["OpenAI:Audio2Text:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    if (!Directory.Exists(path)) {
      Directory.CreateDirectory(path);
    }
  }

  public void OnGet() {
    AudioFiles = GetWaveFiles();
  }

  public async Task<IActionResult> OnPostAsync(string? waveFile) {
    if (string.IsNullOrEmpty(waveFile)){
      return Page();
    }

    string? modelName = _configuration["OpenAI:Audio2Text:Model"];
    var audioClient = _openAIClient.GetAudioClient(modelName);

    var result = await audioClient.TranscribeAudioAsync(waveFile);

    if (result is null) {
      return Page();
    }

    string? folder = _configuration["OpenAI:Audio2Text:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    ViewData["AudioFile"] = waveFile.StartsWith(path) ? waveFile.Substring(path.Length + 1) : waveFile;
    ViewData["Transcription"] = result.Value.Text;
    AudioFiles = GetWaveFiles();

    return Page();
  }

  public List<SelectListItem> GetWaveFiles() {
    List<SelectListItem> items = new List<SelectListItem>();
    string? folder = _configuration["OpenAI:Audio2Text:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    
    // Get files with .wav or .mp3 extensions
    string[] wavFiles = Directory.GetFiles(path, "*.wav");
    string[] mp3Files = Directory.GetFiles(path, "*.mp3");

    // Combine the arrays
    string[] list = wavFiles.Concat(mp3Files).ToArray();

    foreach (var item in list) {
      items.Add(new SelectListItem
      {
          Value = item.ToString(),
          Text = item.StartsWith(path) ? item.Substring(path.Length + 1) : item
      });
    }

    return items;
  }
}
