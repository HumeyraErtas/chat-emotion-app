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
                // Send request to HuggingFace
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(AI_URL, content);

                var jsonResp = await response.Content.ReadAsStringAsync();
                Console.WriteLine("🔍 Step1 Response: " + jsonResp);

                using var step1Doc = JsonDocument.Parse(jsonResp);

                if (!step1Doc.RootElement.TryGetProperty("event_id", out JsonElement eventIdElement))
                {
                    Console.WriteLine("⚠ No event_id received.");
                }
                else
                {
                    string? eventId = eventIdElement.GetString();
                    if (!string.IsNullOrEmpty(eventId))
                    {
                        await Task.Delay(1200); // wait for async HF space processing

                        var resultResp = await _httpClient.GetAsync($"{AI_URL}/{eventId}");
                        var resultJson = await resultResp.Content.ReadAsStringAsync();
                        Console.WriteLine("✅ Step2 Response: " + resultJson);

                        using var docResult = JsonDocument.Parse(resultJson);
                        var label = docResult.RootElement.GetProperty("data")[0].GetProperty("label").GetString();

                        emotion = label?.ToUpper() switch
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
                Console.WriteLine("❌ AI Error: " + ex.Message);
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
