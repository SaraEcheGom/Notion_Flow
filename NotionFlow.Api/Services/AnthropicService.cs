using System.Text;
using System.Text.Json;
using NotionFlow.Api.DTOs;

namespace NotionFlow.Api.Services
{
    public class AnthropicService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicService> _logger;
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
            "- \"open\": pregunta abierta sin opciones (respuesta de texto libre). No incluyas 'options'.\n" +
            "- \"single_choice\": pregunta con opciones donde solo una es correcta.\n" +
            "- \"multiple_choice\": pregunta con opciones donde pueden ser correctas varias.\n\n" +
            "Si no hay preguntas visibles devuelve: {\"title\":\"Sin preguntas\",\"questions\":[]}";

        public AnthropicService(HttpClient httpClient, IConfiguration config, ILogger<AnthropicService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _model = config["Anthropic:Model"] ?? "claude-sonnet-4-20250514";

            var apiKey = config["Anthropic:ApiKey"];
            if (!string.IsNullOrWhiteSpace(apiKey))
                _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task<GeneratedQuizDto> GenerateQuizFromImageAsync(IFormFile image)
        {
            using var ms = new MemoryStream();
            await image.CopyToAsync(ms);
            var base64Data = Convert.ToBase64String(ms.ToArray());
            var mediaType = ResolveMediaType(image.ContentType);

            var requestBody = new
            {
                model = _model,
                max_tokens = 2048,
                system = SystemPrompt,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "image",
                                source = new
                                {
                                    type = "base64",
                                    media_type = mediaType,
                                    data = base64Data
                                }
                            },
                            new
                            {
                                type = "text",
                                text = "Extrae las preguntas de este examen o taller."
                            }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("v1/messages", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Anthropic API respondió {Status}: {Body}", response.StatusCode, errorBody);
                throw new Exception($"Error de la API de Anthropic ({(int)response.StatusCode}). Verifica la clave de API.");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Respuesta Anthropic: {Response}", responseJson);

            string textContent;
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                textContent = doc.RootElement
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString()
                    ?? throw new Exception("Respuesta vacía del modelo.");
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "No se pudo leer la respuesta de Anthropic");
                throw new Exception("Respuesta inesperada de la API de Anthropic.");
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
            var inner = lines.Skip(1).TakeWhile(l => !l.TrimStart().StartsWith("```"));
            return string.Join('\n', inner).Trim();
        }

        private static string ResolveMediaType(string? contentType) =>
            contentType?.ToLowerInvariant() switch
            {
                "image/jpeg" or "image/jpg" => "image/jpeg",
                "image/png"                 => "image/png",
                "image/gif"                 => "image/gif",
                "image/webp"                => "image/webp",
                _                           => "image/jpeg"
            };
    }
}
