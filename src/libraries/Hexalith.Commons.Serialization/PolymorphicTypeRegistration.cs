// <copyright file="PolymorphicTypeRegistration.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Serialization;

/// <summary>
/// Defines one explicit discriminator-to-type registration for a polymorphic registry.
/// </summary>
/// <param name="Discriminator">The stable bounded discriminator or type name.</param>
/// <param name="Type">The runtime type represented by the discriminator.</param>
public sealed record PolymorphicTypeRegistration(string Discriminator, Type Type);
