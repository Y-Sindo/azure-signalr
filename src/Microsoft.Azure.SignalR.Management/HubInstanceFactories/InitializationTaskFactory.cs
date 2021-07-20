// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Azure.SignalR.Management
{
    internal class InitializationTaskFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ServiceManagerOptions _options;

        public InitializationTaskFactory(IServiceProvider serviceProvider, IOptions<ServiceManagerOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public IInitializationTask Create()
        {
            return _options.ServiceTransportType switch
            {
                ServiceTransportType.Persistent => _serviceProvider.GetRequiredService<PersistentInitializationTask>(),
                ServiceTransportType.Transient => _serviceProvider.GetRequiredService<TransientInitializationTask>(),
                _ => throw new NotSupportedException(),
            };
        }
    }
}
