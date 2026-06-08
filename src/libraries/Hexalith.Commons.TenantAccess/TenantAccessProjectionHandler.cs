// <copyright file="TenantAccessProjectionHandler.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

using Microsoft.Extensions.Logging;

namespace Hexalith.Commons.TenantAccess;

/// <summary>
/// Applies module tenant events to a local tenant-access projection with replay and retry tolerance.
/// </summary>
/// <typeparam name="TEvent">The module event type.</typeparam>
/// <typeparam name="TProjection">The module projection type.</typeparam>
public sealed class TenantAccessProjectionHandler<TEvent, TProjection>(
    ITenantAccessProjectionStore<TProjection> store,
    ITenantAccessClock clock,
    TenantAccessProjectionHandlerOptions options,
    Func<TEvent, TenantAccessProjectionEvent> eventAdapter,
    ILogger<TenantAccessProjectionHandler<TEvent, TProjection>>? logger = null)
    where TProjection : TenantAccessProjectionState, new()
{
    private readonly Func<string, bool> _acceptsConfigurationKey =
        options.AcceptsConfigurationKey ?? (_ => true);

    private readonly Func<Exception, bool> _isRetryablePersistenceException =
        options.IsRetryablePersistenceException ?? (static ex => ex is TimeoutException);

    /// <summary>
    /// Applies one event using bounded optimistic-concurrency retries.
    /// </summary>
    /// <param name="event">The module event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(@event);

        TenantAccessProjectionEvent view = eventAdapter(@event);
        if (string.IsNullOrWhiteSpace(view.TenantId))
        {
            return;
        }

        int attempts = Math.Max(1, options.ConcurrencyRetryAttempts);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            try
            {
                await ApplyOnceAsync(view, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (_isRetryablePersistenceException(ex) && attempt + 1 < attempts)
            {
                logger?.LogDebug(
                    "Retryable persistence failure applying tenant event {EventKind} for tenant {TenantId}; retry {Attempt} of {Attempts}.",
                    view.Kind,
                    view.TenantId,
                    attempt + 2,
                    attempts);
            }
        }
    }

    private async Task ApplyOnceAsync(TenantAccessProjectionEvent @event, CancellationToken cancellationToken)
    {
        TProjection projection = await store.GetAsync(@event.TenantId!, cancellationToken).ConfigureAwait(false)
            ?? new TProjection { TenantId = @event.TenantId! };

        if (IsMalformed(@event))
        {
            projection.MalformedEvidence = true;
            await store.SaveAsync(projection, cancellationToken).ConfigureAwait(false);
            return;
        }

        TenantAccessProjectionEvidence evidence = CreateEvidence(@event);
        if (projection.ProcessedMessages.TryGetValue(@event.MessageId!, out TenantAccessProjectionEvidence? existing))
        {
            if (existing != evidence)
            {
                projection.ReplayConflict = true;
                await store.SaveAsync(projection, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        if (@event.SequenceNumber <= projection.Watermark)
        {
            logger?.LogDebug(
                "Dropping out-of-order tenant event {EventKind} for tenant {TenantId}: sequence {Sequence} <= watermark {Watermark}.",
                @event.Kind,
                @event.TenantId,
                @event.SequenceNumber,
                projection.Watermark);
            return;
        }

        Apply(projection, @event);
        projection.ProcessedMessages[@event.MessageId!] = evidence;
        projection.Watermark = @event.SequenceNumber;
        projection.LastEventTimestamp = @event.Timestamp;
        projection.ProjectionWatermark = $"{@event.TenantId}:{@event.SequenceNumber}";

        await store.SaveAsync(projection, cancellationToken).ConfigureAwait(false);
    }

    private void Apply(TProjection projection, TenantAccessProjectionEvent @event)
    {
        switch (@event.Kind)
        {
            case TenantAccessProjectionEventKind.TenantCreated:
            case TenantAccessProjectionEventKind.TenantEnabled:
                projection.Enabled = true;
                break;
            case TenantAccessProjectionEventKind.TenantDisabled:
                projection.Enabled = false;
                break;
            case TenantAccessProjectionEventKind.UserAddedToTenant:
            case TenantAccessProjectionEventKind.UserRoleChanged:
                if (!string.IsNullOrWhiteSpace(@event.PrincipalId) && !string.IsNullOrWhiteSpace(@event.Role))
                {
                    projection.Principals[@event.PrincipalId] = new TenantAccessPrincipalEvidence(@event.PrincipalId, @event.Role);
                }

                break;
            case TenantAccessProjectionEventKind.UserRemovedFromTenant:
                if (!string.IsNullOrWhiteSpace(@event.PrincipalId))
                {
                    _ = projection.Principals.Remove(@event.PrincipalId);
                }

                break;
            case TenantAccessProjectionEventKind.TenantConfigurationSet:
                AddConfigurationKey(projection, @event.ConfigurationKey);
                break;
            case TenantAccessProjectionEventKind.TenantConfigurationRemoved:
                RemoveConfigurationKey(projection, @event.ConfigurationKey);
                break;
            case TenantAccessProjectionEventKind.TenantUpdated:
            default:
                break;
        }
    }

    private void AddConfigurationKey(TProjection projection, string? key)
    {
        if (key is null || !_acceptsConfigurationKey(key))
        {
            return;
        }

        _ = projection.ConfigurationKeys.Add(key);
        _ = projection.RemovedConfigurationKeys.Remove(key);
    }

    private void RemoveConfigurationKey(TProjection projection, string? key)
    {
        if (key is null || !_acceptsConfigurationKey(key))
        {
            return;
        }

        _ = projection.ConfigurationKeys.Remove(key);
        _ = projection.RemovedConfigurationKeys.Add(key);
    }

    private bool IsMalformed(TenantAccessProjectionEvent @event)
        => string.IsNullOrWhiteSpace(@event.MessageId)
            || @event.SequenceNumber <= 0
            || @event.Timestamp - clock.UtcNow > options.ClockSkewTolerance
            || ((@event.Kind is TenantAccessProjectionEventKind.UserAddedToTenant
                or TenantAccessProjectionEventKind.UserRemovedFromTenant
                or TenantAccessProjectionEventKind.UserRoleChanged)
                && string.IsNullOrWhiteSpace(@event.PrincipalId));

    private static TenantAccessProjectionEvidence CreateEvidence(TenantAccessProjectionEvent @event)
        => new(
            @event.MessageId!,
            @event.TenantId!,
            @event.Kind.ToString(),
            @event.SequenceNumber,
            @event.Timestamp,
            @event.PayloadFingerprint);
}
