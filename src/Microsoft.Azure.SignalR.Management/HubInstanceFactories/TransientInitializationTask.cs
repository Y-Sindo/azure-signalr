﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Management
{
    internal class TransientInitializationTask : IInitializationTask
    {
        public Task StartAsync(CancellationToken token)
        {
            // Will have health checker through rest api later.
            return Task.CompletedTask;
        }
    }
}
