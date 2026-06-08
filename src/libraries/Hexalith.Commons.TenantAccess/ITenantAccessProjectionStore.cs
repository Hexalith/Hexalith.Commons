// <copyright file="ITenantAccessProjectionStore.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Stores a module-owned tenant-access projection.
/// </summary>
/// <typeparam name="TProjection">The projection type.</typeparam>
public interface ITenantAccessProjectionStore<TProjection>
    where TProjection : TenantAccessProjectionState, new()
{
    /// <summary>
    /// Gets the projection for a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The projection, or <see langword="null"/> when absent.</returns>
    Task<TProjection?> GetAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a projection.
    /// </summary>
    /// <param name="projection">The projection to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(TProjection projection, CancellationToken cancellationToken = default);
}
