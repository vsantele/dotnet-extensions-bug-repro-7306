using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

var options = new OpenAIClientOptions
{
    Endpoint = new Uri("http://localhost:5209/calculator/v1/")
};

var client = new OpenAIClient(new ApiKeyCredential("not-needed-for-local"), options);
#pragma warning disable OPENAI001
var chatClient = client.GetResponsesClient("gpt-5-mini").AsIChatClient();
#pragma warning restore OPENAI001

try
{
    var messages = new List<ChatMessage>
    {
        new(ChatRole.User, "What is the result of 1258956+15485215. Use your tools to solve this problem.")
    };

    var chatResponse = await chatClient
        .GetStreamingResponseAsync(messages)
        .ToChatResponseAsync();
    Console.Write("Streaming response: ");
    Console.WriteLine(chatResponse.Messages[0].Contents.OfType<FunctionResultContent>().Count());
    
    var chatResponse2 = await chatClient.GetResponseAsync(messages);
    Console.Write("Full response: ");
    Console.WriteLine(chatResponse2.Messages[0].Contents.OfType<FunctionResultContent>().Count());
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("Make sure the AgentService is running on http://localhost:5209");
    Console.WriteLine($"\nStack trace: {ex.StackTrace}");
}
