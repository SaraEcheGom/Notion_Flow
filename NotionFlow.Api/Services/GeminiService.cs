using System.Text;
using System.Text.Json;
using NotionFlow.Api.DTOs;

namespace NotionFlow.Api.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        private static readonly JsonSerializerOptions ResponseJsonOptions =
            new() { PropertyNameCaseInsensitive = true };

        private const string SystemPrompt =
            "Eres un extractor de cuestionarios. Cuando recibes una imagen de un examen o taller escrito a mano, DEBES:\n" +
            "1. Leer todo el contenido manuscrito visible.\n" +
            "2. Extraer ÚNICAMENTE las preguntas y sus opciones de respuesta (si existen).\n" +
            "3. Responder EXCLUSIVAMENTE con un JSON válido, sin texto adicional, sin markdown, sin backticks.\n\n" +
            "El JSON debe tener exactamente esta estructura:\n" +
            "{\"title\":\"título inferido del examen, o 'Cuestionario sin título' si no hay uno\"," +
            "\"questions\":[{\"id\":1,\"text\":\"enunciado de la pregunta\"," +
            "\"type\":\"open | single_choice | multiple_choice\",\"options\":[\"opción 1\",\"opción 2\"]}]}\n\n" +
            "Reglas para el tipo:\n" +
            "- \"open\": pregunta abierta sin opciones. No incluyas 'options'.\n" +
            "- \"single_choice\": opciones donde solo una es correcta.\n" +
            "- \"multiple_choice\": opciones donde pueden ser correctas varias.\n\n" +
            "Si no hay preguntas visibles devuelve: {\"title\":\"Sin preguntas\",\"questions\":[]}";

        public GeminiService(HttpClient httpClient, IConfiguration config, ILogger<GeminiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
            _model = config["Gemini:Model"] ?? "gemini-1.5-flash";
        }

        public async Task<GeneratedQuizDto> GenerateQuizFromImageAsync(IFormFile image)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new Exception("La clave de API de Gemini no está configurada (Gemini:ApiKey).");

            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var base64Data = Convert.ToBase64String(ms.ToArray());
            var mimeType = ResolveMediaType(image.ContentType);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new object[]
                        {
                            new
                            {
                                inlineData = new
                                {
                                    mimeType,
                                    data = base64Data
                                }
                            },
                            new { text = "Extrae las preguntas de este examen o taller." }
                        }
                    }
                },
                systemInstruction = new
                {
                    parts = new[] { new { text = SystemPrompt } }
                },
                generationConfig = new
                {
                    maxOutputTokens = 2048,
                    temperature = 0.1
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"v1beta/models/{_model}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API respondió {Status}: {Body}", response.StatusCode, errorBody);
                throw new Exception($"Error de la API de Gemini ({(int)response.StatusCode}). Verifica la clave de API.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta Gemini: {Response}", responseJson);

            string textContent;
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                textContent = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString()
                    ?? throw new Exception("Respuesta vacía del modelo.");
            }
            catch (Exception ex) when (ex.Message != "Respuesta vacía del modelo.")
            {
                _logger.LogError(ex, "No se pudo leer la respuesta de Gemini");
                throw new Exception("Respuesta inesperada de la API de Gemini.");
            }

            var cleanJson = StripMarkdownFences(textContent.Trim());

            try
            {
                return JsonSerializer.Deserialize<GeneratedQuizDto>(cleanJson, ResponseJsonOptions)
                    ?? throw new Exception("El modelo devolvió un JSON nulo.");
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON del modelo no parseable: {Content}", cleanJson);
                throw new Exception("La imagen no pudo procesarse como cuestionario. Asegúrate de que sea legible.");
            }
        }

        private static string StripMarkdownFences(string text)
        {
            if (!text.StartsWith("```"))
                return text;
            var lines = text.Split('\n');
            return string.Join('\n', lines.Skip(1).TakeWhile(l => !l.TrimStart().StartsWith("```"))).Trim();
        }

        private static string ResolveMediaType(string? contentType) =>
            contentType?.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => "image/jpeg",
                "image/png"  => "image/png",
                "image/gif"  => "image/gif",
                "image/webp" => "image/webp",
                _            => "image/jpeg"
            };
    }
}
