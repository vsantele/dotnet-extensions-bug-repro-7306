# Minimal reproduction for dotnet/extensions#7306

This is a minimal repro for the bug dotnet/extensions#7306

# Step to reproduces

1. Clone this repo
2. Set an openAI key in the AgentService project

```bash
dotnet user-secrets set Token {YourOpenAIKey}
```

3. Launch the server

```bash
cd AgentConsumer
dotnet run
```

4. Launch the Consumer

```bash
cd AgentConsumer
dotnet run
```

5. See the result

   1 FunctionResultContent in non streaming mode, 0 in streaming mode.

You can add breakpoints to have a better view on the Messages[0].Contents
