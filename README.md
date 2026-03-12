# test-harness-monster

A minimal proof-of-concept C# chatbot test harness using [Semantic Kernel](https://github.com/microsoft/semantic-kernel) and xUnit, targeting Azure OpenAI.

## Project structure

```
test-harness-monster.sln
src/
  ChatbotHarness/          # Library — AzureOpenAiChatbot wrapper
tests/
  ChatbotHarness.Tests/    # xUnit integration tests (20 generic questions)
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- An Azure OpenAI resource with a chat-completion deployment

## Configuration

Set the following environment variables before running the integration tests:

| Variable | Description |
|---|---|
| `AZURE_OPENAI_ENDPOINT` | Your Azure OpenAI endpoint, e.g. `https://YOUR-RESOURCE.openai.azure.com/` |
| `AZURE_OPENAI_API_KEY` | Your Azure OpenAI API key |
| `AZURE_OPENAI_CHAT_DEPLOYMENT` | The deployment name for your chat model, e.g. `gpt-4o-mini` |

**Linux / macOS**

```bash
export AZURE_OPENAI_ENDPOINT="https://YOUR-RESOURCE.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_CHAT_DEPLOYMENT="gpt-4o-mini"
```

**Windows (PowerShell)**

```powershell
$env:AZURE_OPENAI_ENDPOINT  = "https://YOUR-RESOURCE.openai.azure.com/"
$env:AZURE_OPENAI_API_KEY   = "your-api-key"
$env:AZURE_OPENAI_CHAT_DEPLOYMENT = "gpt-4o-mini"
```

## Building

```bash
dotnet build test-harness-monster.sln
```

## Running integration tests

```bash
dotnet test --filter "Category=Integration"
```

Tests are automatically **skipped** (not failed) when the environment variables above are not set, so the suite is always safe to run in a vanilla CI environment without Azure credentials.

## AI-based quality evaluation

In addition to the basic integration tests, the test suite includes AI-powered quality evaluation using [Microsoft.Extensions.AI.Evaluation](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai-evaluation).

### How it works

`EvaluationTests.cs` asks the chatbot each question and then passes the question and response to an AI judge model (backed by the same Azure OpenAI deployment) that scores the answer on two dimensions:

| Evaluator | What it measures | Source |
|---|---|---|
| `CoherenceEvaluator` | Whether the response reads as a coherent, well-structured reply | `Microsoft.Extensions.AI.Evaluation.Quality` |
| `RelevanceEvaluator` | Whether the response is relevant to the question asked | `Microsoft.Extensions.AI.Evaluation.Quality` |

Each metric is scored on a **1–5 scale**; the tests assert that both scores are **≥ 3**.

### Environment variables

The evaluation tests use the **same environment variables** as the generic integration tests — no extra configuration is required:

| Variable | Description |
|---|---|
| `AZURE_OPENAI_ENDPOINT` | Your Azure OpenAI endpoint |
| `AZURE_OPENAI_API_KEY` | Your Azure OpenAI API key |
| `AZURE_OPENAI_CHAT_DEPLOYMENT` | The deployment name used for both the chatbot and the judge model |

### Running only evaluation tests

```bash
dotnet test --filter "Category=Integration"
```
