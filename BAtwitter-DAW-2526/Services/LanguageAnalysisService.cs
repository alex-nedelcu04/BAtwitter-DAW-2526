using Azure;
using Azure.Core;
using Humanizer;
using System.Net.Http.Headers;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BAtwitter_DAW_2526.Services
{
    // Clasa pentru rezultatul analizei de limbaj
    public class LanguageResult
    {
        public string Label { get; set; } = "neutral"; // positive, neutral, negative
        public double Confidence { get; set; } = 0.0; // 0.0 - 1.0
        public bool Success { get; set; } = false;
        public string? ErrorMessage { get; set; }
    }

    // Interfata serviciului pentru dependency injection
    public interface ILanguageAnalysisService
    {
        Task<LanguageResult> AnalyzeLanguageAsync(string text);
    }

    // Implementarea serviciului de analiza de limbaj folosind OpenAI API
    public class LanguageAnalysisService : ILanguageAnalysisService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<LanguageAnalysisService> _logger;

        public LanguageAnalysisService(IConfiguration configuration, ILogger<LanguageAnalysisService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"] ?? throw new ArgumentNullException("OpenAI:ApiKey not configured");
            _logger = logger;

            // Configurare HttpClient pentru OpenAI API
            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<LanguageResult> AnalyzeLanguageAsync(string text)
        {
            try
            {
                // Construim prompt-ul pentru analiza de limbaj
                var systemPrompt = @"You are a content moderation assistant for social media. Determine if the text contains insults, harassment, hate speech, discriminatory language, slurs, or dehumanization. Return ONLY JSON: {""label"":""true|uncertain|false"",""confidence"":0.0-1.0}

Rules:
- ""true"" if any of the above is present (including praising or promoting extremist ideology or Nazi/fascist rhetoric/slurs).
- ""uncertain"" only if the text is ambiguous, quoted, censored, or context-dependent, otherwise please respond with either ""true"" or ""false"" if you can derive a conclusion. If ""uncertain"", give values in range 0.0-1.0, not only 0.5, based on what it leans towards.
- ""false"" if none is present.
Return ONLY JSON.";
                var userPrompt = $"Analyze this post: \"{text}\"";
                
                // Construim request-ul pentru OpenAI API
                var requestBody = new
                {
                    model = "gpt-4o-mini", // Using gpt-4o-mini as gpt-5-nano doesn't exist
                    messages = new[] {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userPrompt }
                    },
                    temperature = 0.1, // Low temperature for consistent results
                    max_tokens = 50
                };
            
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending language analysis request to OpenAI API");
                
                // Trimitem request-ul catre OpenAI API
                var response = await _httpClient.PostAsync("chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
                    return new LanguageResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: { response.StatusCode }"
                    };
                }
    
                // Parsam raspunsul de la OpenAI
                var openAiResponse = JsonSerializer.Deserialize<OpenAiResponse>(responseContent);
                var assistantMessage = openAiResponse?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrEmpty(assistantMessage))
                {
                    return new LanguageResult
                    {
                        Success = false,
                        ErrorMessage = "Empty response from API"
                    };
                }

                _logger.LogInformation("OpenAI response: {Response}", assistantMessage);
                
                // Parsam JSON-ul din raspunsul asistentului
                var languageData = JsonSerializer.Deserialize<LanguageResponse>(assistantMessage);
                if (languageData == null)
                {
                    return new LanguageResult
                    {
                        Success = false,
                        ErrorMessage = "Failed to parse language response"
                    };
                }

                // Validam si normalizam label-ul
                var label = languageData.Label?.ToLower() switch
                {
                    "true" => "true",
                    "false" => "false",
                    _ => "uncertain"
                };

                // Validam confidence score
                var confidence = Math.Clamp(languageData.Confidence, 0.0, 1.0);
                return new LanguageResult
                {
                    Label = label,
                    Confidence = confidence,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing language");
                return new LanguageResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
    }

    // Clase pentru deserializarea raspunsului OpenAI
    public class OpenAiResponse
    {
        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }
    }
    public class Choice
    {
        [JsonPropertyName("message")]
        public Message? Message { get; set; }
    }
    public class Message
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class LanguageResponse
    {
        [JsonPropertyName("label")]
        public string? Label { get; set; }
        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }
}