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

        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/gradio_api/call/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (string.IsNullOrWhiteSpace(user.Nickname))
                return BadRequest(new { success = false, error = "Nickname required" });

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
                // ✅ 1. POST → Event ID Al
                var jsonContent = JsonSerializer.Serialize(new
                {
                    data = new[] { message.Text }
                });

                Console.WriteLine("✅ Request Sent To AI!");
                var response = await _httpClient.PostAsync(AI_URL,
                    new StringContent(jsonContent, Encoding.UTF8, "application/json"));

                var rawEvent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("📌 EVENT RESPONSE => " + rawEvent);

                var eventId = JsonDocument.Parse(rawEvent).RootElement.GetProperty("event_id").GetString();
                if (string.IsNullOrEmpty(eventId))
                    throw new Exception("event_id alınamadı!");

                // ✅ 2. GET → Streaming Sonuç Oku
                var resultUrl = $"{AI_URL}/{eventId}";
                Console.WriteLine("🔍 STREAM => " + resultUrl);

                using var stream = await _httpClient.GetStreamAsync(resultUrl);
                using var reader = new StreamReader(stream);

                DateTime timeout = DateTime.Now.AddSeconds(8);
                string? line;

                while ((line = await reader.ReadLineAsync()) != null && DateTime.Now < timeout)
                {
                    if (!line.Trim().StartsWith("{")) continue;
                    if (!line.Contains("label")) continue;

                    Console.WriteLine("🧠 STREAM LINE => " + line);

                    try
                    {
                        var label = JsonDocument.Parse(line)
                            .RootElement.GetProperty("data")[0]
                            .GetProperty("label")
                            .GetString();

                        emotion = label?.ToUpper() switch
                        {
                            "POSITIVE" => "Positive",
                            "NEGATIVE" => "Negative",
                            _ => "Neutral"
                        };

                        Console.WriteLine("✅ EMOTION DETECTED => " + emotion);
                        break;
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI ERROR => " + ex.Message);
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
            var messages = _context.Messages.OrderByDescending(m => m.Id).ToList();
            return Ok(messages);
        }
    }
}
