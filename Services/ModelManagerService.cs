using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace LocalAIRecorder.Services;

public class ModelManagerService
{
    public string WhisperModelPath => Path.Combine(FileSystem.AppDataDirectory, "ggml-base.bin");
    public string LLMModelPath => Path.Combine(FileSystem.AppDataDirectory, "Phi-3-mini-onnx");

    public bool AreModelsReady => File.Exists(WhisperModelPath) && Directory.Exists(LLMModelPath) && File.Exists(Path.Combine(LLMModelPath, "genai_config.json"));

    public async Task CheckAndDownloadModelsAsync(IProgress<string> progress)
    {
        using var client = new HttpClient();

        // Ensure Whisper model via shared provider
        if (!File.Exists(WhisperModelPath))
        {
            progress.Report("Downloading Whisper Model...");
            await WhisperModelProvider.EnsureModelAsync();
        }

        // Download LLM Model
        if (!Directory.Exists(LLMModelPath) || !File.Exists(Path.Combine(LLMModelPath, "genai_config.json")))
        {
            progress.Report("Downloading Phi-3 Model (This may take a while)...");
            
            Directory.CreateDirectory(LLMModelPath);
            
            // Base URL for Phi-3 Mini 4K Instruct ONNX (CPU INT4)
            var baseUrl = "https://huggingface.co/microsoft/Phi-3-mini-4k-instruct-onnx/resolve/main/cpu_and_mobile/cpu-int4-rtn-block-32-acc-level-4";
            
            // Files required for OnnxRuntimeGenAI
            var files = new[] { 
                "model.onnx", 
                "model.onnx.data", 
                "tokenizer.json", 
                "tokenizer_config.json", 
                "genai_config.json" 
            };
            
            foreach (var file in files)
            {
                progress.Report($"Downloading {file}...");
                var fileUrl = $"{baseUrl}/{file}?download=true";
                var filePath = Path.Combine(LLMModelPath, file);
                
                if (!File.Exists(filePath))
                {
                     try 
                     {
                        var response = await client.GetAsync(fileUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            using var fs = new FileStream(filePath, FileMode.Create);
                            await response.Content.CopyToAsync(fs);
                        }
                        else
                        {
                             progress.Report($"Failed to download {file}: {response.StatusCode}");
                        }
                     }
                     catch (Exception ex)
                     {
                         progress.Report($"Error downloading {file}: {ex.Message}");
                     }
                }
            }
        }
        
        progress.Report("Models Ready!");
    }
}