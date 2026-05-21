using System.Text.Json.Serialization;

namespace NotionFlow.Api.DTOs
{
    public record GeneratedQuestionDto(
        [property: JsonPropertyName("id")] int Id,
        [property: JsonPropertyName("text")] string Text,
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("options")] List<string>? Options
    );

    public record GeneratedQuizDto(
        [property: JsonPropertyName("title")] string Title,
        [property: JsonPropertyName("questions")] List<GeneratedQuestionDto> Questions
    );
}
