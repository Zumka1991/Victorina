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

    [HttpPost("generate-questions-stream")]
    public async Task GenerateQuestionsStream([FromBody] GenerateQuestionsRequest request)
    {
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var apiKey = _configuration["DeepSeek:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            await SendEvent("error", new { error = "DeepSeek API key not configured" });
            return;
        }

        if (request.Count > 50)
        {
            await SendEvent("error", new { error = "Maximum 50 questions per request" });
            return;
        }

        if (request.Languages.Length > 7)
        {
            await SendEvent("error", new { error = "Maximum 7 languages per request" });
            return;
        }

        try
        {
            var totalQuestions = request.Count * request.Languages.Length;
            await SendEvent("progress", new {
                stage = "starting",
                message = $"Starting generation of {request.Count} questions in {request.Languages.Length} languages (total: {totalQuestions})",
                progress = 0
            });

            var prompt = BuildPrompt(request);
            await SendEvent("progress", new {
                stage = "requesting",
                message = "Sending request to DeepSeek AI...",
                progress = 10
            });

            var deepSeekRequest = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant that generates quiz questions in multiple languages. Always respond with valid JSON only, no additional text." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.9,
                max_tokens = 8000
            };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(deepSeekRequest),
                Encoding.UTF8,
                "application/json"
            );

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var startTime = DateTime.UtcNow;
            await SendEvent("progress", new {
                stage = "waiting",
                message = "Waiting for AI response (this may take 30-60 seconds)...",
                progress = 30
            });

            var response = await _httpClient.PostAsync(
                "https://api.deepseek.com/v1/chat/completions",
                requestContent
            );

            var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
            await SendEvent("progress", new {
                stage = "received",
                message = $"Received response in {elapsed:F1}s, parsing questions...",
                progress = 70
            });

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("DeepSeek API error: {Error}", errorContent);
                await SendEvent("error", new { error = "DeepSeek API error", details = errorContent });
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var deepSeekResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);

            if (deepSeekResponse?.choices == null || deepSeekResponse.choices.Length == 0)
            {
                await SendEvent("error", new { error = "No response from DeepSeek" });
                return;
            }

            var content = deepSeekResponse.choices[0].message.content;

            // Clean up the response
            content = content.Trim();
            if (content.StartsWith("```json")) content = content.Substring(7);
            else if (content.StartsWith("```")) content = content.Substring(3);
            if (content.EndsWith("```")) content = content.Substring(0, content.Length - 3);
            content = content.Trim();

            await SendEvent("progress", new {
                stage = "parsing",
                message = "Parsing generated questions...",
                progress = 80
            });

            var generatedQuestions = JsonSerializer.Deserialize<List<GeneratedQuestion>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (generatedQuestions == null || generatedQuestions.Count == 0)
            {
                await SendEvent("error", new { error = "No questions generated", rawResponse = content });
                return;
            }

            // Log each question
            var questionGroups = generatedQuestions.GroupBy(q => q.TranslationGroupId).ToList();
            for (int i = 0; i < questionGroups.Count; i++)
            {
                var group = questionGroups[i];
                var firstQuestion = group.First();
                var languages = string.Join(", ", group.Select(q => q.LanguageCode));

                await SendEvent("log", new {
                    message = $"Question {i + 1}/{questionGroups.Count}: \"{firstQuestion.Text}\" [{languages}]",
                    questionNumber = i + 1,
                    totalQuestions = questionGroups.Count
                });

                await Task.Delay(50); // Small delay for smooth UI updates
            }

            await SendEvent("progress", new {
                stage = "complete",
                message = $"Successfully generated {generatedQuestions.Count} questions!",
                progress = 100
            });

            await SendEvent("complete", new { questions = generatedQuestions });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating questions");
            await SendEvent("error", new { error = "Failed to generate questions", details = ex.Message });
        }
    }

    private async Task SendEvent(string eventType, object data)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var json = JsonSerializer.Serialize(data, options);
        await Response.WriteAsync($"event: {eventType}\n");
        await Response.WriteAsync($"data: {json}\n\n");
        await Response.Body.FlushAsync();
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
        if (request.Count > 50)
        {
            return BadRequest(new { error = "Maximum 50 questions per request" });
        }

        if (request.Languages.Length > 7)
        {
            return BadRequest(new { error = "Maximum 7 languages per request" });
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

        var difficultyInfo = request.Difficulty switch
        {
            "easy" => @"
DIFFICULTY: EASY
- Questions should be about well-known facts and basic knowledge
- Suitable for beginners and general audiences
- Answers should be straightforward
- Avoid obscure or specialized knowledge",
            "medium" => @"
DIFFICULTY: MEDIUM
- Questions should require some thinking or general knowledge
- Suitable for average quiz players
- May include some less common facts
- Balance between accessible and challenging",
            "hard" => @"
DIFFICULTY: HARD
- Questions should be challenging and require deeper knowledge
- May include specialized or obscure facts
- Suitable for experienced players
- Test detailed understanding of topics",
            _ => @"
DIFFICULTY: MIXED (easy/medium/hard)
- Generate a mix of easy, medium, and hard questions
- Vary the difficulty across the question set"
        };

        var difficultyLevel = request.Difficulty ?? "medium";

        return $@"Generate {request.Count} UNIQUE and CREATIVE quiz questions{categoryInfo} in the following languages: {languagesStr}.
{difficultyInfo}

IMPORTANT: Make the questions diverse and interesting. Avoid common or obvious questions. Use seed: {randomSeed} for inspiration.

Each question should have:
- A question text (clear, concise, and engaging)
- 4 answer options (one correct, three incorrect)
- An explanation of the correct answer
- A difficulty level: ""{difficultyLevel}""

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
    string? CategoryName,
    string? Difficulty // "easy", "medium", "hard", or null for mixed
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
