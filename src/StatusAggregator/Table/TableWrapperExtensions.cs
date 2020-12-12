// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using NuGet.Jobs;
using NuGet.Services.Status.Table;

namespace StatusAggregator.Table
{
    public static class TableWrapperExtensions
    {
        public static IQueryable<TEntity> GetActiveEntities<TEntity>(this ITableWrapper table)
            where TEntity : ComponentAffectingEntity, new()
        {
            return table
                .CreateQuery<TEntity>()
                .Where(e => e.IsActive);
        }

        public static IQueryable<TChild> GetChildEntities<TChild, TParent>(this ITableWrapper table, TParent entity)
            where TChild : ITableEntity, IChildEntity<TParent>, new()
            where TParent : ITableEntity
        {
            return table
                .CreateQuery<TChild>()
                .Where(e => e.ParentRowKey == entity.RowKey);
        }

        public static async Task<T> RetrieveAsync<T>(this ITableWrapper table, string rowKey)
            where T : class, ITableEntity
        {
            return await table.RetrieveAsync<T>(TablePartitionKeys.Get<T>(), rowKey);
        }

        public static IQueryable<T> CreateQuery<T>(this ITableWrapper table)
            where T : ITableEntity, new()
        {
            return table.CreateQuery<T>(TablePartitionKeys.Get<T>());
        }

    }
}
