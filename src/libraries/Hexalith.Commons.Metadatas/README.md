# Hexalith Commons Metadatas

This project defines common metadata structures used within the Hexalith framework, primarily for messages and their context.

## Classes

### `ContextMetadata`
Represents contextual information associated with a message, such as:
- Correlation ID (`CorrelationId`)
- User ID (`UserId`)
- Partition ID (`PartitionId`)
- Received Date (`ReceivedDate`)
- Sequence Number (`SequenceNumber`)
- Session ID (`SessionId`)
- Scopes (`Scopes`)

### `DomainMetadata`
Represents metadata specific to a domain aggregate:
- Aggregate ID (`Id`)
- Aggregate Name (`Name`)

### `MessageMetadata`
Represents metadata specific to the message itself:
- Message ID (`Id`)
- Message Name (`Name`)
- Message Version (`Version`)
- Associated Domain Metadata (`Domain`)
- Creation Date (`CreatedDate`)

### `Metadata`
A composite record that combines both `MessageMetadata` and `ContextMetadata` to provide a complete metadata picture for a message. It also includes helper methods like:
- `DomainGlobalId`: Generates a globally unique identifier for the domain aggregate based on partition, name, and ID.
- `CreateDomainGlobalId()`: Static and instance methods to construct the domain global ID.
- `ToLogString()`: Provides a concise string representation suitable for logging.

## Usage

These metadata classes are typically used together within message envelopes or event data structures to provide essential tracking, routing, and contextual information as messages flow through the system.