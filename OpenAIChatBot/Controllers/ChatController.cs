using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace OpenAIChatBot.Controllers
{
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ChatController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            try
            {
                var apiKey = _configuration["OpenRouter:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    return Json(new { error = "API key not configured" });
                }

                var payload = new
                {
                    model = "mistralai/mistral-7b-instruct",
                    messages = new[]
                    {
                        new { role = "user", content = request.Message }
                    },
                    max_tokens = 1000,
                    temperature = 0.7
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
                _httpClient.DefaultRequestHeaders.Add("X-Title", "AI Chat Bot");

                var response = await _httpClient.PostAsync("https://openrouter.ai/api/v1/chat/completions", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<OpenRouterResponse>(responseContent);
                    var botMessage = result?.choices?.FirstOrDefault()?.message?.content ?? "Sorry, I couldn't process your request.";

                    return Json(new { message = botMessage });
                }
                else
                {
                    return Json(new { error = "Failed to get response from AI service" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = $"An error occurred: {ex.Message}" });
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class OpenRouterResponse
    {
        public Choice[]? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? content { get; set; }
    }
}