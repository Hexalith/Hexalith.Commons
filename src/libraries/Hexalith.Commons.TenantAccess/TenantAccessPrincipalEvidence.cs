// <copyright file="TenantAccessPrincipalEvidence.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Metadata-only tenant principal evidence.
/// </summary>
/// <param name="PrincipalId">The principal identifier.</param>
/// <param name="Role">The role token.</param>
public sealed record TenantAccessPrincipalEvidence(string PrincipalId, string Role);
