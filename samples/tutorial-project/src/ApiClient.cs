// 🎓 TUTORIAL FILE - API integration with FAKE API keys

using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace TutorialApp
{
    public class ApiClient
    {
        // ⚠️ FAKE OPENAI API KEY (will be detected and masked as OPENAI_KEY_0)
        private const string OpenAiApiKey = "sk-proj-abcdefghijklmnopqrstuvwxyz1234567890ABCD";
        
        // ⚠️ FAKE ANTHROPIC API KEY (will be detected and masked as ANTHROPIC_KEY_0)
        private const string AnthropicApiKey = "sk-ant-api03-FakeKeyForTutorialPurposesOnly1234567890";
        
        // ⚠️ FAKE GITHUB TOKEN (will be detected and masked as GITHUB_TOKEN_0)
        private const string GitHubToken = "ghp_FakeGitHubPersonalAccessToken123456789";
        
        // ⚠️ FAKE SLACK WEBHOOK (will be detected and masked as SLACK_WEBHOOK_0)
        private const string SlackWebhook = "https://hooks.slack.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXX";
        
        private readonly HttpClient _httpClient;
        
        public ApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {OpenAiApiKey}");
        }
        
        public async Task<string> CallOpenAiAsync(string prompt)
        {
            Console.WriteLine($"🤖 Calling OpenAI with key: {OpenAiApiKey.Substring(0, 10)}...");
            // Fake API call for tutorial
            return "AI response here";
        }
        
        public async Task SendSlackNotificationAsync(string message)
        {
            Console.WriteLine($"📢 Sending to Slack: {SlackWebhook}");
            // Fake webhook call for tutorial
        }
    }
}

