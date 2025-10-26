using Microsoft.AspNetCore.Mvc;
using ChatBackend.Data;
using ChatBackend.Models;
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
            _httpClient.Timeout = TimeSpan.FromSeconds(20);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Nickname))
                return BadRequest(new { error = "Nickname is required" });

            _context.Users.Add(user);
            _context.SaveChanges();
            return Ok(user);
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (message == null || string.IsNullOrWhiteSpace(message.Text) || message.UserId <= 0)
                return BadRequest(new { error = "userId and text are required" });

            try
            {
                // Gradio run/predict payload
                var payload = new { data = new[] { message.Text } };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(AI_URL, content);
                var responseText = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // AI servisinden hata dönerse nötr kaydedelim ama ham cevap da dönsün (debug amaçlı)
                    message.Emotion = "😐 Nötr";
                    message.CreatedAt = DateTime.UtcNow;
                    _context.Messages.Add(message);
                    _context.SaveChanges();

                    return Ok(new
                    {
                        message.Id,
                        message.UserId,
                        message.Text,
                        Emotion = message.Emotion,
                        aiRaw = responseText
                    });
                }

                // JSON güvenli çözümleme (çeşitli Gradio yanıt şekillerine dayanıklı)
                string labelRaw = TryExtractLabel(responseText);

                // Etiketi Türkçe + emojiye map et
                string labelPretty = labelRaw.ToUpperInvariant() switch
                {
                    "POSITIVE" => "😊 Pozitif",
                    "NEGATIVE" => "😞 Negatif",
                    "NEUTRAL"  => "😐 Nötr",
                    _ => "😐 Nötr"
                };

                message.Emotion = labelPretty;
                message.CreatedAt = DateTime.UtcNow;

                _context.Messages.Add(message);
                _context.SaveChanges();

                return Ok(new
                {
                    message.Id,
                    message.UserId,
                    message.Text,
                    Emotion = message.Emotion
                });
            }
            catch (Exception ex)
            {
                // En azından kayıt düşsün, servis patlasa bile
                message.Emotion = "😐 Nötr";
                message.CreatedAt = DateTime.UtcNow;

                _context.Messages.Add(message);
                _context.SaveChanges();

                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("messages")]
        public IActionResult GetMessages()
        {
            var messages = _context.Messages
                .OrderByDescending(m => m.Id)
                .Select(m => new
                {
                    m.Id,
                    m.UserId,
                    m.Text,
                    m.Emotion,
                    m.CreatedAt
                })
                .ToList();

            return Ok(messages);
        }

        /// <summary>
        /// Gradio /run/predict yanıtları birden fazla biçimde gelebiliyor.
        /// Bu yardımcı fonksiyon; şu olası formatların hepsini dener:
        /// 1) {"data":[[{"label":"POSITIVE","score":0.9}]]}
        /// 2) {"data":[{"label":"POSITIVE","score":0.9}]}
        /// 3) {"label":"POSITIVE","score":0.9}
        /// 4) {"data":[{"confidences":[{"label":"positive","confidence":0.9}, ...]}]}  (bazı gradio bileşenleri)
        /// Bulamazsa "NEUTRAL" döner.
        /// </summary>
        private static string TryExtractLabel(string responseText)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                // 3) Root'ta "label"
                if (root.TryGetProperty("label", out var rootLabel))
                    return rootLabel.GetString() ?? "NEUTRAL";

                // "data" exist?
                if (!root.TryGetProperty("data", out var data))
                    return "NEUTRAL";

                // data bir dizi olmalı
                if (data.ValueKind != JsonValueKind.Array || data.GetArrayLength() == 0)
                    return "NEUTRAL";

                var first = data[0];

                // 1) İç içe dizi ise: data[0][0].label
                if (first.ValueKind == JsonValueKind.Array && first.GetArrayLength() > 0)
                {
                    var inner = first[0];
                    if (inner.ValueKind == JsonValueKind.Object && inner.TryGetProperty("label", out var innerLabel))
                        return innerLabel.GetString() ?? "NEUTRAL";
                }

                // 2) Düz obje: data[0].label
                if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("label", out var directLabel))
                    return directLabel.GetString() ?? "NEUTRAL";

                // 4) confidences: data[0].confidences[?] -> en yüksek olan?
                if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("confidences", out var confs)
                    && confs.ValueKind == JsonValueKind.Array && confs.GetArrayLength() > 0)
                {
                    // En yüksek confidence'ı al
                    double best = -1;
                    string bestLabel = "NEUTRAL";
                    foreach (var c in confs.EnumerateArray())
                    {
                        if (c.TryGetProperty("label", out var l) && c.TryGetProperty("confidence", out var s))
                        {
                            var lbl = l.GetString() ?? "";
                            var sc = s.GetDouble();
                            if (sc > best)
                            {
                                best = sc;
                                bestLabel = lbl.ToUpperInvariant();
                            }
                        }
                    }
                    return bestLabel switch
                    {
                        "POSITIVE" => "POSITIVE",
                        "NEGATIVE" => "NEGATIVE",
                        "NEUTRAL"  => "NEUTRAL",
                        _ => "NEUTRAL"
                    };
                }

                return "NEUTRAL";
            }
            catch
            {
                return "NEUTRAL";
            }
        }
    }
}
