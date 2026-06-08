// <copyright file="TenantAccessRegistration.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Domain-agnostic registration helper for tenant-access boundaries.
/// </summary>
public static class TenantAccessRegistration
{
    /// <summary>
    /// Registers Tenants projection plumbing plus a module-owned tenant-access facade.
    /// </summary>
    /// <typeparam name="TSignal">The module signal contract.</typeparam>
    /// <typeparam name="TSignalImplementation">The module signal implementation.</typeparam>
    /// <typeparam name="TAccessService">The module access service contract.</typeparam>
    /// <typeparam name="TAccessServiceImplementation">The module access service implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="addTenantsProjection">Registers the upstream Tenants projection plumbing.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddTenantAccess<TSignal, TSignalImplementation, TAccessService, TAccessServiceImplementation>(
        this IServiceCollection services,
        Action<IServiceCollection> addTenantsProjection)
        where TSignal : class
        where TSignalImplementation : class, TSignal
        where TAccessService : class
        where TAccessServiceImplementation : class, TAccessService
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(addTenantsProjection);

        addTenantsProjection(services);
        services.TryAddScoped<TSignal, TSignalImplementation>();
        services.TryAddScoped<TAccessService, TAccessServiceImplementation>();
        return services;
    }
}
