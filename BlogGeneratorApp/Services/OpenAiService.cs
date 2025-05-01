using System.Text.Json;
using System.Text;

namespace BlogGeneratorApp.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OpenAIService> _logger;

        public OpenAIService(IConfiguration configuration, ILogger<OpenAIService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["OpenAI:ApiKey"];
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GenerateBlogContent(string topic, string category)
        {
            try
            {
                var requestBody = new
                {
                    model = "gpt-4", // You can use gpt-3.5-turbo for lower cost
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a professional blog writer specializing in technology and software development. " +
                                      "Write informative, engaging content with a friendly tone. Include section headers, " +
                                      "relevant examples, and actionable advice. Format in HTML for web publishing."
                        },
                        new
                        {
                            role = "user",
                            content = $"Write a comprehensive blog post about '{topic}' for the category '{category}'. " +
                                      "Include an introduction, 3-4 main sections with headers, and a conclusion. " +
                                      "The blog should be between 800-1200 words and include some code examples if relevant. " +
                                      "Use proper HTML formatting with <p>, <h2>, <h3>, <code>, <ul>, and <li> tags where appropriate. " +
                                      "Do not include a title in the content."
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 2500
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                return responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blog content using OpenAI for topic: {Topic}", topic);
                return $"<p>Error generating content for {topic}. Please try again later.</p>";
            }
        }

        public async Task<string> GenerateBlogTitle(string topic, string category)
        {
            try
            {
                var requestBody = new
                {
                    model = "gpt-3.5-turbo", // Using a smaller model for title generation is sufficient
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are a professional headline writer for a technology blog. Create catchy, SEO-friendly titles."
                        },
                        new
                        {
                            role = "user",
                            content = $"Generate a catchy, SEO-friendly blog title about '{topic}' for the category '{category}'. " +
                                      "The title should be concise (under 70 characters) and compelling. Return only the title text."
                        }
                    },
                    temperature = 0.8,
                    max_tokens = 50
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                var title = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    .Trim('"', ' ');

                return title;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blog title using OpenAI for topic: {Topic}", topic);
                return $"Latest Update on {topic}";
            }
        }

        public async Task<string> GenerateBlogSummary(string content, int maxLength = 250)
        {
            try
            {
                var contentPreview = content.Length > 1500 ? content.Substring(0, 1500) + "..." : content;

                var requestBody = new
                {
                    model = "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You are an expert at creating concise summaries of longer content. " +
                                      "Create engaging summaries that encourage readers to read the full article."
                        },
                        new
                        {
                            role = "user",
                            content = $"Create a concise, engaging summary of this blog post, limited to {maxLength} characters. " +
                                      $"The summary should highlight the key points and entice readers to read the full post.\n\n" +
                                      $"CONTENT: {contentPreview}"
                        }
                    },
                    temperature = 0.7,
                    max_tokens = 150
                };

                var contentres = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", contentres);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

                var summary = responseObject
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString()
                    .Trim();

                return summary.Length > maxLength ? summary.Substring(0, maxLength) + "..." : summary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating blog summary using OpenAI");
                return content.Length > maxLength ? content.Substring(0, maxLength) + "..." : content;
            }
        }
    }
}
