using CYHDocBrandModel.Common;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

public class BannerAiService : IBannerAiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly IDoctorAiSettingRepository _aiRepo;
    private readonly ILogger<BannerAiService> _logger;

    public BannerAiService(
        HttpClient httpClient, 
        IConfiguration configuration, 
        IDoctorAiSettingRepository aiRepo,
        ILogger<BannerAiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _aiRepo = aiRepo;
        _logger = logger;
    }

    public List<string> GetSupportedAspectRatios() => new() { "1:1", "4:5", "9:16", "16:9" };

    public List<string> GetTemplatePresets() => new()
    {
        "Hospital",
        "Doctor",
        "Clinic",
        "Lab",
        "Awareness",
        "Promotional"
    };

    public async Task<BannerGenerationResult> GenerateBannerAsync(BannerGenerateRequest request)
    {
        try
        {
            var provider = (request.Provider ?? "Gemini").Trim();
            var enhancedPrompt = BuildOptimizedPrompt(request);

            // 1. Resolve API Key from SQL mapped to Doctor, fallback to Global SQL or Config
            string? activeApiKey = request.ApiKeyOverride;
            string keySource = "Direct Input";

            if (string.IsNullOrWhiteSpace(activeApiKey))
            {
                activeApiKey = await _aiRepo.GetActiveApiKeyAsync(request.DoctorId, request.UserId, provider);
                if (!string.IsNullOrWhiteSpace(activeApiKey))
                {
                    keySource = request.DoctorId.HasValue ? "Doctor SQL Key" : "Hospital Global SQL Key";
                }
            }

            if (string.IsNullOrWhiteSpace(activeApiKey))
            {
                activeApiKey = _configuration[$"BannerAi:{provider}ApiKey"] 
                    ?? Environment.GetEnvironmentVariable($"{provider.ToUpper()}_API_KEY");
                if (!string.IsNullOrWhiteSpace(activeApiKey))
                {
                    keySource = "Config / Env";
                }
            }

            if (provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(activeApiKey))
                {
                    var geminiResult = await CallGeminiImagenAsync(enhancedPrompt, request.AspectRatio, activeApiKey);
                    if (geminiResult.Success)
                    {
                        geminiResult.ProviderUsed = $"Gemini ({keySource})";
                        return geminiResult;
                    }
                    _logger.LogWarning("Gemini generation failed: {Error}. Falling back to curated background.", geminiResult.ErrorMessage);
                }
            }
            else if (provider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(activeApiKey))
                {
                    var openAiResult = await CallOpenAiDallEAsync(enhancedPrompt, request.AspectRatio, activeApiKey);
                    if (openAiResult.Success)
                    {
                        openAiResult.ProviderUsed = $"OpenAI ({keySource})";
                        return openAiResult;
                    }
                    _logger.LogWarning("OpenAI generation failed: {Error}. Falling back to curated background.", openAiResult.ErrorMessage);
                }
            }

            // Fallback: If no API key configured or API call errored, provide a curated medical photography asset
            var presetUrl = GetFallbackPreset(request.TemplateType);
            return new BannerGenerationResult
            {
                Success = true,
                ImageUrl = presetUrl,
                ProviderUsed = $"{provider} (Curated Medical Preset - Map Doctor Key in SQL for Live Generation)",
                GeneratedAt = IndianTimeHelper.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating banner background");
            return new BannerGenerationResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                ProviderUsed = request.Provider
            };
        }
    }

    private string BuildOptimizedPrompt(BannerGenerateRequest request)
    {
        var categoryDescription = request.TemplateType switch
        {
            "Doctor" => "Modern clean medical doctor office with stethoscopes, medical books, soft bokeh lighting, clinical cyan ambient glow",
            "Hospital" => "Modern state-of-the-art hospital exterior and interior glass architecture, bright medical healthcare setting",
            "Clinic" => "Welcoming outpatient consultation room, medical examination equipment, clean white and teal tones",
            "Lab" => "Advanced pathology diagnostics laboratory, microscope, high-tech glass test tubes, pristine laboratory environment",
            "Awareness" => "Healthy lifestyle, wellness, heart health ribbon or clean organic medical wellness theme with warm reassuring atmosphere",
            "Promotional" => "Healthcare checkup package background, modern medical clinic consultation scene, professional aesthetic",
            _ => "Modern professional healthcare and medicine aesthetic background"
        };

        var userAdditions = string.IsNullOrWhiteSpace(request.CustomPrompt) ? "" : $", {request.CustomPrompt.Trim()}";

        // Crucial: instruct AI to NOT hallucinate random illegible text, keep clean negative space for overlays
        return $"{categoryDescription}{userAdditions}. Clean minimalist medical photography background, high resolution 8k, bokeh depth of field, NO TEXT, NO WORDS, NO LETTERS, NO WATERMARK, ample negative space for typography overlay.";
    }

    private async Task<BannerGenerationResult> CallGeminiImagenAsync(string prompt, string aspectRatio, string apiKey)
    {
        // Google Imagen 3 API endpoint
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/imagen-3.0-generate-002:predict?key={apiKey}";
        
        var apiAspectRatio = aspectRatio switch
        {
            "9:16" => "9:16",
            "16:9" => "16:9",
            "4:5" => "3:4", // Imagen supports 1:1, 3:4, 4:3, 9:16, 16:9
            _ => "1:1"
        };

        var requestBody = new
        {
            instances = new[]
            {
                new { prompt = prompt }
            },
            parameters = new
            {
                sampleCount = 1,
                aspectRatio = apiAspectRatio,
                outputMimeType = "image/jpeg"
            }
        };

        using var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(endpoint, content);

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return new BannerGenerationResult { Success = false, ErrorMessage = $"Gemini API Error ({response.StatusCode}): {err}" };
        }

        var jsonStr = await response.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(jsonStr);
        var base64Image = json?["predictions"]?[0]?["bytesBase64Encoded"]?.ToString();

        if (string.IsNullOrEmpty(base64Image))
        {
            return new BannerGenerationResult { Success = false, ErrorMessage = "No image returned from Gemini Imagen." };
        }

        return new BannerGenerationResult
        {
            Success = true,
            ImageBase64 = $"data:image/jpeg;base64,{base64Image}",
            ProviderUsed = "Google Gemini Imagen 3"
        };
    }

    private async Task<BannerGenerationResult> CallOpenAiDallEAsync(string prompt, string aspectRatio, string apiKey)
    {
        var endpoint = "https://api.openai.com/v1/images/generations";
        
        var size = aspectRatio switch
        {
            "9:16" => "1024x1792",
            "16:9" => "1792x1024",
            _ => "1024x1024"
        };

        var requestBody = new
        {
            model = "dall-e-3",
            prompt = prompt,
            n = 1,
            size = size,
            response_format = "b64_json"
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, endpoint);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        req.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(req);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync();
            return new BannerGenerationResult { Success = false, ErrorMessage = $"OpenAI API Error: {err}" };
        }

        var jsonStr = await response.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(jsonStr);
        var base64 = json?["data"]?[0]?["b64_json"]?.ToString();

        if (string.IsNullOrEmpty(base64))
        {
            return new BannerGenerationResult { Success = false, ErrorMessage = "No b64_json data from OpenAI." };
        }

        return new BannerGenerationResult
        {
            Success = true,
            ImageBase64 = $"data:image/png;base64,{base64}",
            ProviderUsed = "OpenAI DALL-E 3"
        };
    }

    private string GetFallbackPreset(string templateType) => templateType switch
    {
        "Doctor" => "https://images.unsplash.com/photo-1622253692010-333f2da6031d?auto=format&fit=crop&w=1200&q=80",
        "Hospital" => "https://images.unsplash.com/photo-1519494026892-80bbd2d6fd0d?auto=format&fit=crop&w=1200&q=80",
        "Clinic" => "https://images.unsplash.com/photo-1586773860418-d37222d8fce3?auto=format&fit=crop&w=1200&q=80",
        "Lab" => "https://images.unsplash.com/photo-1579154204601-01588f351e67?auto=format&fit=crop&w=1200&q=80",
        "Awareness" => "https://images.unsplash.com/photo-1505751172876-fa1923c5c528?auto=format&fit=crop&w=1200&q=80",
        "Promotional" => "https://images.unsplash.com/photo-1532938911079-1b06ac7ceec7?auto=format&fit=crop&w=1200&q=80",
        _ => "https://images.unsplash.com/photo-1584515979956-d9f6e5d09982?auto=format&fit=crop&w=1200&q=80"
    };
}
