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
            if (!await _context.Users.AnyAsync(x => x.Id == message.UserId))
                return BadRequest(new { success = false, error = "Invalid UserId" });

            message.CreatedAt = DateTime.UtcNow;
            string emotion = "Unknown";

            try
            {
                // 1️⃣ İlk istek -> event_id alıyoruz
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var eventResponse = await _httpClient.PostAsync(AI_URL, content);
                var eventText = await eventResponse.Content.ReadAsStringAsync();

                Console.WriteLine("📌 Event Response: " + eventText);

            using var eventDoc = JsonDocument.Parse(eventText);
                var eventId = eventDoc.RootElement.GetProperty("event_id").GetString();

                if (eventId == null)
                    throw new Exception("event_id alınamadı!");

                // ✅ Sonuç için doğru URL
                var resultUrl = $"https://humeyraertas-chat-sentiment-analyzer.hf.space/gradio_api/call/predict/{eventId}";
                Console.WriteLine("🔍 AI Stream URL: " + resultUrl);

                using var stream = await _httpClient.GetStreamAsync(resultUrl);
                using var reader = new StreamReader(stream);

                string? line;
                DateTime timeout = DateTime.Now.AddSeconds(10);

                while ((line = await reader.ReadLineAsync()) != null && DateTime.Now < timeout)
                {
                    Console.WriteLine("📝 AI Stream Line => " + line);

                    if (!line.Trim().StartsWith("{")) continue;
                    if (!line.Contains("label")) continue;

                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var label = doc.RootElement
                                    .GetProperty("data")[0]
                                    .GetProperty("label")
                                    .GetString();

                        Console.WriteLine("✅ LABEL FOUND: " + label);

                        emotion = label?.ToUpper() switch
                        {
                            "POSITIVE" => "Positive",
                            "NEGATIVE" => "Negative",
                            _ => "Neutral"
                        };
                        break;
                    }
                    catch { /* JSON olmayan satır olabilir */ }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI Error => " + ex.Message);
                emotion = "Unknown";
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
            var messages = _context.Messages.OrderByDescending(x => x.Id).ToList();
            return Ok(messages);
        }
    }
}
