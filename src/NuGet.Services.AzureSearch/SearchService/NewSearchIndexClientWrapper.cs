// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Azure.Search.Documents;
using NuGet.Services.AzureSearch.Wrappers;

namespace NuGet.Services.AzureSearch.SearchService
{
    public class NewSearchIndexClientWrapper : ISearchIndexClientWrapper
    {
        private readonly string _indexName;
        private readonly NewDocumentsOperationsWrapper _operations;

        public NewSearchIndexClientWrapper(SearchClient searchClient)
        {
            _indexName = searchClient.IndexName;
            _operations = new NewDocumentsOperationsWrapper(searchClient);
        }

        public string IndexName => _indexName;
        public IDocumentsOperationsWrapper Documents => _operations;
    }
}
