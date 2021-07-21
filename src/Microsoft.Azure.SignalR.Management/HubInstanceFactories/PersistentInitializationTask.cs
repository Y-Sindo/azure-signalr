// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Management
{
    internal class PersistentInitializationTask : IInitializationTask
    {
        private readonly IServiceConnectionContainer _connectionContainer;

        public PersistentInitializationTask(IServiceConnectionContainer connectionContainer)
        {
            _connectionContainer = connectionContainer;
        }

        public async Task StartAsync(CancellationToken token)
        {
            _ = _connectionContainer.StartAsync();
            await _connectionContainer.ConnectionInitializedTask.OrTimeout(token, TimeSpan.FromMinutes(1), "establishing service connections");
        }
    }
}
