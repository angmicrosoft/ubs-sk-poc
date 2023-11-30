

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
            await searchQueryPlugin["SearchQuery"].InvokeAsync(context);
            await searchPlugin["Search"].InvokeAsync(context);
            await conversationPlugin["Chat"].InvokeAsync(context);
            await followupPlugin["FollowUpQuestion"].InvokeAsync(context);
            
            // Get the final answer
            var finalAnswer = context.Result;

            // var stepwiseConfig = new StepwisePlannerConfig()
            // {
            //     MaxIterations = 10,
            // };
            // var planner =  new StepwisePlanner(_kernel, stepwiseConfig);
            // var plan = planner.CreatePlan(question);
            // var result = await _kernel.RunAsync(plan);


            // var stepsTaken = result?.FunctionResults?.First()?.Metadata["stepsTaken"]?.ToString();
            // if (stepsTaken != null)
            // {
            //     var deserializedStepsTaken = JsonConvert.DeserializeObject<Step[]>(stepsTaken);
            //     // Use the deserializedStepsTaken object as needed
            //     Console.WriteLine(deserializedStepsTaken);
                
            //     if(deserializedStepsTaken == null)
            //     {
            //         return Results.NoContent();
            //     }
            //     var approachResponse = new ApproachResponse(deserializedStepsTaken[stepsTaken.Length - 1]?.FinalAnswer, 
            //     deserializedStepsTaken[stepsTaken.Length - 1]?.ActionVariables["dataPoints"],
            //     new []{ new SupportingContentRecord(deserializedStepsTaken[stepsTaken.Length - 1]?.ActionVariables["documentTitle"], deserializedStepsTaken[stepsTaken.Length - 1]?.ActionVariables["documentContent"]) },
            //      deserializedStepsTaken[stepsTaken.Length - 1]?.Thought,
            //     deserializedStepsTaken[stepsTaken.Length - 1]?.ActionVariables["citationBaseUrl"]);
            //     return Results.Ok(approachResponse);
            // }        
            
            return Results.BadRequest();
        
        }
    }
}