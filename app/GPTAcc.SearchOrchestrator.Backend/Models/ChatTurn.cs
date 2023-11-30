// Copyright (c) Microsoft. All rights reserved.

namespace GPTAcc.SearchOrchestrator.Backend.Models;

public record ChatTurn(string User, string? Bot = null);
