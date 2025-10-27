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

        // ✅ Correct Hugging Face URL
        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/gradio_api/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Nickname))
                return BadRequest(new { error = "Nickname is required" });

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { success = true, userId = user.Id });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == message.UserId))
                return BadRequest(new { success = false, error = "Invalid UserId" });

            message.CreatedAt = DateTime.UtcNow;
            string emotion = "Unknown";

            try
            {
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine("🔍 AI Response: " + responseText);

                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                if (root.TryGetProperty("data", out JsonElement dataArray))
                {
                    var first = dataArray[0];

                    if (first.TryGetProperty("label", out JsonElement labelElement))
                    {
                        var label = labelElement.GetString();
                        emotion = label switch
                        {
                            "POSITIVE" => "Positive",
                            "NEGATIVE" => "Negative",
                            _ => "Neutral"
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI ERROR: " + ex.Message);
            }

            message.Emotion = emotion;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                userId = message.UserId,
                text = message.Text,
                emotion = emotion
            });
        }

        [HttpGet("messages")]
        public IActionResult GetMessages()
        {
            var messages = _context.Messages
                .OrderByDescending(m => m.Id)
                .ToList();

            return Ok(messages);
        }
    }
}
