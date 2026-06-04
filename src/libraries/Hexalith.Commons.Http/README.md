# Hexalith.Commons.Http

Domain-agnostic typed `HttpClient` dependency-injection registration with endpoint-options validation.

`HttpClientRegistration.AddTypedHttpClient<TClient, TImplementation, TOptions>(...)` extracts the duplicated
`AddXxxClient()` pattern shared by Hexalith module clients. A domain module supplies only the client
interface, the typed implementation, its options type, and an endpoint selector; the helper owns
registration, options validation, and the returned `IHttpClientBuilder` (so callers can chain message
handlers such as a bearer-token `DelegatingHandler`).

## Validation

- The endpoint must be an absolute URI.
- When `requireWebScheme: true`, the endpoint must use the `http` or `https` scheme.
- Validation timing is selectable:
  - `HttpClientEndpointValidation.OnResolve` (default) — lazy, via `IOptions<TOptions>.Validate` (the
    Folders/Projects shape).
  - `HttpClientEndpointValidation.OnRegistration` — eager, throwing `InvalidOperationException` immediately
    (the Conversations shape, the stronger guarantee).

## Example

```csharp
services.AddTypedHttpClient<IConversationClient, ConversationClient, ConversationClientOptions>(
    configure,
    static options => options.Endpoint,
    HttpClientEndpointValidation.OnRegistration,
    requireWebScheme: true);
```
