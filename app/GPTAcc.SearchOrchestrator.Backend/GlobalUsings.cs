// Copyright (c) Microsoft. All rights reserved.

global using System.Diagnostics;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Text.Json;
global using Azure.AI.OpenAI;
global using Azure.Identity;
global using Azure.Search.Documents;
global using Azure.Search.Documents.Models;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Mvc.RazorPages;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.Orchestration;
global using Microsoft.SemanticKernel.AI.ChatCompletion;
global using Microsoft.SemanticKernel.AI.Embeddings;
global using GPTAcc.SearchOrchestrator.Backend.Extensions;
global using GPTAcc.SearchOrchestrator.Backend;
global using GPTAcc.SearchOrchestrator.Backend.Models;
global using GPTAcc.SearchOrchestrator.Backend.Services;
global using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
global using System.Collections.Generic;
global using System.Text.Json.Serialization;
