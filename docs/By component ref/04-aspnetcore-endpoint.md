# 4. Implementing the ASP.NET Core data portal endpoint

`A5Soft.CARMA.DataPortal.Http.Server` hosts the server data portal over HTTP in ASP.NET Core. It is small and
transport-only: it translates an HTTP request into the canonical call, hands it to the app's
`ServerDataPortalBase`, and writes the response back, streaming bodies both ways. Its full source is in
[§13](#13-source-code).

**Contents**

1. [Project setup](#1-project-setup)
2. [Hosting](#2-hosting)
3. [Mapping the endpoint](#3-mapping-the-endpoint)
4. [How a request is processed](#4-how-a-request-is-processed)
5. [Options and limits](#5-options-and-limits)
6. [CORS for browser clients](#6-cors-for-browser-clients)
7. [Authentication, authorization and other middleware](#7-authentication-authorization-and-other-middleware)
8. [Kestrel, IIS and reverse proxies](#8-kestrel-iis-and-reverse-proxies)
9. [Security](#9-security)
10. [Operations](#10-operations)
11. [Deployment checklist](#11-deployment-checklist)
12. [Pitfalls](#12-pitfalls)
13. [Source code](#13-source-code)

---

## 1. Project setup

- Target **.NET 5** (`net5.0`), `FrameworkReference Microsoft.AspNetCore.App`, a project reference to
  `A5Soft.CARMA.Application` (Newtonsoft.Json comes with it).
- The server host project references the endpoint project, the app's assemblies and the backend assemblies.

| Type | Namespace | Visibility | Role |
|---|---|---|---|
| `CarmaDataPortalEndpointRouteBuilderExtensions` | `Microsoft.AspNetCore.Builder` | public | `MapCarmaDataPortal<TPortal>(pattern)` |
| `HttpDataPortalEndpoint` | `A5Soft.CARMA.DataPortal.Http.Server` | public | `HandleAsync<TPortal>` (POST), `HandleSettingsAsync<TPortal>` (GET): for custom routing |
| `MultipartRequestBody` | same | internal | `IDataPortalRequestBody` over the multipart body |
| `RejectingReadStream` | same | internal | turns a broken file part into a refusal instead of a server error |

The extension lives in `Microsoft.AspNetCore.Builder`, as ASP.NET Core's own `Map...` extensions do, so `Startup`
needs no extra `using`.

---

## 2. Hosting

### 2.1 .NET 5 (`Startup`)

```csharp
public static class Program
{
    public static void Main(string[] args)
        => Host.CreateDefaultBuilder(args)
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateOnBuild = true;      // every missing dependency fails at startup
                options.ValidateScopes = true;
            })
            .ConfigureWebHostDefaults(web => web.UseStartup<Startup>())
            .Build()
            .Run();
}

public sealed class Startup
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        new AcmeIoCManager(services, _configuration, BuildConfiguration.Server, _environment.IsDevelopment(),
            BindingMode.WinForms,   // irrelevant on a server: no UI binds there
            AppAssemblies.Server, appPath: _environment.ContentRootPath).Initialize();

        services.AddRouting();
        // services.AddCors(...)       only for browser clients on another origin (§6)
        // services.AddSingleton<AcmeServerDataPortal>();   if not registered through IAssemblyDependencies
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        // app.UseCors();              (§6)
        // no response compression or body buffering in front of the data portal (§7)
        app.UseEndpoints(endpoints => endpoints.MapCarmaDataPortal<AcmeServerDataPortal>("/api/dataportal"));
    }
}
```

### 2.2 ASP.NET server with a web UI (`Client | Server`)

The same, with `BuildConfiguration.Client | BuildConfiguration.Server`, plus the UI (Razor Pages, MVC, Blazor
Server). The UI's scopes run use cases locally; the mapped endpoint serves remote clients.

### 2.3 .NET 6+ minimal hosting

The calls are the same:

```csharp
var builder = WebApplication.CreateBuilder(args);
new AcmeIoCManager(builder.Services, builder.Configuration, BuildConfiguration.Server,
    builder.Environment.IsDevelopment(), BindingMode.WinForms, AppAssemblies.Server,
    appPath: builder.Environment.ContentRootPath).Initialize();

var app = builder.Build();
app.MapCarmaDataPortal<AcmeServerDataPortal>("/api/dataportal");
app.Run();
```

---

## 3. Mapping the endpoint

```csharp
IEndpointConventionBuilder MapCarmaDataPortal<TPortal>(this IEndpointRouteBuilder endpoints,
    string pattern = "/dataportal") where TPortal : ServerDataPortalBase
```

- Maps **GET** (server settings) and **POST** (calls) on one route.
- `TPortal` is resolved from the **request's** services for every request (`GetRequiredService<TPortal>()`): it
  must be registered (singleton or scoped, [03 §9.2](03-app-data-portal-components.md#92-registration-and-lifetime)).
- The returned builder applies conventions to both methods, e.g. `.RequireCors("dataportal")`,
  `.RequireHost("api.acme.example")`. **Never `.RequireAuthorization()`** (§7).
- The client's endpoint address is the absolute URL of this route, including any path base:
  `https://acme.example/api/dataportal`.
- For custom routing (e.g. a controller or your own middleware), call `HttpDataPortalEndpoint.HandleAsync<TPortal>`
  for POST and `HandleSettingsAsync<TPortal>` for GET.

---

## 4. How a request is processed

### 4.1 GET: server settings

1. Resolve `TPortal`; serialize `portal.GetServerSettings()` (app name, version, limits, plugin fingerprint).
2. Answer `200`, `X-Carma-Protocol: 2`, `Cache-Control: no-store`, `application/json`, with a `Content-Length`.

No authentication, no custom request header (no CORS preflight).

### 4.2 POST: a call

1. Resolve `TPortal`, the server options (`portal.Options`) and `IHttpDataPortalServerOptions` (defaults if not
   registered).
2. **Raise the host's body size limit** for this request to exactly what the options allow:
   `MaxPayloadLength + MaxUploadLength + (MaxFileCount + 1) × 4096` (`HttpDataPortalServerOptions.GetMaxRequestLength`),
   if the host lets it be changed. Kestrel's default 30 MB would otherwise cut uploads short.
3. Note whether the client sent `Expect: 100-continue`.
4. **Read the call from the headers** (`X-Carma-Protocol` must be `2`; each header ≤ `MaxRequestHeaderLength`;
   `Authorization: Carma <token>`; file count a valid number). A malformed header is refused here, from the
   headers alone.
5. Create the multipart body reader (content type must be `multipart/form-data` with a valid boundary ≤ 70 chars;
   part headers ≤ `MaxPartHeadersLength`, ≤ 16 headers per part).
6. `portal.HandleAsync(call, body, http.RequestServices, http.RequestAborted)`: the server pipeline
   ([01 §7](01-data-portal-mechanics.md#7-a-call-on-the-server)). The body is read only after the call is authorized,
   payload first (must be the first part, named `payload`), then files in order (`file-0`, `file-1`, ...).
7. Write the response:
   - an **error refused before the body was read, with `Expect: 100-continue`** → **417** with
     `X-Carma-Result: error` and the error JSON. Kestrel sends `100 Continue` only when the app first reads the
     body, so the client never sends its files.
   - any other **error** → first **drain** the rest of the request body (an HTTP/1.1 client still sending would
     otherwise see a reset connection instead of the answer; the body size limit bounds the drain), then **200**
     with the error JSON.
   - **none / value / file** → **200**; a refreshed identity first in the body (`X-Carma-Identity-Length`), then the
     value JSON or the file content, streamed (`CopyBufferSize`).
8. Dispose the response: spooled upload files are deleted, result buffers released.

A download streams after the headers are sent: a failure while streaming aborts the response; the client gets an
`IOException` reading the file.

---

## 5. Options and limits

Three option classes affect the endpoint (all bound from the app configuration by the IoC manager, sections named
after the classes; reference in [05](05-configuration-reference.md)):

| Section | Key settings (defaults) | Effect on the endpoint |
|---|---|---|
| `DataPortalServerOptions` | `MaxFileCount` 50, `MaxFileLength` 5 MB, `MaxUploadLength` 250 MB, `MaxPayloadLength` 4 MB, `MaxDepth` 32, `CopyBufferSize` 81920, `SpoolDirectory` (system temp) | enforced by the portal; published to clients; the body size limit derives from them |
| `HttpDataPortalServerOptions` (`Server` builds) | `MaxRequestHeaderLength` 8 KB, `MaxPartHeadersLength` 16 KB | header and part header limits of the endpoint |
| `HttpDataPortalClientOptions` (clients) | `MaxRequestHeaderLength` 8 KB, `MaxRequestHeadersLength` 16 KB, `CallTimeout` 100 s | must fit within the server's and the proxies' limits |

Size the limits end to end:

- `HttpDataPortalServerOptions.MaxRequestHeaderLength` ≥ the clients' `MaxRequestHeaderLength`.
- Kestrel's `MaxRequestHeadersTotalSize` (default 32 KB) and every proxy's header limits ≥ the clients'
  `MaxRequestHeadersLength`.
- Every proxy's body limit ≥ `GetMaxRequestLength(options)` (≈ 254 MB with the defaults).
- Timeouts along the path ≥ the time the largest upload takes, and ≥ the clients' `CallTimeout`.
- The spool directory's free space ≥ `MaxUploadLength` × the number of concurrent uploading calls.

---

## 6. CORS for browser clients

Needed only for Blazor WebAssembly (or any browser client) served from **another origin** than the API.

```csharp
services.AddCors(options => options.AddPolicy("dataportal", policy => policy
    .WithOrigins("https://app.acme.example")
    .WithMethods("GET", "POST")
    .WithHeaders(DataPortalHeaders.RequestHeaders.Concat(new[] { "Authorization", "Content-Type" }).ToArray())
    .WithExposedHeaders(DataPortalHeaders.ResponseHeaders.ToArray())));

// Configure: app.UseRouting(); app.UseCors(); app.UseEndpoints(...)
endpoints.MapCarmaDataPortal<AcmeServerDataPortal>("/api/dataportal").RequireCors("dataportal");
```

- **Exposed headers are essential**: without them the browser hides `X-Carma-Result` and every call looks like an
  infrastructure failure (`ServerError`).
- The settings GET sends no custom header and needs no preflight; every POST is preflighted (custom headers).
- `Expect` is never sent by browsers: refused calls always transfer their files first.
- Credentials (cookies) are not used by the data portal; do not add `AllowCredentials` for it.

---

## 7. Authentication, authorization and other middleware

- **Do not put an authorization policy on the endpoint** (`RequireAuthorization`, `[Authorize]`, a fallback policy
  that applies to all endpoints). The settings GET must stay anonymous (a client needs it before login), anonymous
  calls (login) must reach the portal, and the portal authorizes every call itself. With a global fallback policy,
  mark the endpoint `.AllowAnonymous()`.
- ASP.NET Core authentication middleware (`UseAuthentication`) may run for the rest of the app; the portal does not
  use `HttpContext.User`, it verifies `Authorization: Carma <token>` itself. An authentication handler that reacts
  to every `Authorization` header (e.g. a JWT bearer handler configured as the default scheme) ignores the `Carma`
  scheme; make sure no handler challenges or rejects requests to this route.
- **Avoid middleware that buffers or rewrites bodies** on this route: request buffering (`EnableBuffering`), body
  logging (HTTP logging with body capture), response compression (excludes `application/octet-stream` by default,
  but would buffer `application/json` errors and change lengths). They defeat streaming and the early refusal.
- **Do not log the `Authorization` header** (HTTP logging, proxies): it carries the token.
- Exception handling and status code pages middleware do not interfere: the endpoint never throws and always writes
  a body.

---

## 8. Kestrel, IIS and reverse proxies

### 8.1 Kestrel

| Setting | Default | Note |
|---|---|---|
| `MaxRequestBodySize` | 30 MB | raised per request by the endpoint; leave the global default for the rest of the app |
| `MaxRequestHeadersTotalSize` | 32 KB | ≥ clients' `MaxRequestHeadersLength` |
| `MaxRequestLineSize` | 8 KB | the route only; fine |
| `MinRequestBodyDataRate` | 240 B/s after 5 s grace | slow uploads below it are aborted |
| `KeepAliveTimeout`, `RequestHeadersTimeout` | 130 s, 30 s | |

### 8.2 IIS (in-process or out-of-process)

- **`maxAllowedContentLength`** (request filtering, default 30,000,000 bytes) is enforced by IIS **before** the app
  runs; the endpoint cannot raise it. Set it in `web.config` ≥ the max request length:

  ```xml
  <system.webServer>
    <security>
      <requestFiltering>
        <requestLimits maxAllowedContentLength="268435456" />
      </requestFiltering>
    </security>
  </system.webServer>
  ```
- Header size limits of HTTP.sys (`MaxFieldLength`, `MaxRequestBytes`, 16 KB by default) must fit the clients'
  header limits.
- A server or proxy that answers `100 Continue` by itself makes the client send its files before the portal can
  refuse; the outcome is the same, only bandwidth is wasted.

### 8.3 Reverse proxies (nginx, YARP, gateways)

| Concern | nginx example | Why |
|---|---|---|
| Body size | `client_max_body_size 260m;` (default 1m) | uploads |
| Upload streaming | `proxy_request_buffering off;` | otherwise the proxy reads the whole upload first (and answers `100 Continue` itself), defeating early refusal |
| Download streaming | `proxy_buffering off;` (or accept buffering to disk) | large downloads |
| Timeouts | `proxy_read_timeout`, `proxy_send_timeout` ≥ the clients' `CallTimeout` | long calls |
| Header sizes | `large_client_header_buffers 4 16k;` | 8 KB data portal headers |
| HTTP version to Kestrel | `proxy_http_version 1.1;` | keep-alive, chunked bodies |
| Forwarded headers | `UseForwardedHeaders` in the app | correct scheme/host for anything that needs it |

A gateway answering with its own error page (502, 504, 413) is reported to the client as `ServiceUnavailable`
(408/502/503/504) or `ServerError`, because it lacks `X-Carma-Result`.

---

## 9. Security

- **HTTPS only** (the token is in a header); HSTS for browser clients.
- **Token verification** in `VerifyIdentityAsync`: signature, issuer, audience, expiry. It runs before uploads are
  accepted, so keep it fast (no database round trip per call if possible).
- **Anonymous use cases**: refuse `identity == null` in `AuthorizeAsync` except for explicitly marked use cases.
- **Rate limiting** of the route (login brute force, upload floods) at the proxy or with middleware.
- **Spool directory**: a dedicated directory, writable by the app only, on a volume with enough space; files are
  deleted on close (`DeleteOnClose`), but a killed process can leave files behind in temp: clean the directory at
  startup if you configure your own.
- **Error details** never leave the server for `ServerError`/`BadRequest`; correlation ids tie client reports to
  server logs.
- **Server settings are public**: app name, version, limits, plugin fingerprint. If the version itself is sensitive,
  restrict network access to the endpoint instead.

---

## 10. Operations

- **Correlation ids**: every `ServerError`, `BadRequest`, `Unauthorized`, `ServiceUnavailable` and
  `VersionMismatch` is logged through the app's `ILogger` with a correlation id that the client receives.
- **Health checks**: the settings GET is a cheap liveness probe of the data portal (expect 200 and
  `X-Carma-Protocol`).
- **Rolling deployments**: clients cache server settings per endpoint and refetch after a `BadRequest` or
  `VersionMismatch`, so a server update is detected; plan app version increments by `AppVersionMatch`
  ([01 §10](01-data-portal-mechanics.md#10-versions-protocol-and-plugins-compatibility-checks)). Servers behind one load
  balancer must run the same version and plugins.

---

## 11. Deployment checklist

- [ ] `IAppVersionProvider` gives the same name and a compatible version on server and clients.
- [ ] The same plugins (ids and versions) on server and clients.
- [ ] `DataPortalServerOptions` sized for the app's largest uploads; `SpoolDirectory` exists and has room.
- [ ] IIS `maxAllowedContentLength` / proxy body limits ≥ `GetMaxRequestLength`.
- [ ] Proxy header limits ≥ clients' header limits; timeouts ≥ `CallTimeout`.
- [ ] Upload streaming through proxies (`proxy_request_buffering off` or equivalent).
- [ ] CORS policy with exposed headers, if browser clients come from another origin.
- [ ] No authorization policy, body buffering or response compression on the route.
- [ ] HTTPS; `Authorization` not logged.
- [ ] `ValidateOnBuild` on; the convention test passes.

---

## 12. Pitfalls

| Pitfall | Symptom | Fix |
|---|---|---|
| `RequireAuthorization` / global fallback policy | clients cannot fetch settings: `ServerError` or `ServiceUnavailable` at login | no policy, or `.AllowAnonymous()` |
| Portal class not registered | every request: 500 from ASP.NET (`GetRequiredService`) | register it (`IAssemblyDependencies` or `AddSingleton`) |
| IIS `maxAllowedContentLength` not raised | uploads > 30 MB fail with 404.13 → `ServerError` | `web.config` |
| nginx `client_max_body_size` default | uploads > 1 MB fail with 413 → `ServerError` | raise it |
| Missing CORS exposed headers | browser: every call `ServerError` | `WithExposedHeaders(DataPortalHeaders.ResponseHeaders)` |
| Response compression on the route | errors buffered; content length changed | exclude the route |
| Slow token verification | with `Expect: 100-continue`, the client's 1 s timeout passes and files are sent anyway | fast verification; longer `Expect100ContinueTimeout` on clients |
| Spool directory missing | startup fails (`DataPortalServerOptions` validation) | create it, or leave unset for temp |
| Several servers with different versions behind one balancer | intermittent `VersionMismatch` / `BadRequest` | deploy all at once, or drain old ones |
| Client address without the route's path base | 404 → `ServerError` | absolute URL of the mapped route |

---

## 13. Source code

The complete endpoint project (namespaces as in the agreed layout).

### 13.1 `A5Soft.CARMA.DataPortal.Http.Server.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
    <LangVersion>8.0</LangVersion>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <Description>Hosts the CARMA server data portal in ASP.NET Core over HTTP</Description>
  </PropertyGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <ProjectReference Include="..\A5Soft.CARMA.Application\A5Soft.CARMA.Application.csproj" />
  </ItemGroup>

</Project>
```

### 13.2 `CarmaDataPortalEndpointRouteBuilderExtensions.cs`

```csharp
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Http;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.DataPortal.Server;
using A5Soft.CARMA.DataPortal.Http.Server;

// in the ASP.NET Core namespace, as ASP.NET Core's own Map... extensions are, so that Startup needs no using
namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Maps the HTTP endpoint of the server data portal.
    /// </summary>
    public static class CarmaDataPortalEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the server data portal, served by <see cref="HttpDataPortalEndpoint"/>: POST for the calls,
        /// GET for the <see cref="ServerSettings"/>.
        /// </summary>
        /// <typeparam name="TPortal">the application's server data portal, registered in the container</typeparam>
        /// <param name="endpoints">the endpoint route builder</param>
        /// <param name="pattern">the route of the endpoint</param>
        /// <returns>a builder to add conventions to the endpoint, e.g. a CORS policy
        /// (<c>.RequireCors(...)</c>); they apply to both methods</returns>
        /// <remarks>
        /// <para>Register <see cref="IHttpDataPortalServerOptions"/> (usually by the configuration scan of
        /// <see cref="HttpDataPortalServerOptions"/>) to change the HTTP limits; without it, the defaults
        /// apply.</para>
        /// <para>The GET must stay open to anonymous callers: a client needs the settings before it can
        /// log in. Do not put an authorization policy on this endpoint; the portal authorizes each call
        /// itself.</para>
        /// </remarks>
        public static IEndpointConventionBuilder MapCarmaDataPortal<TPortal>(
            this IEndpointRouteBuilder endpoints, string pattern = "/dataportal")
            where TPortal : ServerDataPortalBase
        {
            if (null == endpoints) throw new ArgumentNullException(nameof(endpoints));

            return endpoints.MapMethods(pattern, new[] { HttpMethods.Get, HttpMethods.Post },
                (RequestDelegate)(http => HttpMethods.IsGet(http.Request.Method)
                    ? HttpDataPortalEndpoint.HandleSettingsAsync<TPortal>(http)
                    : HttpDataPortalEndpoint.HandleAsync<TPortal>(http)));
        }
    }
}
```

### 13.3 `HttpDataPortalEndpoint.cs`

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Http;
using A5Soft.CARMA.Application.DataPortal.Protocol;
using A5Soft.CARMA.Application.DataPortal.Server;

namespace A5Soft.CARMA.DataPortal.Http.Server
{
    /// <summary>
    /// The HTTP endpoint of the server data portal: translates an HTTP request into the canonical form
    /// (a <see cref="DataPortalCall"/> from the headers, an <see cref="IDataPortalRequestBody"/> over the
    /// multipart body), hands it to the server data portal, and translates the
    /// <see cref="DataPortalResponse"/> back into HTTP. Bodies are streamed both ways.
    /// </summary>
    /// <remarks>
    /// <para>Every portal outcome is answered with HTTP 200 and described by the headers, except a call
    /// that asked <c>Expect: 100-continue</c> and was refused before its body was read: that one is
    /// answered with 417 Expectation Failed, so that the client never sends the files.</para>
    /// <para>A call refused while its body is arriving, or refused without the expectation, is answered
    /// only after the rest of the body has been read and discarded: an HTTP/1.1 client that is still
    /// sending does not read the response, and would otherwise see a broken connection instead of the
    /// error. The host's request size limit bounds the drain.</para>
    /// <para>A download is streamed after the headers have been sent, so a failure while it is streamed
    /// cannot be reported as an error: the host aborts the response, and the client gets an
    /// <see cref="IOException"/> while reading the file.</para>
    /// <para>A GET is answered with the JSON of the <see cref="ServerSettings"/>, without authentication,
    /// and with the <c>X-Carma-Protocol</c> header, so that the client can tell a server of another protocol
    /// version from one of another application version.</para>
    /// <para>Avoid middleware that buffers bodies on this endpoint (response compression, body logging,
    /// request buffering): it would defeat the streaming.</para>
    /// </remarks>
    public static class HttpDataPortalEndpoint
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);


        /// <summary>
        /// Handles a request for the server settings (a GET).
        /// </summary>
        /// <typeparam name="TPortal">the application's server data portal, registered in the container</typeparam>
        /// <param name="http">the HTTP context of the request</param>
        public static async Task HandleSettingsAsync<TPortal>(HttpContext http)
            where TPortal : ServerDataPortalBase
        {
            if (null == http) throw new ArgumentNullException(nameof(http));

            var portal = http.RequestServices.GetRequiredService<TPortal>();
            var json = Serialize(writer => DataPortalSerialization
                .CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth)
                .Serialize(writer, portal.GetServerSettings()));

            http.Response.StatusCode = 200;
            http.Response.Headers[DataPortalHeaders.Protocol] = DataPortalHeaders.CurrentProtocolVersion;
            // the settings change with every deployment; never let a browser or a proxy keep them
            http.Response.Headers["Cache-Control"] = "no-store";
            http.Response.ContentType = "application/json; charset=utf-8";
            http.Response.ContentLength = json.Length;

            await http.Response.Body.WriteAsync(json, 0, json.Length, http.RequestAborted).ConfigureAwait(false);
        }

        /// <summary>
        /// Handles a data portal request.
        /// </summary>
        /// <typeparam name="TPortal">the application's server data portal, registered in the container</typeparam>
        /// <param name="http">the HTTP context of the request</param>
        public static async Task HandleAsync<TPortal>(HttpContext http)
            where TPortal : ServerDataPortalBase
        {
            if (null == http) throw new ArgumentNullException(nameof(http));

            var portal = http.RequestServices.GetRequiredService<TPortal>();
            var options = portal.Options;
            var httpOptions = http.RequestServices.GetService<IHttpDataPortalServerOptions>() ?? new HttpDataPortalServerOptions();
            var aborted = http.RequestAborted;

            // the host's default body limit (30 MB in Kestrel) would cut uploads short;
            // the portal enforces its own, finer limits
            var bodySize = http.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (null != bodySize && !bodySize.IsReadOnly) bodySize.MaxRequestBodySize = HttpDataPortalServerOptions.GetMaxRequestLength(options);

            var expectContinue = http.Request.Headers["Expect"].Any(v =>
                null != v && v.IndexOf("100-continue", StringComparison.OrdinalIgnoreCase) >= 0);

            DataPortalResponse response;
            try
            {
                var call = ReadCall(http.Request, httpOptions);
                var body = new MultipartRequestBody(http.Request, httpOptions);

                response = await portal.HandleAsync(call, body, http.RequestServices, aborted).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // the request could not be translated: refused from its headers alone
                response = portal.CreateErrorResponse(ex, true);
            }

            using (response)
            {
                var refuseBody = response.Kind == DataPortalResultKind.Error && response.RefusedBeforeBody && expectContinue;

                if (response.Kind == DataPortalResultKind.Error && !refuseBody)
                    await DrainAsync(http.Request.Body, aborted).ConfigureAwait(false);

                await WriteResponseAsync(http.Response, response, refuseBody ? 417 : 200, options, aborted)
                    .ConfigureAwait(false);
            }
        }


        #region Request

        /// <summary>
        /// The call description, from the headers.
        /// </summary>
        private static DataPortalCall ReadCall(HttpRequest request, IHttpDataPortalServerOptions httpOptions)
        {
            string Header(string name)
            {
                var values = request.Headers[name];
                if (StringValues.IsNullOrEmpty(values)) return null;

                var value = values.ToString();
                if (value.Length > httpOptions.MaxRequestHeaderLength)
                    throw Rejected($"header {name} is longer than {httpOptions.MaxRequestHeaderLength}");

                return value;
            }

            if (Header(DataPortalHeaders.Protocol)?.Trim() != DataPortalHeaders.CurrentProtocolVersion)
                throw Rejected("the protocol version is missing or not supported");

            var fileCountHeader = Header(DataPortalHeaders.FileCount);
            var fileCount = null == fileCountHeader ? 0 : DataPortalHeaderValues.DecodeNumber(fileCountHeader)
                ?? throw Rejected("the file count is not valid");
            if (fileCount > int.MaxValue) throw Rejected("the file count is not valid");

            string token = null;
            var authorization = Header("Authorization");
            if (null != authorization)
            {
                var prefix = DataPortalHeaders.AuthorizationScheme + " ";
                if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    throw Rejected("the authorization scheme is not supported");

                token = authorization.Substring(prefix.Length).Trim();
            }

            return new DataPortalCall(
                DataPortalHeaderValues.DecodeText(Header(DataPortalHeaders.UseCase)),
                DataPortalHeaderValues.DecodeList(Header(DataPortalHeaders.ParameterTypes)),
                token,
                DataPortalHeaderValues.DecodeText(Header(DataPortalHeaders.Culture)),
                DataPortalHeaderValues.DecodeText(Header(DataPortalHeaders.UICulture)),
                (int)fileCount,
                DataPortalHeaderValues.DecodeList(Header(DataPortalHeaders.GenericArguments)),
                // checked against the server's by the portal, before anything else is read
                DataPortalHeaderValues.DecodeText(Header(DataPortalHeaders.Plugins))?.Trim());
        }

        private static async Task DrainAsync(Stream body, CancellationToken cancellationToken)
        {
            var buffer = new byte[16 * 1024];
            try
            {
                while (await body.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false) > 0) { }
            }
            catch (Exception)
            {
                // the client has gone, or the host's size limit has been reached: nothing more to do
            }
        }

        #endregion

        #region Response

        private static async Task WriteResponseAsync(HttpResponse http, DataPortalResponse response, int statusCode,
            IDataPortalServerOptions options, CancellationToken cancellationToken)
        {
            http.StatusCode = statusCode;

            if (response.Kind == DataPortalResultKind.Error)
            {
                var error = SerializeError(response.Error, options);

                http.Headers[DataPortalHeaders.Result] = DataPortalHeaders.ResultKinds.Error;
                http.ContentType = "application/json; charset=utf-8";
                http.ContentLength = error.Length;
                await http.Body.WriteAsync(error, 0, error.Length, cancellationToken).ConfigureAwait(false);
                return;
            }

            // a refreshed identity goes first in the body: a full identity can exceed proxies'
            // response header limits
            var identity = null == response.RefreshedIdentity
                ? Array.Empty<byte>()
                : Serialize(writer => PortalMessageSerializer().Serialize(writer, response.RefreshedIdentity));
            if (identity.Length > 0)
                http.Headers[DataPortalHeaders.IdentityLength] = DataPortalHeaderValues.EncodeNumber(identity.Length);

            http.ContentType = "application/octet-stream";

            switch (response.Kind)
            {
                case DataPortalResultKind.None:
                    http.Headers[DataPortalHeaders.Result] = DataPortalHeaders.ResultKinds.None;
                    http.ContentLength = identity.Length;
                    await http.Body.WriteAsync(identity, 0, identity.Length, cancellationToken).ConfigureAwait(false);
                    break;

                case DataPortalResultKind.Value:
                    http.Headers[DataPortalHeaders.Result] = DataPortalHeaders.ResultKinds.Value;
                    http.Headers[DataPortalHeaders.ResultType] = DataPortalHeaderValues.EncodeText(response.ResultType);
                    http.ContentLength = identity.Length + response.Value.Length;
                    await http.Body.WriteAsync(identity, 0, identity.Length, cancellationToken).ConfigureAwait(false);
                    // Stream.WriteAsync(ReadOnlyMemory<byte>) writes the memory as it is, no array needed
                    await http.Body.WriteAsync(response.Value, cancellationToken).ConfigureAwait(false);
                    break;

                case DataPortalResultKind.File:
                    var file = response.File;
                    long? length = file.Content.CanSeek ? file.Content.Length - file.Content.Position : (long?)null;

                    http.Headers[DataPortalHeaders.Result] = DataPortalHeaders.ResultKinds.File;
                    http.Headers[DataPortalHeaders.FileName] = DataPortalHeaderValues.EncodeText(file.FileName);
                    http.Headers[DataPortalHeaders.FileContentType] = DataPortalHeaderValues.EncodeText(file.ContentType);
                    if (length.HasValue)
                        http.Headers[DataPortalHeaders.FileLength] = DataPortalHeaderValues.EncodeNumber(length.Value);
                    http.ContentLength = identity.Length + length;

                    await http.Body.WriteAsync(identity, 0, identity.Length, cancellationToken).ConfigureAwait(false);
                    await file.Content.CopyToAsync(http.Body, options.CopyBufferSize, cancellationToken).ConfigureAwait(false);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown result kind {response.Kind}.");
            }
        }

        private static byte[] SerializeError(RemoteError error, IDataPortalServerOptions options)
        {
            try
            {
                return Serialize(writer => PortalMessageSerializer().Serialize(writer, error));
            }
            catch (Exception)
            {
                // the error data supplied by the mapper cannot be serialized
                var plain = new RemoteError { Kind = error.Kind, Message = error.Message, CorrelationId = error.CorrelationId };
                return Serialize(writer => PortalMessageSerializer().Serialize(writer, plain));
            }
        }

        private static JsonSerializer PortalMessageSerializer()
            => DataPortalSerialization.CreateSerializer(DataPortalSerialization.PortalMessageMaxDepth);

        private static byte[] Serialize(Action<JsonWriter> write)
        {
            using (var result = new MemoryStream())
            {
                using (var textWriter = new StreamWriter(result, Utf8, 4096, true))
                using (var jsonWriter = new JsonTextWriter(textWriter))
                {
                    write(jsonWriter);
                }

                return result.ToArray();
            }
        }

        #endregion

        private static RemoteCallRejectedException Rejected(string reason)
            => new RemoteCallRejectedException(RemoteErrorKind.BadRequest, $"Remote call was rejected: {reason}.");
    }
}
```

### 13.4 `MultipartRequestBody.cs`

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using A5Soft.CARMA.Application.DataPortal;
using A5Soft.CARMA.Application.DataPortal.Http;
using A5Soft.CARMA.Application.DataPortal.Server;

namespace A5Soft.CARMA.DataPortal.Http.Server
{
    /// <summary>
    /// The data of an HTTP data portal call, read from its multipart body as it arrives, with
    /// ASP.NET Core's <see cref="MultipartReader"/>: the payload part, then a part per file.
    /// </summary>
    /// <remarks>Malformed data, and a body that ends early, surface as a
    /// <see cref="RemoteCallRejectedException"/>.</remarks>
    internal sealed class MultipartRequestBody : IDataPortalRequestBody
    {
        private const int MaxBoundaryLength = 70;

        private readonly MultipartReader _reader;
        private int _nextFile;


        /// <summary>
        /// Creates a reader of the body of the request given.
        /// </summary>
        /// <exception cref="RemoteCallRejectedException">the body is not multipart form data</exception>
        public MultipartRequestBody(HttpRequest request, IHttpDataPortalServerOptions options)
        {
            if (!MediaTypeHeaderValue.TryParse(request.ContentType, out var mediaType)
                || !mediaType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                throw Rejected("the body is not multipart form data");

            var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary) || boundary.Length > MaxBoundaryLength)
                throw Rejected("the multipart boundary is not valid");

            _reader = new MultipartReader(boundary, request.Body)
            {
                HeadersLengthLimit = options.MaxPartHeadersLength,
                HeadersCountLimit = 16
            };
        }


        /// <inheritdoc />
        public async Task<ReadOnlyMemory<byte>> ReadPayloadAsync(int maxLength, CancellationToken cancellationToken)
        {
            var section = await NextSectionAsync(cancellationToken).ConfigureAwait(false)
                ?? throw Rejected("the body has no payload part");

            if (GetName(section, out _) != DataPortalHeaders.PayloadPartName)
                throw Rejected("the first part is not the payload");

            var payload = new MemoryStream();
            var buffer = new byte[16 * 1024];
            int read;
            while ((read = await ReadAsync(section.Body, buffer, cancellationToken).ConfigureAwait(false)) > 0)
            {
                if (payload.Length + read > maxLength) throw Rejected($"the payload is longer than {maxLength}");
                payload.Write(buffer, 0, read);
            }

            // over the stream's own buffer, which is longer than the payload: no copy to an exact-length array.
            // Nothing needs releasing: the buffer is managed memory, collected with the request
            return new ReadOnlyMemory<byte>(payload.GetBuffer(), 0, (int)payload.Length);
        }

        /// <inheritdoc />
        public async Task<IncomingFile> ReadNextFileAsync(CancellationToken cancellationToken)
        {
            var section = await NextSectionAsync(cancellationToken).ConfigureAwait(false);
            if (null == section) return null;

            if (GetName(section, out var disposition) != DataPortalHeaders.FilePartName(_nextFile))
                throw Rejected($"the next part is not file {_nextFile}");

            _nextFile++;

            var fileName = disposition.FileNameStar.HasValue
                ? disposition.FileNameStar.Value
                : HeaderUtilities.RemoveQuotes(disposition.FileName).Value;

            return new IncomingFile(fileName, section.ContentType, new RejectingReadStream(section.Body));
        }


        private async Task<MultipartSection> NextSectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await _reader.ReadNextSectionAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException)
            {
                throw Rejected("the multipart body is malformed or incomplete", ex);
            }
        }

        private static async Task<int> ReadAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            try
            {
                return await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException)
            {
                throw Rejected("the multipart body is malformed or incomplete", ex);
            }
        }

        private static string GetName(MultipartSection section, out ContentDispositionHeaderValue disposition)
        {
            if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out disposition)
                || !disposition.DispositionType.Equals("form-data", StringComparison.OrdinalIgnoreCase))
                throw Rejected("a part has no form data disposition");

            return HeaderUtilities.RemoveQuotes(disposition.Name).Value;
        }

        internal static RemoteCallRejectedException Rejected(string reason, Exception inner = null)
            => new RemoteCallRejectedException(RemoteErrorKind.BadRequest, $"Remote call was rejected: {reason}.", inner);
    }
}
```

### 13.5 `RejectingReadStream.cs`

```csharp
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace A5Soft.CARMA.DataPortal.Http.Server
{
    /// <summary>
    /// The body of a file part as the server data portal reads it: a read that fails because the
    /// multipart body is malformed or ends early (or the client has gone) surfaces as a refusal
    /// rather than as a server error.
    /// </summary>
    internal sealed class RejectingReadStream : Stream
    {
        private readonly Stream _inner;

        public RejectingReadStream(Stream inner) => _inner = inner ?? throw new ArgumentNullException(nameof(inner));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                return _inner.Read(buffer, offset, count);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException)
            {
                throw MultipartRequestBody.Rejected("a file part is malformed or incomplete", ex);
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            try
            {
                return await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException)
            {
                throw MultipartRequestBody.Rejected("a file part is malformed or incomplete", ex);
            }
        }

        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _inner.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException)
            {
                throw MultipartRequestBody.Rejected("a file part is malformed or incomplete", ex);
            }
        }

        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
```
