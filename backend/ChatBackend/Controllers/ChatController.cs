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

        // ✅ Hugging Face API - NEW endpoint
        private readonly string AI_URL = "https://humeyraertas-chat-sentiment-analyzer.hf.space/api/predict/";

        public ChatController(AppDbContext context)
        {
            _context = context;
            _httpClient = new HttpClient();
        }

        // ✅ Register user
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

        // ✅ Send Message + AI Prediction
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            // ✅ Validate UserId
            var userExists = await _context.Users.AnyAsync(x => x.Id == message.UserId);
            if (!userExists)
                return BadRequest(new { success = false, error = "Invalid UserId" });

            message.CreatedAt = DateTime.UtcNow;

            try
            {
                // ✅ OpenAI payload format
                var payload = new { text = message.Text };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                Console.WriteLine("🔍 AI Response: " + responseText);

                if (!response.IsSuccessStatusCode)
                {
                    message.Emotion = "Unknown";
                }
                else
                {
                    using var doc = JsonDocument.Parse(responseText);
                    var root = doc.RootElement;

                    var label = root.GetProperty("label").GetString() ?? "NEUTRAL";
                    message.Emotion = label;
                }

                // ✅ Save to database
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    userId = message.UserId,
                    text = message.Text,
                    emotion = message.Emotion
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ AI ERROR: " + ex.Message);
                return Ok(new { success = false, emotion = "Unknown", detail = ex.Message });
            }
        }

        // ✅ Get messages
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
