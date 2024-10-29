#if NET472_OR_GREATER || NETSTANDARD2_0
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
#endif
using System.Text;

namespace Mscc.GenerativeAI
{
    /// <summary>
    /// Content that has been preprocessed and can be used in subsequent request to GenerativeService.
    /// Cached content can be only used with model it was created for.
    /// </summary>
    public class CachedContentModel : BaseGeneration
    {
        protected override string Version => ApiVersion.V1Beta;
        private string Url => "{EndpointGoogleAI}/{Version}/cachedContents";
        
        /// <summary>
        /// 
        /// </summary>
        public CachedContentModel()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cachedContentName"></param>
        public CachedContentModel(string cachedContentName) : this()
        {
            cachedContentName = cachedContentName.SanitizeCachedContentName();
        }

        /// <summary>
        /// Creates CachedContent resource.
        /// </summary>
        /// <param name="request">The cached content resource to create.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The cached content resource created</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="request"/> is <see langword="null"/>.</exception>
        public async Task<CachedContent> Create(CachedContent request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var url = ParseUrl(Url);
            string json = Serialize(request);
            var payload = new StringContent(json, Encoding.UTF8, Constants.MediaType);
            var response = await Client.PostAsync(url, payload, cancellationToken);
            await response.EnsureSuccessAsync();
            return await Deserialize<CachedContent>(response);
        }

        /// <summary>
        /// Creates CachedContent resource.
        /// </summary>
        /// <remarks>The minimum input token count for context caching is 32,768, and the maximum is the same as the maximum for the given model.</remarks>
        /// <param name="model"></param>
        /// <param name="displayName"></param>
        /// <param name="systemInstruction"></param>
        /// <param name="contents"></param>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public async Task<CachedContent> Create(string model,
            string? displayName = null, 
            Content? systemInstruction = null,
            List<Content>? contents = null,
            TimeSpan? ttl = null)
        {
            var request = new CachedContent()
            {
                Model = model,
                DisplayName = displayName,
                SystemInstruction = systemInstruction,
                Contents = contents,
                Ttl = ttl ?? TimeSpan.FromMinutes(5)
            };
            return await Create(request);
        }

        /// <summary>
        /// Lists CachedContents resources.
        /// </summary>
        /// <param name="pageSize">Optional. The maximum number of cached contents to return. The service may return fewer than this value. If unspecified, some default (under maximum) number of items will be returned. The maximum value is 1000; values above 1000 will be coerced to 1000.</param>
        /// <param name="pageToken">Optional. A page token, received from a previous `ListCachedContents` call. Provide this to retrieve the subsequent page. When paginating, all other parameters provided to `ListCachedContents` must match the call that provided the page token.</param>
        /// <returns></returns>
        public async Task<List<CachedContent>> List(int? pageSize = 50, 
            string? pageToken = null)
        {
            var queryStringParams = new Dictionary<string, string?>()
            {
                [nameof(pageSize)] = Convert.ToString(pageSize), 
                [nameof(pageToken)] = pageToken
            };

            var url = ParseUrl(Url).AddQueryString(queryStringParams);
            var response = await Client.GetAsync(url);
            await response.EnsureSuccessAsync();
            var cachedContents = await Deserialize<ListCachedContentsResponse>(response);
            return cachedContents?.CachedContents!;
        }
        
        /// <summary>
        /// Reads CachedContent resource.
        /// </summary>
        /// <param name="cachedContentName">Required. The resource name referring to the content cache entry. Format: `cachedContents/{id}`</param>
        /// <returns></returns>
        public async Task<CachedContent> Get(string cachedContentName)
        {
            cachedContentName = cachedContentName.SanitizeCachedContentName();
            if (!string.IsNullOrEmpty(_apiKey) && cachedContentName.StartsWith("cachedContents", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new NotSupportedException("Accessing tuned models via API key is not provided. Setup OAuth for your project.");
            }

            var url = $"{Url}/{cachedContentName}";
            url = ParseUrl(url);
            var response = await Client.GetAsync(url);
            await response.EnsureSuccessAsync();
            return await Deserialize<CachedContent>(response);
        }

        /// <summary>
        /// Updates CachedContent resource (only expiration is updatable).
        /// </summary>
        /// <param name="ttl"></param>
        /// <returns></returns>
        public async Task<CachedContent> Update(CachedContent request, TimeSpan ttl, string? updateMask = null)
        {
            var url = $"{Url}/{request.Name}";
            var queryStringParams = new Dictionary<string, string?>()
            {
                [nameof(updateMask)] = updateMask
            };
            
            url = ParseUrl(url).AddQueryString(queryStringParams);
            string json = Serialize(request);
            var payload = new StringContent(json, Encoding.UTF8, Constants.MediaType);
#if NET472_OR_GREATER || NETSTANDARD2_0
            var message = new HttpRequestMessage
            {
                Method = new HttpMethod("PATCH"),
                Content = payload,
                RequestUri = new Uri(url),
                Version = _httpVersion
            };
            var response = await Client.SendAsync(message);
#else
            var response = await Client.PatchAsync(url, payload);
#endif
            await response.EnsureSuccessAsync();
            return await Deserialize<CachedContent>(response);
        }

        /// <summary>
        /// Deletes CachedContent resource.
        /// </summary>
        /// <param name="cachedContentName"></param>
        public async Task<string> Delete(string cachedContentName)
        {
            if (cachedContentName == null) throw new ArgumentNullException(nameof(cachedContentName));

            cachedContentName = cachedContentName.SanitizeCachedContentName();
            var url = $"{Url}/{cachedContentName}";
            url = ParseUrl(url);
            var response = await Client.DeleteAsync(url);
            await response.EnsureSuccessAsync();
            return await response.Content.ReadAsStringAsync();
        }
    }
}