using System.Collections.Generic;
using System;
using Nest;
using Elasticsearch.Net;
using TorSearch.Backend.Data;

namespace TorSearch.Backend.Services
{
    public class ElasticSearchService
    {
        private readonly ElasticClient client;

        public ElasticSearchService(Uri clusterUri, string defaultIndex)
        {
            var pool = new SingleNodeConnectionPool(clusterUri);
            var settings = new ConnectionSettings(pool)
                .DefaultIndex(defaultIndex)
                .RequestTimeout(TimeSpan.FromSeconds(60));

            client = new ElasticClient(settings);
        }

        public bool SaveDomainInfo(DomainInfo info)
        {
            var response = client.IndexDocument(info);

            return response.Result == Result.Created;
        }

        public List<DomainInfo> Search(string query, int page)
        {
            var queries = new QueryContainer[] {
                new QueryContainerDescriptor<DomainInfo>().QueryString(m => m
            .Fields(f => f.Field(f1 => f1.Title))
            .Query(query)
            .Boost(8)),
                new QueryContainerDescriptor<DomainInfo>().QueryString(m => m
            .Fields(f => f.Field(f1 => f1.Description))
            .Query(query)
            .Boost(4)),
                new QueryContainerDescriptor<DomainInfo>().QueryString(m => m
            .Fields(f => f.Field(f1 => f1.Keywords))
            .Query(query)
            .Boost(2)),
                new QueryContainerDescriptor<DomainInfo>().QueryString(m => m
            .Fields(f => f.Field(f1 => f1.Html))
            .Query(query)
            .Boost(1))
            };

            var searchResponse = client.Search<DomainInfo>(s => s
            .Query(q => q
            .Bool(b => b
            .Should(queries)
            ))
            .Collapse(c => c
            .Field("baseurl.keyword")
            .InnerHits(i => i
            .Name("best")
            .Size(3)
            ))
            .MaxConcurrentShardRequests(4)
            );

            return searchResponse.Documents.ToList();
        }
    }
}
