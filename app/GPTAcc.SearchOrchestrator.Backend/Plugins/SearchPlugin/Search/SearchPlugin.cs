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
            var query = context.Variables["question"];
            var embeddingsString = context.Variables["embeddings"];
            string[] embeddingsStringArray = embeddingsString.Split(',');
            float[] embedding = Array.ConvertAll(embeddingsStringArray, float.Parse);

            var overrides = new RequestOverrides
            {
                Top = 3,
                RetrievalMode = "Semantic"
            };

            var documentContents = await _searchClient.QueryDocumentsAsync(query, embedding, overrides, cancellationToken);
            
            
            var document = documentContents.FirstOrDefault();
            context.Variables.Add("documentTitle", document?.Title);
            context.Variables.Add("documentContent", document?.Content);
            return context;
        }

    }
}