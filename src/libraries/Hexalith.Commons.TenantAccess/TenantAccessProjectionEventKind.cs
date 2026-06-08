// <copyright file="TenantAccessProjectionEventKind.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Normalized tenant event kinds understood by the generic tenant-access projection handler.
/// </summary>
public enum TenantAccessProjectionEventKind
{
    /// <summary>The tenant was created.</summary>
    TenantCreated,

    /// <summary>The tenant was updated.</summary>
    TenantUpdated,

    /// <summary>The tenant was enabled.</summary>
    TenantEnabled,

    /// <summary>The tenant was disabled.</summary>
    TenantDisabled,

    /// <summary>A user was added to a tenant.</summary>
    UserAddedToTenant,

    /// <summary>A user was removed from a tenant.</summary>
    UserRemovedFromTenant,

    /// <summary>A user's tenant role changed.</summary>
    UserRoleChanged,

    /// <summary>A tenant configuration value was set.</summary>
    TenantConfigurationSet,

    /// <summary>A tenant configuration value was removed.</summary>
    TenantConfigurationRemoved,
}
