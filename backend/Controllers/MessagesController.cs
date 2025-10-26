using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        // Hugging Face Space URL’inizi buraya yazın
        private const string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/run/predict";

        public MessagesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeMessage([FromBody] MessageInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Text))
                return BadRequest("Text cannot be empty.");

            // Hugging Face API payload
            var payload = new { data = new[] { input.Text } };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(AI_URL, payload);
                response.EnsureSuccessStatusCode(); // 200–299 değilse exception fırlatır

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(json);

                return Ok(result.RootElement);
            }
            catch (HttpRequestException ex)
            {
                // Hata varsa detaylı mesaj döndür
                return StatusCode(500, $"AI service error: {ex.Message}");
            }
        }
    }

    // Request body sınıfı
    public class MessageInput
    {
        public string? Text { get; set; }
    }
}
