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

        // ✅ Hugging Face Sentiment Analysis API
        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/run/predict";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // ✅ REGISTER NEW USER
        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Nickname))
                return BadRequest(new { error = "Nickname is required" });

            _context.Users.Add(user);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                userId = user.Id,
                nickname = user.Nickname
            });
        }

        // ✅ SEND MESSAGE + AI EMOTION PREDICTION
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == message.UserId);
            if (!userExists)
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

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseText);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("data", out JsonElement dataArray) &&
                            dataArray.ValueKind == JsonValueKind.Array &&
                            dataArray.GetArrayLength() > 0)
                        {
                            var first = dataArray[0];

                            if (first.TryGetProperty("label", out JsonElement labelElement))
                                emotion = labelElement.GetString() ?? "Unknown";
                        }
                    }
                    catch
                    {
                        Console.WriteLine("⚠ AI JSON Parse Error!");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI ERROR: " + ex.Message);
            }

            // ✅ Save to DB
            message.Emotion = emotion;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // ✅ Response to Client
            return Ok(new
            {
                success = true,
                messageId = message.Id,
                message = message.Text,
                emotion = emotion
            });
        }

        // ✅ GET ALL MESSAGES
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
