

using Azure.Core.Pipeline;
using GPTAcc.SearchOrchestrator.Backend.Plugins;
using Microsoft.SemanticKernel.Planners;
using Newtonsoft.Json;

namespace GPTAcc.SearchOrchestrator.Backend.Services
{
    internal class ChatService
    {
        private readonly IKernel _kernel;
        private readonly SearchClient _searchClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatService> _logger;

        public ChatService(IKernel kernel, SearchClient searchClient, IConfiguration configuration, ILogger<ChatService> logger)
        {
            _kernel = kernel;
            _searchClient = searchClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IResult> PostChatAsync(string question, CancellationToken cancellationToken = default)
        {
            
            var currentDirectory = Directory.GetCurrentDirectory();
            IChatCompletion gpt35Turbo = _kernel.GetService<IChatCompletion>()!;
            ITextEmbeddingGeneration? embedding = _kernel.GetService<ITextEmbeddingGeneration>();
            float[]? embeddings = null;

            // Get embeddings for the question
            embeddings = (await embedding.GenerateEmbeddingAsync(question, cancellationToken: cancellationToken)).ToArray();
            Console.WriteLine( $"Embeddings: {embeddings.Length}");

            // Create context
            var context = _kernel.CreateNewContext();
            context.Variables.Add("question", question);
            context.Variables.Add("embeddings", string.Join(",", embeddings));

            var pluginsDirectory = Path.Combine(currentDirectory, "Plugins");
            // Create intent plugin
            var intentPlugin = _kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "IntentClassifierPlugin");

            // Create Search Query Plugin
            var searchQueryPlugin = _kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "SearchQueryPlugin");

            // Create Search plugin
            ILogger<SearchPlugin> logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SearchPlugin>();            
           var searchPlugin = _kernel.ImportFunctions(new SearchPlugin(_searchClient, _configuration, logger));

            // Create Chat plugin
            var conversationPlugin = _kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "ConversationPlugin");

            // Create Followup plugin
            var followupPlugin = _kernel.ImportSemanticFunctionsFromDirectory(pluginsDirectory, "FollowUpQuestionPlugin");

            // Implement plugin calls
            var queryResult =  await searchQueryPlugin["SearchQuery"].InvokeAsync(context);

            ApproachResponse approachResponse;
            Console.WriteLine($"Query Result: {context.Result}");
            var query = context.Result;
            context.Variables.Add("query", query);
            var searchContentResult = await searchPlugin["Search"].InvokeAsync(context);
            var chatConversationResult = await conversationPlugin["Chat"].InvokeAsync(context);
            Console.WriteLine($"Chat Conversation: {context.Result}");
            var chatConversation = context.Result;
            var answerObject = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(chatConversation);
            var ans = answerObject.GetProperty("answer").GetString() ?? throw new InvalidOperationException("Failed to get answer");
            var thoughts = answerObject.GetProperty("thoughts").GetString() ?? throw new InvalidOperationException("Failed to get thoughts");
            context.Variables.Add("answer", ans);
            await followupPlugin["FollowUpQuestion"].InvokeAsync(context);
            
            // Get the final answer
            var finalAnswer = context.Result;
            SupportingContentRecord[] supportingContentRecords =new [] { 
                new SupportingContentRecord(context.Variables["documentTitle1"], context.Variables["documentContent1"]), 
                new SupportingContentRecord(context.Variables["documentTitle2"], context.Variables["documentContent2"]), 
                new SupportingContentRecord(context.Variables["documentTitle3"], context.Variables["documentContent3"])
                };
            approachResponse = new ApproachResponse(ans, thoughts, supportingContentRecords, "", "");

            return Results.Ok(approachResponse);
        }
    }
}