﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Services.Status.Table;
using StatusAggregator.Factory;

namespace StatusAggregator.Messages
{
    public class MessageChangeEventIterator : IMessageChangeEventIterator
    {
        private readonly IMessageChangeEventProcessor _processor;

        private readonly ILogger<MessageChangeEventIterator> _logger;

        public MessageChangeEventIterator(
            IMessageChangeEventProcessor processor,
            StatusAggregatorConfiguration configuration,
            ILogger<MessageChangeEventIterator> logger)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Iterate(IEnumerable<MessageChangeEvent> changes, EventEntity eventEntity)
        {
            var rootComponent = NuGetServiceComponentFactory.CreateNuGetServiceRootComponent();
            CurrentMessageContext context = null;
            foreach (var change in changes.OrderBy(c => c.Timestamp))
            {
                context = await _processor.Process(change, eventEntity, rootComponent, context);
            }
        }
    }
}
