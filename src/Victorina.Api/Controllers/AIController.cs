using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Victorina.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AIController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AIController> _logger;
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromMinutes(5) // Increase timeout for larger requests
    };

    public AIController(IConfiguration configuration, ILogger<AIController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("generate-questions")]
    public async Task<IActionResult> GenerateQuestions([FromBody] GenerateQuestionsRequest request)
    {
        var apiKey = _configuration["DeepSeek:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            return BadRequest(new { error = "DeepSeek API key not configured" });
        }

        // Validate reasonable limits to avoid excessive API costs
        if (request.Count > 10)
        {
            return BadRequest(new { error = "Maximum 10 questions per request" });
        }

        if (request.Languages.Length > 6)
        {
            return BadRequest(new { error = "Maximum 6 languages per request" });
        }

        try
        {
            var prompt = BuildPrompt(request);
            _logger.LogInformation("Generating {Count} questions in {Languages} languages. Total expected: {Total}",
                request.Count, request.Languages.Length, request.Count * request.Languages.Length);

            var deepSeekRequest = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that generates quiz questions in multiple languages. Always respond with valid JSON only, no additional text." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.9, // Increased for more variety
                max_tokens = 8000
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(deepSeekRequest),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            _logger.LogInformation("Sending request to DeepSeek API...");
            var startTime = DateTime.UtcNow;

            var response = await _httpClient.PostAsync(
                "https://api.deepseek.com/v1/chat/completions",
                requestContent
            );

            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            _logger.LogInformation("DeepSeek API responded in {Elapsed:F2} seconds", elapsed);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("DeepSeek API error: {Error}", errorContent);
                return StatusCode((int)response.StatusCode, new { error = "DeepSeek API error", details = errorContent });
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);

            if (deepSeekResponse?.choices == null || deepSeekResponse.choices.Length == 0)
            {
                return BadRequest(new { error = "No response from DeepSeek" });
            }

            var content = deepSeekResponse.choices[0].message.content;

            // Clean up the response - remove markdown code blocks if present
            content = content.Trim();
            if (content.StartsWith("```json"))
            {
                content = content.Substring(7);
            }
            else if (content.StartsWith("```"))
            {
                content = content.Substring(3);
            }
            if (content.EndsWith("```"))
            {
                content = content.Substring(0, content.Length - 3);
            }
            content = content.Trim();

            // Parse the generated questions
            var generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (generatedQuestions == null || generatedQuestions.Count == 0)
            {
                return BadRequest(new { error = "No questions generated", rawResponse = content });
            }

            // Validate that we have all expected questions
            var expectedCount = request.Count * request.Languages.Length;
            if (generatedQuestions.Count != expectedCount)
            {
                _logger.LogWarning("Expected {Expected} questions but got {Actual}", expectedCount, generatedQuestions.Count);
            }

            return Ok(generatedQuestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating questions");
            return StatusCode(500, new { error = "Failed to generate questions", details = ex.Message });
        }
    }

    private string BuildPrompt(GenerateQuestionsRequest request)
    {
        var languagesStr = string.Join(", ", request.Languages);
        var categoryInfo = request.CategoryName != null ? $" about '{request.CategoryName}'" : "";
        var randomSeed = Guid.NewGuid().ToString("N").Substring(0, 8); // Add randomness

        return $@"Generate {request.Count} UNIQUE and CREATIVE quiz questions{categoryInfo} in the following languages: {languagesStr}.

IMPORTANT: Make the questions diverse and interesting. Avoid common or obvious questions. Use seed: {randomSeed} for inspiration.

Each question should have:
- A question text (clear, concise, and engaging)
- 4 answer options (one correct, three incorrect)
- An explanation of the correct answer
- A difficulty level (easy/medium/hard)

Critical requirements:
- Questions with the same meaning across ALL languages MUST have the SAME translationGroupId (generate a new UUID for each unique question)
- Each question must appear in ALL requested languages with the same translationGroupId
- All questions must be appropriate for a quiz game
- AVOID COMMON OR OBVIOUS QUESTIONS - be creative and think of unusual facts or interesting angles
- Make each question UNIQUE - no repetitive themes or similar questions
- Wrong answers must be plausible but clearly incorrect
- Maintain cultural sensitivity across all languages
- Ensure translations are natural and idiomatic, not literal word-for-word

Example: If generating 2 questions in 2 languages (ru, de):
- Question 1: 2 entries with translationGroupId=""abc-123"" (one in ru, one in de)
- Question 2: 2 entries with translationGroupId=""def-456"" (one in ru, one in de)
Total: 4 entries (2 questions x 2 languages)

Return ONLY a valid JSON array with this exact structure (no markdown, no additional text):
[
  {{
    ""translationGroupId"": ""uuid-here"",
    ""languageCode"": ""ru"",
    ""text"": ""Question text in Russian"",
    ""correctAnswer"": ""Correct answer"",
    ""wrongAnswer1"": ""Wrong answer 1"",
    ""wrongAnswer2"": ""Wrong answer 2"",
    ""wrongAnswer3"": ""Wrong answer 3"",
    ""explanation"": ""Explanation in Russian"",
    ""difficulty"": ""medium""
  }},
  {{
    ""translationGroupId"": ""same-uuid-as-above"",
    ""languageCode"": ""de"",
    ""text"": ""Same question in German"",
    ""correctAnswer"": ""Correct answer in German"",
    ""wrongAnswer1"": ""Wrong answer 1 in German"",
    ""wrongAnswer2"": ""Wrong answer 2 in German"",
    ""wrongAnswer3"": ""Wrong answer 3 in German"",
    ""explanation"": ""Explanation in German"",
    ""difficulty"": ""medium""
  }}
]";
    }
}

public record GenerateQuestionsRequest(
    int Count,
    string[] Languages,
    string? CategoryName
);

public record GeneratedQuestion(
    string TranslationGroupId,
    string LanguageCode,
    string Text,
    string CorrectAnswer,
    string WrongAnswer1,
    string WrongAnswer2,
    string WrongAnswer3,
    string? Explanation,
    string Difficulty
);

public class DeepSeekResponse
{
    public Choice[] choices { get; set; } = Array.Empty<Choice>();
}

public class Choice
{
    public Message message { get; set; } = new();
}

public class Message
{
    public string content { get; set; } = "";
}
