# CARMA

**C**lean **A**rchitecture **R**ich **M**odel **A**pplication — a .NET Standard 2.1 framework for building
line-of-business applications around a rich domain model rather than anaemic data structures.

```
A5Soft.CARMA.Domain        1.0.0
A5Soft.CARMA.Application   1.0.0
```

CARMA is CSLA-influenced: domain objects track their own state, validate themselves against rules declared
as attributes, and raise change notifications suitable for direct GUI data binding. On top of that sits a
use case layer that is the only sanctioned entry point into application logic, and a data portal that lets
the same use case run in-process or across a network without conditional code of your own.

---

## The two assemblies

### `A5Soft.CARMA.Domain`

The business model.

| Concept | Type |
|---|---|
| Object with an identity, one of many | `DomainEntity<T>` — carries `DomainEntityIdentity<T>` |
| Object with no identity — a singleton, or an editable container | `DomainObject<T>` |
| Child collection with add/remove/delete tracking | `DomainBindingList<TChild>` |
| Read-only reference for dropdowns | `LookupBase<TEntity>` |
| Plain bindable object, no rules or state tracking | `BindableBase` |

Around those:

- **State tracking** — `IsNew`, `IsDirty`, `IsSelfDirty`, `IsValid`, `IsSavable`, aggregated automatically
  across a parent/child graph.
- **Validation** — rules are `ValidationAttribute` descendants (`PropertyRuleAttributeBase`,
  `EntityRuleAttributeBase`), discovered by reflection from the type *or its interfaces*, and re-run
  automatically when the properties they depend on change. `RuleSeverity` distinguishes errors from
  warnings and information.
- **Metadata** — `IMetadataProvider` exposes display names, descriptions and property information, driven
  by `[Display]` and `[ClassDescription]` and resolved through a `ResourceManager` for localization.
  Providers are replaceable.
- **Change notification** — `INotifyPropertyChanged` plus a `ChildChanged` event that bubbles up an
  aggregate, so a root can react to an edit several levels down.
- **Suspension** — `SuspendBindings()` and `SuspendValidation()` return `IDisposable` scopes for bulk
  population.

### `A5Soft.CARMA.Application`

The application layer.

| Concept | Type |
|---|---|
| Use case base classes | `UseCaseBase` → `RemoteUseCaseBase` → `AuthorizedUseCaseBase`, and the Create/Fetch/Save/Query/Command family |
| Local or remote execution | `IClientDataPortal`, `IDataPortalProxy`, `ServerDataPortalBase` |
| Authorization | `IUseCaseAuthorizer`, `AuthorizationBaseAttribute` |
| Lookup access control and caching | `GenericLookupService`, `LookupServiceBase<T>` |
| DI registration by attribute | `[Service]`, `[UseCase]`, `[DefaultServiceImplementation]`, `IoC.GetApplicationServices` |
| Configuration | `ConfigurationBase`, `IAppConfigurationProvider` |
| Retry | `IRetryPolicy` and several implementations, `TransiantException` |
| Navigation | `MenuItem`, `FetchMainMenuUseCaseBase` |
| Health checks | `[WithHealthCheck]` |

---

## Two ideas worth understanding first

### The data portal

A use case's body does not know whether it is running on the user's machine or a server. `RemoteUseCaseBase`
holds an `IClientDataPortal`; where `IsRemote` is false — the default, via `LocalDataPortalProxy` — the call
runs in-process. Configure a real proxy and the identical use case executes on a server, with the request
carrying the caller's claims **and their culture**, so server-side messages come back localized correctly.

In practice this means a Blazor Server application can become a WebAssembly client by configuration rather
than by rewriting the application layer — provided portal calls stay inside your use case base classes.

> **Note:** the current portal implementation is being reworked. Treat `IClientDataPortal` as an interface
> contract rather than settled behaviour, and keep portal calls confined to base classes.

### Localization shapes the framework's boundaries

CARMA cannot reach your application's resource assembly. Anything needing a localized string is therefore
deliberately left abstract for the application to complete:

- The `CommonRules` validation attributes ship **abstract**, with an abstract message hook.
- `AuthorizationBaseAttribute` is abstract — the permission model and its messages are yours.
- There is no built-in `ValidationException`, `ConcurrencyException` or auditing base class.

