using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAI;
using OpenAI.Audio;

namespace WhisperWebOpenAI.Pages;

public class Text2AudioModel : PageModel {
  private readonly ILogger<Text2AudioModel> _logger;
  private readonly OpenAIClient _openAIClient;
  private readonly IConfiguration _configuration;

  const string DefaultText = @"Security officials confiscating bottles of water, tubes of 
shower gel and pots of face creams are a common sight at airport security.  
But officials enforcing the no-liquids rule at South Korea's Incheon International Airport 
have been busy seizing another outlawed item: kimchi, a concoction of salted and fermented 
vegetables that is a staple of every Korean dinner table.";

  public Text2AudioModel(ILogger<Text2AudioModel> logger,
      OpenAIClient client,
      IConfiguration configuration
  )
  {
    _logger = logger;
    _openAIClient = client;
    _configuration = configuration;

    // create wwroot/audio folder if it doesn't exist
    string? folder = _configuration["OpenAI:Text2Audio:Folder"];
    string? path = $"wwwroot/audio/{folder}";
    if (!Directory.Exists(path)) {
        Directory.CreateDirectory(path);
    }
  }

  public void OnGet() { 
    ViewData["sampleText"] = DefaultText;
  }

  public async Task<IActionResult> OnPostAsync(string inputText) {
    string? modelName = _configuration["OpenAI:Text2Audio:Model"];
    var audioClient = _openAIClient.GetAudioClient(modelName);

    BinaryData speech = await audioClient.GenerateSpeechAsync(inputText, GeneratedSpeechVoice.Alloy);

    // Generate a consistent file name based on the hash of the input text
    using var sha256 = System.Security.Cryptography.SHA256.Create();
    byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inputText));
    string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    string fileName = $"{hashString}.mp3";

    string? folder = _configuration["OpenAI:Text2Audio:Folder"];
    string filePath = Path.Combine("wwwroot", "audio", folder!, fileName);

    // Check if the file already exists
    if (!System.IO.File.Exists(filePath)) {
      using FileStream stream = System.IO.File.OpenWrite(filePath);
      speech.ToStream().CopyTo(stream);
    }
    ViewData["sampleText"] = inputText;
    ViewData["AudioFilePath"] = $"/audio/{folder}/{fileName}";
    return Page();
  }
}
