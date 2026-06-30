// <copyright file="MetadataTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Tests.Metadatas;

using System;
using System.Text.Json;

using Hexalith.Commons.Metadatas;

using Shouldly;

/// <summary>
/// Unit tests for Metadata classes.
/// </summary>
public class MetadataTest
{
    /// <summary>
    /// Tests that ContextMetadata copy constructor throws on null context.
    /// </summary>
    [Fact]
    public void ContextMetadataCopyConstructorShouldThrowOnNullContext()
    {
        // Arrange
        ContextMetadata? nullContext = null;

        // Act & Assert
        _ = Should.Throw<ArgumentNullException>(() => new ContextMetadata(nullContext!, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Tests that ContextMetadata copy constructor works correctly.
    /// </summary>
    [Fact]
    public void ContextMetadataCopyConstructorShouldWork()
    {
        // Arrange
        ContextMetadata original = new(
            "correlation-123",
            "user-456",
            "partition-789",
            null)
        {
            TimeToLive = TimeSpan.FromMinutes(5),
            SequenceNumber = 100L,
            Etag = "etag-value",
            SessionId = "session-abc",
            Scopes = ["scope1"],
        };
        DateTimeOffset newReceivedDate = DateTimeOffset.UtcNow;

        // Act
        ContextMetadata copy = new(original, newReceivedDate);

        // Assert
        copy.CorrelationId.ShouldBe(original.CorrelationId);
        copy.UserId.ShouldBe(original.UserId);
        copy.ReceivedDate.ShouldBe(newReceivedDate);
    }

    /// <summary>
    /// Tests that ContextMetadata can be created with all properties.
    /// </summary>
    [Fact]
    public void ContextMetadataShouldStoreAllProperties()
    {
        // Arrange & Act
        ContextMetadata context = new(
            "correlation-123",
            "user-456",
            "partition-789",
            DateTimeOffset.UtcNow)
        {
            TimeToLive = TimeSpan.FromMinutes(5),
            SequenceNumber = 100L,
            Etag = "etag-value",
            SessionId = "session-abc",
            Scopes = ["scope1", "scope2"],
        };

        // Assert
        context.CorrelationId.ShouldBe("correlation-123");
        context.UserId.ShouldBe("user-456");
        context.PartitionId.ShouldBe("partition-789");
        context.SequenceNumber.ShouldBe(100L);
        context.Etag.ShouldBe("etag-value");
        context.SessionId.ShouldBe("session-abc");
        context.Scopes.ShouldBe(["scope1", "scope2"]);
    }

    /// <summary>
    /// Tests that Metadata.CreateDomainGlobalId static method works correctly.
    /// </summary>
    [Fact]
    public void CreateDomainGlobalIdStaticMethodShouldWork()
    {
        // Act
        string result = Metadata.CreateDomainGlobalId("part1", "Aggregate", "id123");

        // Assert
        result.ShouldBe("part1-Aggregate-id123");
    }

    /// <summary>
    /// Tests that DomainMetadata.Empty returns correct empty instance.
    /// </summary>
    [Fact]
    public void DomainMetadataEmptyShouldReturnEmptyInstance()
    {
        // Act
        DomainMetadata empty = DomainMetadata.Empty;

        // Assert
        empty.Name.ShouldBeEmpty();
        empty.Id.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that MessageMetadata.Empty returns correct empty instance.
    /// </summary>
    [Fact]
    public void MessageMetadataEmptyShouldReturnEmptyInstance()
    {
        // Act
        MessageMetadata empty = MessageMetadata.Empty;

        // Assert
        empty.Id.ShouldBeEmpty();
        empty.Name.ShouldBeEmpty();
        empty.Version.ShouldBe(0);
        empty.Domain.ShouldBe(DomainMetadata.Empty);
        empty.CreatedDate.ShouldBe(DateTimeOffset.MinValue);
    }

    /// <summary>
    /// Tests that Metadata can be serialized and deserialized.
    /// </summary>
    [Fact]
    public void MetadataShouldBeSerializable()
    {
        // Arrange - DomainMetadata(Id, Name)
        DomainMetadata domain = new("domain-id", "TestDomain");
        MessageMetadata message = new(
            "msg-id",
            "TestMsg",
            2,
            domain,
            new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        ContextMetadata context = new(
            "corr-id",
            "user-id",
            "part-id",
            new DateTimeOffset(2024, 1, 1, 12, 5, 0, TimeSpan.Zero))
        {
            TimeToLive = TimeSpan.FromMinutes(10),
            SequenceNumber = 42L,
            Etag = "etag",
            SessionId = "session",
            Scopes = ["scope1"],
        };
        Metadata original = new(message, context);

        // Act
        string json = JsonSerializer.Serialize(original);
        Metadata? deserialized = JsonSerializer.Deserialize<Metadata>(json);

        // Assert
        _ = deserialized.ShouldNotBeNull();
        deserialized.Message.Id.ShouldBe(original.Message.Id);
        deserialized.Message.Name.ShouldBe(original.Message.Name);
        deserialized.Context.CorrelationId.ShouldBe(original.Context.CorrelationId);
    }

    /// <summary>
    /// Tests that Metadata creates correct DomainGlobalId.
    /// </summary>
    [Fact]
    public void MetadataShouldCreateCorrectDomainGlobalId()
    {
        // Arrange - DomainMetadata(Id, Name)
        DomainMetadata domain = new("aggregate-123", "TestAggregate");
        MessageMetadata message = new(
            "msg-001",
            "TestMessage",
            1,
            domain,
            DateTimeOffset.UtcNow);
        ContextMetadata context = new(
            "corr-001",
            "user-001",
            "partition-001",
            null);
        Metadata metadata = new(message, context);

        // Act
        string globalId = metadata.DomainGlobalId;

        // Assert - Format: partitionId-aggregateName-aggregateId
        globalId.ShouldBe("partition-001-TestAggregate-aggregate-123");
    }

    /// <summary>
    /// Tests that ToLogString returns formatted log string.
    /// </summary>
    [Fact]
    public void ToLogStringShouldReturnFormattedString()
    {
        // Arrange - DomainMetadata(Id, Name)
        DomainMetadata domain = new("order-999", "Order");
        MessageMetadata message = new(
            "msg-123",
            "OrderCreated",
            1,
            domain,
            DateTimeOffset.UtcNow);
        ContextMetadata context = new(
            "corr-456",
            "admin-user",
            "main-partition",
            null);
        Metadata metadata = new(message, context);

        // Act
        string logString = metadata.ToLogString();

        // Assert
        logString.ShouldContain("MessageName=OrderCreated");
        logString.ShouldContain("MessageId=msg-123");
        logString.ShouldContain("CorrelationId=corr-456");
        logString.ShouldContain("UserId=admin-user");
    }
}
