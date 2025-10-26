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

        // Hugging Face Space API endpoint
        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/run/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // ✅ Kullanıcı Kaydı
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        // ✅ Mesaj Gönder + AI Duygu Analizi
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrEmpty(message.Text))
                return BadRequest(new { error = "Text cannot be empty" });

            try
            {
                // AI servisine gönderilecek payload
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine("AI ✓ Response: " + responseText);

                // JSON Parse (✅ İç içe dizi desteği eklendi)
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                string label = "NEUTRAL";

                if (root.TryGetProperty("data", out JsonElement dataArray)
                    && dataArray.ValueKind == JsonValueKind.Array
                    && dataArray.GetArrayLength() > 0)
                {
                    var innerArray = dataArray[0];
                    if (innerArray.ValueKind == JsonValueKind.Array && innerArray.GetArrayLength() > 0)
                    {
                        var prediction = innerArray[0];
                        if (prediction.TryGetProperty("label", out JsonElement labelElement))
                        {
                            label = labelElement.GetString() ?? "NEUTRAL";
                        }
                    }
                }

                // ✅ Etiketi daha kullanıcı dostu hale getir
                label = label switch
                {
                    "POSITIVE" => "😊 Pozitif",
                    "NEGATIVE" => "😞 Negatif",
                    _ => "😐 Nötr"
                };

                // ✅ Veritabanına kaydet
                message.Emotion = label;
                message.CreatedAt = DateTime.UtcNow;

                _context.Messages.Add(message);
                _context.SaveChanges();

                return Ok(new
                {
                    message.Id,
                    message.UserId,
                    message.Text,
                    Emotion = label
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI Error: " + ex.Message);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ✅ Mesajları Getir
        [HttpGet("messages")]
        public IActionResult GetMessages()
        {
            var messages = _context.Messages
                .OrderByDescending(m => m.Id)
                .Select(m => new {
                    m.Id,
                    m.Text,
                    m.Emotion,
                    m.CreatedAt
                })
                .ToList();

            return Ok(messages);
        }
    }
}
