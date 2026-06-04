// <copyright file="HttpClientRegistrationTest.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Http.Tests;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shouldly;

/// <summary>
/// Verifies the promoted domain-agnostic typed-HttpClient registration helper, covering the five
/// acceptance cases (missing endpoint, relative URI, non-http(s) scheme, valid endpoint, and the
/// returned <see cref="IHttpClientBuilder"/>) plus the lazy and configuration-bound shapes.
/// </summary>
public sealed class HttpClientRegistrationTest
{
    /// <summary>
    /// Eager validation rejects a missing endpoint at registration time.
    /// </summary>
    [Fact]
    public void EagerRegistrationShouldRejectMissingEndpoint()
    {
        ServiceCollection services = new();

        _ = Should.Throw<InvalidOperationException>(() =>
            services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
                static options => options.Endpoint = null,
                static options => options.Endpoint,
                HttpClientEndpointValidation.OnRegistration,
                requireWebScheme: true));
    }

    /// <summary>
    /// Eager validation rejects a relative (non-absolute) endpoint URI at registration time.
    /// </summary>
    [Fact]
    public void EagerRegistrationShouldRejectRelativeUri()
    {
        ServiceCollection services = new();

        _ = Should.Throw<InvalidOperationException>(() =>
            services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
                static options => options.Endpoint = new Uri("/relative", UriKind.Relative),
                static options => options.Endpoint,
                HttpClientEndpointValidation.OnRegistration,
                requireWebScheme: true));
    }

    /// <summary>
    /// Eager validation rejects a non-http(s) scheme when the web-scheme guard is enabled.
    /// </summary>
    [Fact]
    public void EagerRegistrationShouldRejectNonWebScheme()
    {
        ServiceCollection services = new();

        _ = Should.Throw<InvalidOperationException>(() =>
            services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
                static options => options.Endpoint = new Uri("ftp://example.test/"),
                static options => options.Endpoint,
                HttpClientEndpointValidation.OnRegistration,
                requireWebScheme: true));
    }

    /// <summary>
    /// Eager validation accepts a valid endpoint and registers a resolvable typed client.
    /// </summary>
    [Fact]
    public void EagerRegistrationShouldAcceptValidEndpointAndRegisterTypedClient()
    {
        ServiceCollection services = new();

        _ = services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
            static options => options.Endpoint = new Uri("https://example.test/"),
            static options => options.Endpoint,
            HttpClientEndpointValidation.OnRegistration,
            requireWebScheme: true);

        using ServiceProvider provider = services.BuildServiceProvider();
        ITestClient client = provider.GetRequiredService<ITestClient>();

        _ = client.ShouldBeOfType<TestClient>();
        client.BaseAddress.ShouldBe(new Uri("https://example.test/"));
    }

    /// <summary>
    /// The helper returns an <see cref="IHttpClientBuilder"/> usable for message-handler chaining.
    /// </summary>
    [Fact]
    public void EagerRegistrationShouldReturnHttpClientBuilderForHandlerChaining()
    {
        ServiceCollection services = new();

        IHttpClientBuilder builder = services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
            static options => options.Endpoint = new Uri("https://example.test/"),
            static options => options.Endpoint,
            HttpClientEndpointValidation.OnRegistration,
            requireWebScheme: true);

        _ = builder.ShouldNotBeNull();

        // Prove the builder is usable for handler chaining (the reason it is returned).
        _ = builder.AddHttpMessageHandler(static () => new MarkerHandler());
        using ServiceProvider provider = services.BuildServiceProvider();
        _ = provider.GetRequiredService<ITestClient>().ShouldBeOfType<TestClient>();
    }

    /// <summary>
    /// Lazy validation rejects a missing endpoint at first resolve rather than silently accepting it.
    /// </summary>
    [Fact]
    public void LazyRegistrationShouldRejectMissingEndpointOnResolve()
    {
        ServiceCollection services = new();

        _ = services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
            static options => options.Endpoint = null,
            static options => options.Endpoint,
            HttpClientEndpointValidation.OnResolve,
            requireWebScheme: true);

        using ServiceProvider provider = services.BuildServiceProvider();

        _ = Should.Throw<OptionsValidationException>(provider.GetRequiredService<ITestClient>);
    }

    /// <summary>
    /// Without the web-scheme guard, a non-web absolute URI is accepted (the permissive sibling shape).
    /// </summary>
    [Fact]
    public void LazyRegistrationShouldAcceptValidEndpointWithoutWebSchemeGuard()
    {
        ServiceCollection services = new();

        _ = services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
            static options => options.Endpoint = new Uri("ftp://files.example.test/"),
            static options => options.Endpoint,
            HttpClientEndpointValidation.OnResolve,
            requireWebScheme: false);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ITestClient>().BaseAddress.ShouldBe(new Uri("ftp://files.example.test/"));
    }

    /// <summary>
    /// The configuration-section overload binds the endpoint from configuration and validates it.
    /// </summary>
    [Fact]
    public void ConfigurationSectionRegistrationShouldBindAndValidateEndpoint()
    {
        ServiceCollection services = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestClient:Endpoint"] = "https://configured.example.test/",
            })
            .Build();
        _ = services.AddSingleton(configuration);

        _ = services.AddTypedHttpClient<ITestClient, TestClient, TestClientOptions>(
            "TestClient",
            static options => options.Endpoint,
            requireWebScheme: true);

        using ServiceProvider provider = services.BuildServiceProvider();

        provider.GetRequiredService<ITestClient>().BaseAddress.ShouldBe(new Uri("https://configured.example.test/"));
    }
}
