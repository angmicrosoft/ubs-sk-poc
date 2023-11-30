// Copyright (c) Microsoft. All rights reserved.

namespace GPTAcc.SearchOrchestrator.Backend.Extensions;

internal static class WebApplicationExtensions
{
    internal static WebApplication MapApi(this WebApplication app)
    {
        var api = app.MapGroup("api");

        // Long-form chat w/ contextual history endpoint
        api.MapPost("chat", OnPostChatAsync);

        return app;
    }

    private static async Task<IResult> OnPostChatAsync(string question,
        ChatService chatService,
        CancellationToken cancellationToken = default)
    {
       
        var result = await chatService.PostChatAsync(question, cancellationToken);
        return Results.Ok(result);
        
    }
    
}
