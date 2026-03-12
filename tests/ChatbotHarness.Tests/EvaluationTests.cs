using Azure;
using Azure.AI.OpenAI;
using ChatbotHarness;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Xunit;

namespace ChatbotHarness.Tests;

public class EvaluationTests
{
    private const string SystemPrompt =
        "You are a helpful assistant. Answer concisely in 1-5 sentences.";

    public static TheoryData<string> Questions => new()
    {
        "What is the difference between a planet and a star?",
        "Explain the water cycle in simple terms.",
        "What is the difference between encryption and hashing?",
        "What is the purpose of unit tests versus integration tests?",
        "Explain the concept of technical debt.",
    };

    [SkippableTheory]
    [MemberData(nameof(Questions))]
    [Trait("Category", "Integration")]
    public async Task Chatbot_Response_Is_Coherent_And_Relevant(string question)
    {
        var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deployment = Environment.GetEnvironmentVariable("AZURE_OPENAI_CHAT_DEPLOYMENT");

        Skip.If(
            endpoint is null || apiKey is null || deployment is null,
            "Azure OpenAI environment variables not configured — skipping integration test.");

        var bot = new AzureOpenAiChatbot(SystemPrompt);
        var answer = await bot.AskAsync(question);

        var azureOpenAiClient = new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey!));
        IChatClient judgeClient = azureOpenAiClient.GetChatClient(deployment!).AsIChatClient();

        var chatConfig = new ChatConfiguration(judgeClient);
        var messages = new List<ChatMessage> { new(ChatRole.User, question) };
        var modelResponse = new ChatResponse(new ChatMessage(ChatRole.Assistant, answer));

        var coherenceEvaluator = new CoherenceEvaluator();
        var relevanceEvaluator = new RelevanceEvaluator();

        var coherenceResult = await coherenceEvaluator.EvaluateAsync(messages, modelResponse, chatConfig);
        var relevanceResult = await relevanceEvaluator.EvaluateAsync(messages, modelResponse, chatConfig);

        Assert.True(
            coherenceResult.TryGet<NumericMetric>(CoherenceEvaluator.CoherenceMetricName, out var coherenceScore),
            "Coherence metric was not returned by the evaluator.");
        Assert.True(
            relevanceResult.TryGet<NumericMetric>(RelevanceEvaluator.RelevanceMetricName, out var relevanceScore),
            "Relevance metric was not returned by the evaluator.");

        Assert.True(
            coherenceScore!.Value >= 3,
            $"Coherence score {coherenceScore.Value} is below the minimum threshold of 3.");
        Assert.True(
            relevanceScore!.Value >= 3,
            $"Relevance score {relevanceScore.Value} is below the minimum threshold of 3.");
    }
}
