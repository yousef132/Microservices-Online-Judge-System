using CodeSphere.Domain.Abstractions.Repositories;
using CoreJudge.Application.Abstractions.Repositories;
using CoreJudge.Infrastructure.Helpers;
using Microsoft.Extensions.Options;
using Nest;

namespace CoreJudge.Infrastructure.Implementation.Repositories
{
    public class ElasticSearchRepository : IElasticSearchRepository
    {
        private readonly ElasticClient _elasticClient;
        private readonly ElasticSetting elasticSetting;
        private readonly HttpClient httpClient;

        public ElasticSearchRepository(IOptions<ElasticSetting> options, HttpClient httpClient)
        {
            this.elasticSetting = options.Value;

            var settings = new ConnectionSettings(new Uri(elasticSetting.Url))
                .DefaultIndex(elasticSetting.DefaultIndex);


            this._elasticClient = new ElasticClient(settings);
            this.httpClient = httpClient;
        }

        public async Task<bool> IndexDocumentAsync<T>(T document, string indexName) where T : class
        {
            var response = await _elasticClient.IndexAsync(document, i => i.Index(indexName));
            return response.IsValid;
        }

        public async Task<bool> BulkIndexDocumentsAsync<T>(IEnumerable<T> documents, string indexName) where T : class
        {
            var response = await _elasticClient.BulkAsync(b => b
                .Index(indexName)
                .IndexMany(documents));
            return response.IsValid;
        }

        public Task InitializeIndexes()
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateDocumentAsync<T>(T document, string documentId, string indexName) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDocumentAsync(string documentId, string indexName)
        {
            throw new NotImplementedException();
        }

        public Task<T?> GetDocumentByIdAsync<T>(string documentId, string indexName) where T : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> IndexExistsAsync(string indexName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CreateIndexIfNotExistsAsync(string indexName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteIndexAsync(string indexName)
        {
            throw new NotImplementedException();
        }
    }

}