This is the single most surprising thing about the framework on first contact, and it explains most of the
"why isn't this in the box?" questions. Some of these are expected to move into the framework in future
releases.

---

## Getting started

```
YourCo.App.Localization    ResourceManager + resx only
YourCo.App.Domain          → A5Soft.CARMA.Domain, → Localization
YourCo.App.Application     → A5Soft.CARMA.Application, → Domain, → Localization
YourCo.App.Domain.Test
```

A minimal entity:

```csharp
[ClassDescription(ResourceType = typeof(Resources),
    NameForNew = nameof(Resources.Domain_Parties_Customer_NameForNew),
    NameForOld = nameof(Resources.Domain_Parties_Customer_NameForOld))]
public class Customer : DomainEntity<Customer>
{
    private string _name = string.Empty;

    public Customer(IValidationEngineProvider validationEngineProvider)
        : base(validationEngineProvider) { }

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.Domain_Parties_Customer_Name_Name),
        ShortName = nameof(Resources.Domain_Parties_Customer_Name_ShortName),
        Description = nameof(Resources.Domain_Parties_Customer_Name_Description),
        Prompt = nameof(Resources.Domain_Parties_Customer_Name_Prompt))]
    [ValueRequired, StringMaxLength(200)]
    public string Name
    {
        get => _name;
        set => SetPropertyValue(nameof(Name), ref _name, value);
    }
}
```

`SetPropertyValue` is the load-bearing call: it raises `PropertyChanging`/`PropertyChanged`, marks the object
dirty and re-runs the property's rules. Assigning `_name` directly compiles and silently disables all three.

A use case exposing it:

```csharp
[UseCase]
[Authorization(typeof(ViewCustomersPermission))]
public interface IFetchCustomerUseCase : IAuthorizedUseCase
{
    [RemoteMethod]
    Task<Customer> FetchAsync(DomainEntityIdentity<Customer> id);
}
```

Exactly one `[RemoteMethod]` per interface: the server resolves the method by taking the first match, so a
second one makes dispatch non-deterministic.

Register with `IoC.GetApplicationServices(BuildConfiguration.Client)` or `.Server`, and feed the returned
list into your container.

---

## Things that will bite you

Collected from real use. Each compiles cleanly and fails silently.

| | |
|---|---|
| Assigning a backing field directly outside a constructor | dirty tracking and validation stop for that property |
| A computed property as `=> _lines.Sum(...)` | never notifies, and re-runs on every getter call — GUI frameworks call getters many times per render |
| `PropertyHasChanged` in a constructor | it is `MarkDirty(true)` + rule checks, so it dirties a brand-new object |
| `[ChildField]` on the property instead of the field | the argument is the *property* name; `SetChildPropertyValue` wants the *field* name |
| `SuspendValidation` assumed to cascade | it is a per-instance flag and never re-validates on dispose |
| `[Required]` on a decimal | means **non-zero**, rounded to `SignificantDigits` (default 2) — not a null check |
| `[Required]` on a non-nullable `DateTime`/`bool`/`enum` | can never fail; silently dead |
| `GetErrorDescription` | the framework spells it **`GetErrorDescripton`**; the correct spelling never runs |
| `TransientException` | the class is **`TransiantException`** — but `ITransientExceptionValidator` *is* spelled correctly |
| `[GreaterThan(0)]` | `GreaterThan`/`LessThan`/`EqualTo` compare against another **property**; use `MinValue`/`MaxValue` for a constant |
| An app rule attribute named `Required` or `MaxLength` | collides with `System.ComponentModel.DataAnnotations`, which every entity file imports for `[Display]` |
| A missing `[Display]` | falls back to `Name.SplitCamelCase()` — plausible English that never localizes |

---

## Documentation

This README covers the framework. Application-level conventions — auditing, DTOs, `Merge`, repositories,
permissions, project layout, localization keys — are documented in the **`carma-framework` Claude skill**,
which also carries the boilerplate every application repeats as ready-to-copy templates.

---

## A note on this file

Written from reading the CARMA 1.0.0 source rather than from official documentation. The domain-layer claims
were verified by compiling against a real build; the application-layer ones were read from source but not
compiled. Corrections welcome.
