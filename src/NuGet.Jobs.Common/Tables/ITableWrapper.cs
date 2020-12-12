// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace NuGet.Jobs
{
    public interface ITableWrapper
    {
        Task CreateIfNotExistsAsync();

        Task<T> RetrieveAsync<T>(string partitionKey, string rowKey)
            where T : class, ITableEntity;

        Task InsertAsync(ITableEntity tableEntity);

        Task InsertOrReplaceAsync(ITableEntity tableEntity);

        Task ReplaceAsync(ITableEntity tableEntity);

        Task DeleteAsync(string partitionKey, string rowKey);

        Task DeleteAsync(string partitionKey, string rowKey, string eTag);

        Task DeleteAsync(ITableEntity tableEntity);

        IQueryable<T> CreateQuery<T>(string partitionKey)
            where T : ITableEntity, new();
    }
}
