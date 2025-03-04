﻿#if NET472_OR_GREATER || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
#endif
using mea = Microsoft.Extensions.AI;
using System.Runtime.CompilerServices;

namespace Mscc.GenerativeAI.Microsoft;

public sealed class GeminiChatClient : mea.IChatClient
{
    private const string ProviderName = "gemini";
    
    /// <summary>
    /// Gets the Gemini model that is used to communicate with.
    /// </summary>
    private readonly GenerativeModel _client;
    
    /// <inheritdoc/>
    public mea.ChatClientMetadata Metadata { get; }

    /// <summary>
    /// Creates an instance of the Gemini API client using Google AI.
    /// </summary>
    /// <param name="apiKey">API key provided by Google AI Studio</param>
    /// <param name="model">Model to use.</param>
    public GeminiChatClient(string apiKey, string model = Model.Gemini15Pro)
    {
        var genAi = new GoogleAI(apiKey);
        _client = genAi.GenerativeModel(model);
        Metadata = new(ProviderName, null, model);
    }

    /// <summary>
    /// Creates an instance of the Gemini API client using Vertex AI.
    /// </summary>
    /// <param name="projectId">Identifier of the Google Cloud project.</param>
    /// <param name="region">Optional. Region to use (default: "us-central1").</param>
    /// <param name="model">Model to use.</param>
    public GeminiChatClient(string projectId, string? region = null, string model = Model.Gemini15Pro)
    {
        var genAi = new VertexAI(projectId, region);
        _client = genAi.GenerativeModel(model);
        Metadata = new(ProviderName, null, model);
    }
    
    /// <inheritdoc/>
    public async Task<mea.ChatCompletion> CompleteAsync(
        IList<mea.ChatMessage> chatMessages, 
        mea.ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (chatMessages == null) throw new ArgumentNullException(nameof(chatMessages));

        var request = MicrosoftAi.AbstractionMapper.ToGeminiGenerateContentRequest(chatMessages, options);
        var requestOptions = MicrosoftAi.AbstractionMapper.ToGeminiGenerateContentRequestOptions(options);
		var response = await _client.GenerateContent(request, requestOptions);
		return MicrosoftAi.AbstractionMapper.ToChatCompletion(response) ?? new mea.ChatCompletion([]);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<mea.StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<mea.ChatMessage> chatMessages, 
        mea.ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (chatMessages == null) throw new ArgumentNullException(nameof(chatMessages));

        var request = MicrosoftAi.AbstractionMapper.ToGeminiGenerateContentRequest(chatMessages, options);
        var requestOptions = MicrosoftAi.AbstractionMapper.ToGeminiGenerateContentRequestOptions(options);
		await foreach (var response in _client.GenerateContentStream(request, requestOptions, cancellationToken))
			yield return MicrosoftAi.AbstractionMapper.ToStreamingChatCompletionUpdate(response);
    }

    /// <inheritdoc/>
    public object? GetService(Type serviceType, object? key)
        => key is null && serviceType?.IsInstanceOfType(this) is true ? this : null;
        
    /// <inheritdoc/>
    public void Dispose() { }
}