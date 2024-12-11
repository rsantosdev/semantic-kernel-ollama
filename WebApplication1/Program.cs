using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.TextGeneration;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOllamaTextGeneration("phi3:latest", new Uri("http://localhost:11434"));
builder.Services.AddOllamaChatCompletion("phi3:latest", new Uri("http://localhost:11434"));

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(s =>
{
    s.Theme = ScalarTheme.BluePlanet;
});

app.MapGet("/", () => "Hello World!");

app.MapPost("/generate", async (ChatRequest chatRequest, ITextGenerationService textGenerationService) =>
{
    var response = await textGenerationService
        .GetTextContentsAsync(chatRequest.Message);
    return response.First().Text;
});

var history = new ChatHistory();
history.AddSystemMessage("You are a helpful assistant.");

app.MapPost("/chat", async (ChatRequest chatRequest, IChatCompletionService chatCompletionService) =>
{
    history.AddUserMessage(chatRequest.Message);
    var response = await chatCompletionService.GetChatMessageContentAsync(history);
    history.AddMessage(response.Role, response.Content ?? string.Empty);
    return response.Content;
});

app.Run();

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}