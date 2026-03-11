using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace ChatbotHarness;

public sealed class AzureOpenAiChatbot
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ChatHistory _history;

    public AzureOpenAiChatbot(string systemPrompt)
    {
        var endpoint = RequireEnv("AZURE_OPENAI_ENDPOINT");
        var apiKey = RequireEnv("AZURE_OPENAI_API_KEY");
        var deployment = RequireEnv("AZURE_OPENAI_CHAT_DEPLOYMENT");

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: deployment,
            endpoint: endpoint,
            apiKey: apiKey);

        _kernel = builder.Build();
        _chat = _kernel.GetRequiredService<IChatCompletionService>();

        _history = new ChatHistory();
        _history.AddSystemMessage(systemPrompt);
    }

    public async Task<string> AskAsync(string question, CancellationToken ct = default)
    {
        _history.AddUserMessage(question);

        var settings = new AzureOpenAIPromptExecutionSettings
        {
            Temperature = 0.2
        };

        var response = await _chat.GetChatMessageContentAsync(
            _history,
            executionSettings: settings,
            kernel: _kernel,
            cancellationToken: ct);

        var text = response?.Content?.Trim() ?? string.Empty;
        _history.AddAssistantMessage(text);
        return text;
    }

    private static string RequireEnv(string name)
        => Environment.GetEnvironmentVariable(name)
           ?? throw new InvalidOperationException(
               $"Missing required environment variable: {name}. " +
               $"Set this variable before running integration tests.");
}
