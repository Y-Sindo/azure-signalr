// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.SignalR.Tests.Common;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Azure.SignalR.Management.Tests
{
    public class TransientTest
    {
        [Fact]
        public async Task Test()
        {
            var hubName = "HubName";
            var endpoints = FakeEndpointUtils.GetFakeEndpoint(2).ToArray();
            var services = new ServiceCollection()
                .AddSignalRServiceManager()
                .Configure<ServiceManagerOptions>(o =>
                {
                    o.ConnectionString = endpoints[0].ConnectionString; //currently transient mode requires connection string to be not null
                    o.ServiceEndpoints = endpoints;
                });

            var serviceManager = services.AddSingleton(services.ToList() as IReadOnlyCollection<ServiceDescriptor>)
                .BuildServiceProvider()
                .GetRequiredService<IServiceManager>();
            var serviceHubContext = await serviceManager.CreateHubContextAsync(hubName);
            Assert.Equal(2, endpoints.Length);
            for(int i = 0; i < 5; i++)
            {
                var negotiateResponse = await serviceHubContext.NegotiateAsync();
                negotiateResponse.Url = ClientEndpointUtils.GetExpectedClientEndpoint(hubName, null, endpoints[1].Endpoint);
            }
            
        }
    }
}
