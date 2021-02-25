// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Azure.SignalR.Management
{
    internal class MoqHealthChecker
    {
        private readonly IServiceEndpointManager _endpointManager;
        private readonly string _hubName;

        public MoqHealthChecker(IServiceEndpointManager endpointManager, string hubName)
        {
            _endpointManager = endpointManager;
            _hubName = hubName;
            var hubEndpoints = _endpointManager.GetEndpoints(hubName);
            foreach (var e in hubEndpoints)
            {
                e.Online = false;
            }
        }

        public async Task FirstCheck()
        {
            await CheckStatusAsync();
            _ = Loop();
        }

        private async Task Loop()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromMinutes(2));
                await CheckStatusAsync();
            }
        }

        private async Task CheckStatusAsync()
        {
            var hubEndpoints = _endpointManager.GetEndpoints(_hubName).ToArray();
            await Task.Delay(2000);
            foreach (var e in hubEndpoints)
            {
                e.Online = true;
            }
            //for test
            if (hubEndpoints.Length >= 2)
            {
                hubEndpoints[0].Online = false;
            }
        }
    }
}