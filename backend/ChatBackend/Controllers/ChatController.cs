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

        // 🔴 BURAYI 1. ADIMDAKİ TAM URL İLE GÜNCELLE
        // Örn: private const string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/api/predict/";
        private const string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/api/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(25);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Nickname))
                return BadRequest(new { success = false, error = "Nickname is required" });

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new { success = true, userId = user.Id, nickname = user.Nickname });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text))
                return BadRequest(new { success = false, error = "Text is required" });

            if (!await _context.Users.AnyAsync(u => u.Id == message.UserId))
                return BadRequest(new { success = false, error = "Invalid UserId" });

            message.CreatedAt = DateTime.UtcNow;

            string emotion = "Unknown";

            try
            {
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("Accept", "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine("🔍 AI Response: " + responseText);

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseText);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var dataArr) &&
                        dataArr.ValueKind == JsonValueKind.Array &&
                        dataArr.GetArrayLength() > 0)
                    {
                        var first = dataArr[0];
                        if (first.TryGetProperty("label", out var labelEl))
                        {
                            var raw = labelEl.GetString()?.ToUpperInvariant() ?? "NEUTRAL";
                            emotion = raw switch
                            {
                                "POSITIVE" => "Positive",
                                "NEGATIVE" => "Negative",
                                _ => "Neutral"
                            };
                        }
                    }
                }
                else
                {
                    // 404/502 gibi durumları logla
                    Console.WriteLine($"⚠ AI HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI Error: " + ex.Message);
            }

            message.Emotion = emotion;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                messageId = message.Id,
                userId = message.UserId,
                text = message.Text,
                emotion
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
