using System.ComponentModel;

namespace GPTAcc.SearchOrchestrator.Backend.Plugins
{
    internal class SearchPlugin
    {
        private readonly SearchClient _searchClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SearchPlugin> _logger;

        public SearchPlugin(SearchClient searchClient, IConfiguration configuration, ILogger<SearchPlugin> logger)
        {
            _searchClient = searchClient;
            _configuration = configuration;
            _logger = logger;
        }

        [SKFunction, Description("Searches for documents using the query")]
        public async Task<SKContext> SearchAsync(
            SKContext context,
            CancellationToken cancellationToken = default)
        {
            var query = context.Variables["query"];
            var embeddingsString = context.Variables["embeddings"];
            string[] embeddingsStringArray = embeddingsString.Split(',');
            float[] embedding = Array.ConvertAll(embeddingsStringArray, float.Parse);

            var overrides = new RequestOverrides
            {
                Top = 3,
                RetrievalMode = "Semantic"
            };

            var documentContents = await _searchClient.QueryDocumentsAsync(query, embedding, overrides, cancellationToken);            
            int i =1;
            foreach (var document in documentContents)
            {                
                context.Variables.Add($"documentTitle{i}", document?.Title);
                context.Variables.Add($"documentContent{i}", document?.Content);
                i++;
            }
            
            return context;
        }

    }
}