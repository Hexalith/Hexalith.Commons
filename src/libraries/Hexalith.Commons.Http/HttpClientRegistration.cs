// <copyright file="HttpClientRegistration.cs" company="ITANEO">
// Copyright (c) ITANEO. All rights reserved.
// Licensed under the MIT License.
// </copyright>

namespace Hexalith.Commons.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

/// <summary>
/// Domain-agnostic registration helpers for typed <see cref="System.Net.Http.HttpClient"/> clients with
/// endpoint-options validation.
/// </summary>
/// <remarks>
/// <para>
/// This is the shared promotion of the duplicated <c>AddXxxClient()</c> registration pattern. A domain
/// module supplies only the client interface, the typed implementation, its options type, and an endpoint
/// selector; the helper owns the registration, the options-validation policy, and the
/// <see cref="IHttpClientBuilder"/> wiring so callers can chain message handlers (for example a bearer-token
/// <see cref="System.Net.Http.DelegatingHandler"/>).
/// </para>
/// <para>
/// The helper is a superset of every existing shape and never weakens a caller:
/// <list type="bullet">
/// <item><description>
/// Validation timing is selectable via <see cref="HttpClientEndpointValidation"/> — lazy
/// (<see cref="HttpClientEndpointValidation.OnResolve"/>, the Folders/Projects shape) or eager
/// (<see cref="HttpClientEndpointValidation.OnRegistration"/>, the Conversations shape).
/// </description></item>
/// <item><description>
/// The http/https scheme guard is a first-class, opt-in option (<c>requireWebScheme</c>) so a stricter
/// caller (Conversations) keeps its guard while a permissive caller is unaffected.
/// </description></item>
/// </list>
/// </para>
/// </remarks>
public static class HttpClientRegistration
{
    /// <summary>
    /// Registers a typed HttpClient whose options are configured by a delegate.
    /// </summary>
    /// <typeparam name="TClient">The typed client contract registered in DI.</typeparam>
    /// <typeparam name="TImplementation">The concrete typed-client implementation.</typeparam>
    /// <typeparam name="TOptions">The endpoint-carrying options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">A delegate that configures <typeparamref name="TOptions"/>.</param>
    /// <param name="endpointSelector">Selects the transport endpoint from <typeparamref name="TOptions"/>.</param>
    /// <param name="validation">When the endpoint is validated. Defaults to <see cref="HttpClientEndpointValidation.OnResolve"/>.</param>
    /// <param name="requireWebScheme">When <see langword="true"/>, the endpoint must use the http or https scheme.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> so callers can chain message handlers.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown at registration time when <paramref name="validation"/> is
    /// <see cref="HttpClientEndpointValidation.OnRegistration"/> and the configured endpoint is missing,
    /// relative, or (when <paramref name="requireWebScheme"/> is set) not http/https.
    /// </exception>
    public static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation, TOptions>(
        this IServiceCollection services,
        Action<TOptions> configureOptions,
        Func<TOptions, Uri?> endpointSelector,
        HttpClientEndpointValidation validation = HttpClientEndpointValidation.OnResolve,
        bool requireWebScheme = false)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : class, new()
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);
        ArgumentNullException.ThrowIfNull(endpointSelector);

        if (validation == HttpClientEndpointValidation.OnRegistration)
        {
            // Eager shape: validate now and throw immediately, preserving registration-time rejection.
            TOptions options = new();
            configureOptions(options);
            Uri endpoint = ValidateEndpointOrThrow(endpointSelector(options), requireWebScheme);

            return services.AddHttpClient<TClient, TImplementation>(client => client.BaseAddress = endpoint);
        }

        _ = services.Configure(configureOptions);
        return services.AddConfiguredTypedHttpClient<TClient, TImplementation, TOptions>(endpointSelector, requireWebScheme);
    }

    /// <summary>
    /// Registers a typed HttpClient whose options are bound from a configuration section. The endpoint is
    /// always validated lazily (<see cref="HttpClientEndpointValidation.OnResolve"/>) because the bound
    /// configuration is not available until the service provider is built.
    /// </summary>
    /// <typeparam name="TClient">The typed client contract registered in DI.</typeparam>
    /// <typeparam name="TImplementation">The concrete typed-client implementation.</typeparam>
    /// <typeparam name="TOptions">The endpoint-carrying options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationSectionName">The configuration section bound to <typeparamref name="TOptions"/>.</param>
    /// <param name="endpointSelector">Selects the transport endpoint from <typeparamref name="TOptions"/>.</param>
    /// <param name="requireWebScheme">When <see langword="true"/>, the endpoint must use the http or https scheme.</param>
    /// <returns>An <see cref="IHttpClientBuilder"/> so callers can chain message handlers.</returns>
    public static IHttpClientBuilder AddTypedHttpClient<TClient, TImplementation, TOptions>(
        this IServiceCollection services,
        string configurationSectionName,
        Func<TOptions, Uri?> endpointSelector,
        bool requireWebScheme = false)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : class
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationSectionName);
        ArgumentNullException.ThrowIfNull(endpointSelector);

        _ = services.AddOptions<TOptions>().BindConfiguration(configurationSectionName);
        return services.AddConfiguredTypedHttpClient<TClient, TImplementation, TOptions>(endpointSelector, requireWebScheme);
    }

    private static IHttpClientBuilder AddConfiguredTypedHttpClient<TClient, TImplementation, TOptions>(
        this IServiceCollection services,
        Func<TOptions, Uri?> endpointSelector,
        bool requireWebScheme)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : class
    {
        // Lazy shape: fail at first resolve when the transport endpoint is missing, relative, or
        // (when required) not an http/https URI.
        _ = services
            .AddOptions<TOptions>()
            .Validate(
                options => IsEndpointValid(endpointSelector(options), requireWebScheme),
                BuildValidationMessage(typeof(TOptions), requireWebScheme));

        return services.AddHttpClient<TClient, TImplementation>((serviceProvider, httpClient) =>
        {
            TOptions options = serviceProvider.GetRequiredService<IOptions<TOptions>>().Value;
            httpClient.BaseAddress = endpointSelector(options);
        });
    }

    private static Uri ValidateEndpointOrThrow(Uri? endpoint, bool requireWebScheme)
    {
        if (endpoint is null || !endpoint.IsAbsoluteUri)
        {
            throw new InvalidOperationException("The typed HttpClient endpoint must be configured as an absolute URI.");
        }

        return !requireWebScheme || IsWebScheme(endpoint)
            ? endpoint
            : throw new InvalidOperationException("The typed HttpClient endpoint must use the http or https scheme.");
    }

    private static bool IsEndpointValid(Uri? endpoint, bool requireWebScheme)
        => endpoint is { IsAbsoluteUri: true } && (!requireWebScheme || IsWebScheme(endpoint));

    private static bool IsWebScheme(Uri endpoint)
        => endpoint.Scheme is "http" or "https";

    private static string BuildValidationMessage(Type optionsType, bool requireWebScheme)
        => requireWebScheme
            ? $"{optionsType.Name} endpoint must be configured as an absolute http or https URI."
            : $"{optionsType.Name} endpoint must be configured as an absolute URI.";
}
