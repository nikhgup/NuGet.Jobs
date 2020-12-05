// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Search.Models;
using NuGet.Services.AzureSearch.Wrappers;

namespace NuGet.Services.AzureSearch.SearchService
{
    public class NewDocumentsOperationsWrapper : IDocumentsOperationsWrapper
    {
        private readonly SearchClient _searchClient;

        public NewDocumentsOperationsWrapper(SearchClient searchClient)
        {
            _searchClient = searchClient ?? throw new ArgumentNullException(nameof(searchClient));
        }

        public async Task<long> CountAsync()
        {
            return await _searchClient.GetDocumentCountAsync();
        }

        public async Task<T> GetOrNullAsync<T>(string key) where T : class
        {
            return await _searchClient.GetDocumentAsync<T>(key);
        }

        public Task<DocumentIndexResult> IndexAsync<T>(IndexBatch<T> batch) where T : class
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentSearchResult<T>> SearchAsync<T>(string searchText, SearchParameters searchParameters) where T : class
        {
            var options = new SearchOptions
            {
                Filter = searchParameters.Filter,
                IncludeTotalCount = searchParameters.IncludeTotalResultCount,
                QueryType = (SearchQueryType)searchParameters.QueryType,
                Skip = searchParameters.Skip,
                Size = searchParameters.Top,
            };

            if (searchParameters.OrderBy != null)
            {
                foreach (var orderBy in searchParameters.OrderBy)
                {
                    options.OrderBy.Add(orderBy);
                }
            }

            if (searchParameters.Select != null)
            {
                foreach (var select in searchParameters.Select)
                {
                    options.Select.Add(select);
                }
            }

            var output = new DocumentSearchResult<T>
            {
                Results = new List<Microsoft.Azure.Search.Models.SearchResult<T>>(),
            };

            SearchResults<T> results = await _searchClient.SearchAsync<T>(searchText, options);
            output.Count = results.TotalCount;

            var enumerator = results.GetResultsAsync().AsPages().GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    foreach (var result in enumerator.Current.Values)
                    {
                        output.Results.Add(new Microsoft.Azure.Search.Models.SearchResult<T>
                        {
                            Document = result.Document,
                            Score = result.Score.GetValueOrDefault(),
                        });
                    }

                    // Only read the first page. We use paging parameters through the application to avoid paging.
                    break;
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }

            return output;
        }
    }
}
