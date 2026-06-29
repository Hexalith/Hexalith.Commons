// <copyright file="BoundedProblemDetailsReader.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http;

using System.Text.Json;

/// <summary>
/// Reads bounded ProblemDetails-compatible fields from HTTP responses.
/// </summary>
public static class BoundedProblemDetailsReader
{
    /// <summary>
    /// Reads a bounded ProblemDetails-compatible payload from an HTTP response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parsed bounded fields, or HTTP status-only defaults if parsing is not possible.</returns>
    public static async Task<BoundedProblemDetails> ReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(response);

        int status = (int)response.StatusCode;
        string? title = null;
        string? type = null;
        string? detail = null;
        string? correlationId = null;

        string contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
        if (contentType.Contains("problem+json", StringComparison.OrdinalIgnoreCase)
            || contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                using JsonDocument document = await JsonDocument.ParseAsync(
                    await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false),
                    cancellationToken: cancellationToken).ConfigureAwait(false);

                JsonElement root = document.RootElement;
                if (root.TryGetProperty("status", out JsonElement statusElement)
                    && statusElement.TryGetInt32(out int problemStatus))
                {
                    status = problemStatus;
                }

                title = TryGetString(root, "title");
                type = TryGetString(root, "type");
                detail = TryGetString(root, "detail");
                correlationId = TryGetString(root, "correlationId");
            }
            catch (JsonException)
            {
                // Use HTTP status fields when the body is not parseable JSON.
            }
        }

        return new BoundedProblemDetails(
            status,
            title ?? response.ReasonPhrase ?? "Error",
            type,
            detail,
            correlationId);
    }

    private static string? TryGetString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out JsonElement element)
            && element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;
}
