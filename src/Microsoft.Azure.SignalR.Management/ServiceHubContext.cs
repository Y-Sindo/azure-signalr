﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Azure.SignalR.Management
{
    internal class ServiceHubContext : IInternalServiceHubContext
    {
        private readonly string _hubName;
        private readonly IHubContext<Hub> _hubContext;
        private readonly IServiceHubLifetimeManager _lifetimeManager;
        private readonly NegotiateProcessor _negotiateProcessor;
        private readonly IServiceEndpointManager _endpointManager;
        private readonly ServiceTransportType _transportType;

        internal IServiceProvider ServiceProvider { get; }

        public IHubClients Clients => _hubContext.Clients;

        public IGroupManager Groups => _hubContext.Groups;

        public IUserGroupManager UserGroups { get; }

        public ServiceHubContext(string hubName, IHubContext<Hub> hubContext, IServiceHubLifetimeManager lifetimeManager, IServiceProvider serviceProvider, NegotiateProcessor negotiateProcessor, IServiceEndpointManager endpointManager, IOptions<ServiceManagerOptions> options)
        {
            _hubName = hubName;
            _hubContext = hubContext;
            _lifetimeManager = lifetimeManager;
            UserGroups = new UserGroupsManager(lifetimeManager);
            ServiceProvider = serviceProvider;
            _negotiateProcessor = negotiateProcessor;
            _endpointManager = endpointManager;
            _transportType = options.Value.ServiceTransportType;
        }

        public Task<NegotiationResponse> NegotiateAsync(NegotiationOptions options, CancellationToken cancellationToken)
        {
            return _negotiateProcessor.NegotiateAsync(_hubName, options?.HttpContext, options?.UserId, options?.Claims, options?.Lifetime, options != null && options.IsDiagnosticClient, cancellationToken);
        }

        IEnumerable<ServiceEndpoint> IInternalServiceHubContext.GetServiceEndpoints() => _endpointManager.GetEndpoints(_hubName);

        public async Task DisposeAsync()
        {
            await _lifetimeManager.DisposeAsync();
            (ServiceProvider as IDisposable)?.Dispose();
        }

        IInternalServiceHubContext IInternalServiceHubContext.WithEndpoints(IEnumerable<ServiceEndpoint> endpoints)
        {
            if (endpoints is null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

            var targetEndpoints = _endpointManager.GetEndpoints(_hubName).Intersect(endpoints, EqualityComparer<ServiceEndpoint>.Default).Select(e => e as HubServiceEndpoint).ToList();
            var container = new MultiEndpointMessageWriter(targetEndpoints, ServiceProvider.GetRequiredService<ILoggerFactory>());
            var servicesFromServiceManager = ServiceProvider.GetRequiredService<IReadOnlyCollection<ServiceDescriptor>>();
            var services = new ServiceCollection()
                .Add(servicesFromServiceManager)
                //Allow chained call serviceHubContext.WithEndpoints(...).WithEndpoints(...)
                .AddSingleton(servicesFromServiceManager)
                //add factory method
                .AddHub(_hubName, _transportType)
                //overwrite container
                .AddSingleton<IServiceConnectionContainer>(container)
                //add required service instances
                .AddSingleton(ServiceProvider.GetRequiredService<IOptions<ServiceManagerOptions>>())
                .AddSingleton(_negotiateProcessor)
                .AddSingleton(_endpointManager);

            return services.BuildServiceProvider().GetRequiredService<ServiceHubContext>();
        }
    }
}