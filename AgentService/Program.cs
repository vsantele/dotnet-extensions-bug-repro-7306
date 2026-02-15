using System.ClientModel;
using System.ComponentModel;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Responses;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var token = builder.Configuration["Token"]
                    ?? throw new InvalidOperationException(
                        "OpenAI token not configured. Run: dotnet user-secrets set Token YOUR-TOKEN");

var ghModelsClient = new OpenAIClient(
    credential: new ApiKeyCredential(token));

// Register IChatClient for the agent
builder.Services.AddSingleton<IChatClient>(sp =>
{
#pragma warning disable OPENAI001
    var chatClient = ghModelsClient.GetResponsesClient("gpt-5-mini");
    return chatClient.AsIChatClient();
#pragma warning restore OPENAI001
});

// Register Responses API and Conversations API services
builder.AddOpenAIResponses();
builder.AddOpenAIConversations();

// Create calculator agent with tools using factory delegate pattern
var calculatorAgent = builder.AddAIAgent("calculator", (sp, key) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Tools =
            [
                AIFunctionFactory.Create(Add),
                AIFunctionFactory.Create(Subtract),
                AIFunctionFactory.Create(Multiply),
                AIFunctionFactory.Create(Divide)
            ],
            Instructions =
                "You are a helpful calculator assistant. Use the provided tools to perform calculations. Always show your work and explain the steps.",
            Reasoning = new ReasoningOptions()
            {
                Effort = ReasoningEffort.Medium,
            },
#pragma warning disable OPENAI001
            RawRepresentationFactory = _ => new CreateResponseOptions
            {
                IncludedProperties = { IncludedResponseProperty.ReasoningEncryptedContent }
            }
#pragma warning restore OPENAI001
        },
        Name = "calculator"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Map OpenAI protocol endpoints
app.MapOpenAIResponses(calculatorAgent);
app.MapOpenAIConversations();

app.Run();

// Calculator tool functions
[Description("Adds two numbers together")]
static double Add(
    [Description("First number")] double a,
    [Description("Second number")] double b)
{
    Console.WriteLine($"[TOOL CALL] Add({a}, {b})");
    return a + b;
}

[Description("Subtracts the second number from the first")]
static double Subtract(
    [Description("First number")] double a,
    [Description("Second number to subtract")]
    double b)
{
    Console.WriteLine($"[TOOL CALL] Subtract({a}, {b})");
    return a - b;
}

[Description("Multiplies two numbers")]
static double Multiply(
    [Description("First number")] double a,
    [Description("Second number")] double b)
{
    Console.WriteLine($"[TOOL CALL] Multiply({a}, {b})");
    return a * b;
}

[Description("Divides the first number by the second")]
static double Divide(
    [Description("Numerator")] double a,
    [Description("Denominator (cannot be zero)")]
    double b)
{
    Console.WriteLine($"[TOOL CALL] Divide({a}, {b})");
    return b == 0 ? throw new ArgumentException("Cannot divide by zero") : a / b;
}