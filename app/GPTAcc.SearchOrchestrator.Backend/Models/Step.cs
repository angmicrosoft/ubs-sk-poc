namespace GPTAcc.SearchOrchestrator.Backend.Models;

public class Step
{

    [JsonPropertyName("action")]
    public string? Action {get; set;}

    [JsonPropertyName("action_variables")]
    public Dictionary<string, dynamic>? ActionVariables {get; set;}

    [JsonPropertyName("final_answer")]
    public string? FinalAnswer {get; set;}

    [JsonPropertyName("observation")]
    public string? Observation {get; set;}

    [JsonPropertyName("original_response")]
    public string? OriginalResponse {get; set;}

    [JsonPropertyName("thought")]
    public string? Thought {get; set;}


}