// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using NuGet.Jobs;
using NuGet.Services.Status.Table;
using NuGet.Services.Status.Table.Manual;

namespace StatusAggregator.Manual
{
    public class DeleteStatusEventManualChangeHandler : IManualStatusChangeHandler<DeleteStatusEventManualChangeEntity>
    {
        private readonly ITableWrapper _table;

        public DeleteStatusEventManualChangeHandler(
            ITableWrapper table)
        {
            _table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public Task Handle(DeleteStatusEventManualChangeEntity entity)
        {
            var eventRowKey = EventEntity.GetRowKey(entity.EventAffectedComponentPath, entity.EventStartTime);
            return _table.DeleteAsync(EventEntity.DefaultPartitionKey, eventRowKey);
        }
    }
}
