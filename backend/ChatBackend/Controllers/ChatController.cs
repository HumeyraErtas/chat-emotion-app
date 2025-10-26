using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatBackend.Data;
using ChatBackend.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ChatBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/run/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            try
            {
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine("🔍 AI Response: " + responseText);

                using JsonDocument doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                string label = "Unknown";
                float score = 0;

                if (root.TryGetProperty("data", out JsonElement dataArray)
                    && dataArray.ValueKind == JsonValueKind.Array
                    && dataArray.GetArrayLength() > 0)
                {
                    var result = dataArray[0];

                    if (result.TryGetProperty("label", out JsonElement labelElement))
                        label = labelElement.GetString() ?? "Unknown";

                    if (result.TryGetProperty("score", out JsonElement scoreElement))
                        score = scoreElement.GetSingle();
                }

                message.Emotion = label;
                message.CreatedAt = DateTime.UtcNow;

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message, score });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI ERROR: " + ex.Message);
                return Ok(new
                {
                    success = false,
                    emotion = "Unknown",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("messages")]
        public IActionResult GetMessages()
        {
            var messages = _context.Messages.OrderByDescending(m => m.Id).ToList();
            return Ok(messages);
        }
    }
}
