using ChatbotHarness;
using Xunit;

namespace ChatbotHarness.Tests;

public class GenericQuestionTests
{
    private const string SystemPrompt =
        "You are a helpful assistant. Answer concisely in 1-5 sentences.";

    public static TheoryData<string> Questions => new()
    {
        "What is the difference between a planet and a star?",
        "Explain the water cycle in simple terms.",
        "What are good strategies for managing time during a busy week?",
        "Give three tips for writing a clear email to a coworker.",
        "What is a healthy way to handle stress before a presentation?",
        "Explain what an API is to a non-technical person.",
        "What is the difference between encryption and hashing?",
        "How do I choose a strong password and manage them safely?",
        "What are the pros and cons of remote work?",
        "Summarise what photosynthesis is and why it matters.",
        "What is compound interest? Provide a simple example.",
        "How can I improve my sleep routine?",
        "What are some common causes of bugs in software projects?",
        "What is the purpose of unit tests versus integration tests?",
        "Explain the concept of technical debt.",
        "What are practical steps to learn a new programming language?",
        "What is a balanced diet? Give an example day of meals.",
        "How does a thermostat maintain temperature?",
        "What is a good approach to resolving conflicts in a team?",
        "Explain the difference between TCP and UDP at a high level."
    };

    [SkippableTheory]
    [MemberData(nameof(Questions))]
    [Trait("Category", "Integration")]
    public async Task Chatbot_Returns_Reasonable_Response(string question)
    {
        Skip.If(
            Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") is null ||
            Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") is null ||
            Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT") is null,
            "Azure OpenAI environment variables not configured — skipping integration test.");

        var bot = new AzureOpenAiChatbot(SystemPrompt);
        var answer = await bot.AskAsync(question);

        Assert.False(string.IsNullOrWhiteSpace(answer));
        Assert.InRange(answer.Length, 5, 1500);
        Assert.DoesNotContain("As an AI language model", answer, StringComparison.OrdinalIgnoreCase);
    }
}
