# 5. Configuration reference

Every framework configuration class derives from `ConfigurationBase`. The IoC manager creates one instance per class
for the app build, binds it from the section named after the class (`RootSection`), calls `Init(appPath,
isDevelopment)` and `Validate()`, and registers it as a singleton under its interface. **A missing section or
setting keeps the default**, so most apps configure little or nothing. An out-of-range value stops startup with a
message listing every problem.

## Sections at a glance

| Section | Interface | Builds | Namespace |
|---|---|---|---|
| `DataPortalClientOptions` | `IDataPortalClientOptions` | all | `A5Soft.CARMA.Application.DataPortal.Client` |
| `DataPortalConnectionConfiguration` | `IDataPortalConnectionConfiguration` | all | `...DataPortal.Client` |
| `HttpDataPortalClientOptions` | `IHttpDataPortalClientOptions` | `Client`, `Client \| Server`, `Client \| Local` | `...DataPortal.Http` |
| `DataPortalServerOptions` | `IDataPortalServerOptions` | all (used by servers) | `...DataPortal.Server` |
| `HttpDataPortalServerOptions` | `IHttpDataPortalServerOptions` | `Server`, `Client \| Server` | `...DataPortal.Http` |
| `PluginsConfiguration` | `IPluginsConfiguration` | all | `A5Soft.CARMA.Application.IoC.Plugins` |

---

## `DataPortalClientOptions` (client)

| Setting | Type | Default | Range / values | Notes |
|---|---|---|---|---|
| `TokenClaimType` | string | `carma:token` | non-empty, no surrounding spaces | The identity claim holding the token. |
| `AppVersionMatch` | enum | `MajorMinor` | `Exact`, `MajorMinor`, `None` | How strictly the server's app version must match. App names must always be equal. |
| `MaxIdentityLength` | int (bytes) | 1,048,576 (1 MB) | 64 KB – 16 MB | Longest refreshed identity accepted from a server (400 claims ≈ 25 KB). |
| `MaxResponseLength` | int (bytes) | 67,108,864 (64 MB) | 1 MB – 1 GB | Longest result value or error read into memory; keep ≥ the server's `MaxPayloadLength` (a save returns what it received). Downloads are not limited by it. |
| `CopyBufferSize` | int (bytes) | 81,920 | 4 KB – 1 MB | Keep < 85,000 so buffers stay off the large object heap. |

The limits a call must respect are **not** configured on the client: they come from the server's settings.

## `DataPortalConnectionConfiguration` (client)

| Setting | Type | Default | Notes |
|---|---|---|---|
| `Transport` | string | unset | `http`, `local` or a custom transport name. Unset: chosen at runtime (or implied local in builds with a backend). |
| `Address` | string (absolute URL) | unset | The server's data portal route for a remote transport. Setting it without `Transport` fails. |

The transport name and whether it needs an address are checked when the endpoint is first used. Used by every
scope that does not `Initialize` its own connection.

## `HttpDataPortalClientOptions` (client, HTTP)

| Setting | Type | Default | Range | Notes |
|---|---|---|---|---|
| `MaxRequestHeaderLength` | int (chars) | 8,192 | 4 KB – 64 KB | Checked before sending; reverse proxies often refuse > 8 KB. |
| `MaxRequestHeadersLength` | int (chars) | 16,384 | ≥ `MaxRequestHeaderLength`, ≤ 256 KB | All headers together; IIS refuses > 16 KB. |
| `BrowserRequestStreaming` | bool | `false` | | Blazor WebAssembly only: stream uploads (needs .NET 7+ in the browser, Chromium, HTTP/2). |
| `CallTimeout` | TimeSpan | `00:01:40` (100 s) | positive, or `-00:00:00.001` (infinite) | Until the response headers arrive, upload included; downloads are read afterwards. |

The HTTP connection itself (pooling, `Expect100ContinueTimeout`, proxy, certificates) is configured in code:
`IoCManagerBase.CreateHttpMessageHandler`.

## `DataPortalServerOptions` (server)

| Setting | Type | Default | Range | Notes |
|---|---|---|---|---|
| `MaxFileCount` | int | 50 | 0 – 1,000 | Files per call; 0 disables uploads. |
| `MaxFileLength` | long (bytes) | 5,242,880 (5 MB) | 64 KB – 2 GB | Per file. |
| `MaxUploadLength` | long (bytes) | 262,144,000 (250 MB) | ≥ `MaxFileLength`, ≤ 16 GB | All files of a call. |
| `MaxPayloadLength` | int (bytes) | 4,194,304 (4 MB) | 256 KB – 256 MB | The JSON parameters. |
| `MaxDepth` | int | 32 | 16 – 128 | JSON nesting depth (a DTO graph with child lists and lookups takes ~10). |
| `CopyBufferSize` | int (bytes) | 81,920 | 4 KB – 1 MB | Spooling and streaming buffers. |
| `SpoolDirectory` | string | unset (system temp) | must exist | Where uploads are spooled; relative to the app path. |

Published to clients in the server settings (except `CopyBufferSize` and `SpoolDirectory`). The HTTP endpoint sets
the host's request body limit to `MaxPayloadLength + MaxUploadLength + (MaxFileCount + 1) × 4096`.

## `HttpDataPortalServerOptions` (server, HTTP)

| Setting | Type | Default | Range | Notes |
|---|---|---|---|---|
| `MaxRequestHeaderLength` | int (chars) | 8,192 | 4 KB – 64 KB | Any single data portal header; at least the clients'. |
| `MaxPartHeadersLength` | int (bytes) | 16,384 | 1 KB – 64 KB | Headers of one multipart part (name, file name twice for non-ASCII, type, length). |

Optional: without a registration (e.g. a custom host), the endpoint uses the defaults.

## `PluginsConfiguration`

| Setting | Type | Default | Notes |
|---|---|---|---|
| `Folder` | string | `Plugins` | Folder holding one folder per plugin; relative to the app path unless absolute. |
| `LibraryName` | string | unset | The plugin library file name in every folder (e.g. `Plugin.dll`); unset: named after the folder (`Acme.Invoicing/Acme.Invoicing.dll`). A file name, not a path. |

No plugins folder: no plugins (not an error).

---

## Example `appsettings.json` per deployment

Server:

```json
{
  "DataPortalServerOptions": {
    "MaxFileLength": 20971520,
    "MaxUploadLength": 104857600,
    "SpoolDirectory": "App_Data/upload-spool"
  },
  "PluginsConfiguration": { "Folder": "Plugins" }
}
```

ASP.NET server with a web UI that calls a separate application server:

```json
{
  "DataPortalConnectionConfiguration": { "Transport": "http", "Address": "https://app-server.acme.local/api/dataportal" }
}
```

Thin client (Blazor WebAssembly, `wwwroot/appsettings.json`):

```json
{
  "DataPortalConnectionConfiguration": { "Transport": "http", "Address": "https://api.acme.example/api/dataportal" },
  "HttpDataPortalClientOptions": { "CallTimeout": "00:05:00" }
}
```

Thick client choosing the server at login: usually nothing; optionally a stricter version match:

```json
{
  "DataPortalClientOptions": { "AppVersionMatch": "Exact" }
}
```

## App configuration classes

See [02 §8](02-ioc-manager.md#8-configuration-classes): derive directly from `ConfigurationBase`, override
`InterfaceType`, optionally `ForBuildConfiguration` (a constant), `RootSection`, `Init`, `Validate`.
