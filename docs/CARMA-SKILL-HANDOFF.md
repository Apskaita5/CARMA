# CARMA skill — handoff summary

Import this alongside `carma-framework.skill` to continue improving the skill in a new chat.

---

## 1. What the skill is and how it got here

`carma-framework.skill` is a Claude skill documenting the CARMA framework (`A5Soft.CARMA.Domain 1.0.0`,
`A5Soft.CARMA.Application 1.0.0`) **plus the app-level conventions layered on it**. It was built from a
1,159-line merged guide and then corrected over roughly fifteen rounds of review against the framework
source and the maintainer's own sample code.

**Final shape — 42 files:**

```
SKILL.md                              routing, quick recipes, ~30 silent-failure gotchas
references/generating-an-app.md       step-by-step procedure from a domain description
references/domain-model.md            framework mechanics
references/app-conventions.md         project layout, DTOs, Merge, exceptions
references/validation-metadata-lookups.md
references/application-layer.md       use cases, services, queries, commands
references/data-portal-authorization.md
references/testing.md                 reflection-based convention tests
templates/  (34 .cs + README)         boilerplate identical in every app, {App} placeholders
```

**Verification status:**

| | Status |
|---|---|
| `templates/Domain.*` | **compiled clean** against a real CARMA.Domain 1.0.0 build from source |
| Domain half of two independently generated apps | **compiled clean** |
| `templates/Application.Internals` | **never compiled** — CARMA.Application needs Castle.Core + Newtonsoft.Json, unreachable in the build environment |
| Application layer of both test apps | **never compiled** |

Compilation was done with mono `mcs` against CARMA compiled from source, because no .NET SDK was
available and `nuget.org` / `dot.net` were blocked. Mono has a bug losing named tuple element names when
the tuple returns from a generic method — this blocked CARMA.Application, and is **not** a defect in
CARMA or in the app code. Roslyn handles it.

---

## 2. Every convention specified by the maintainer

Recorded verbatim in intent, so a future session can check the skill still says all of it.

### Domain model

- **`DomainEntity<T>`** = one of many, identified by `DomainEntityIdentity<TEntity>` (invoice, invoice
  line). **`DomainObject<T>`** = a domain singleton (app-scope profile, settings) *or* a plain rich object
  with no id (query criteria needing field validation, command parameter backing a dialog). The test is
  simply "does it have an id" — **not** whether it is embedded in a parent.
- **Calculated fields are cached in a private backing field.** The getter returns the field and does
  nothing else. Recalculation happens in a constructor or the relevant change handler, and raises
  `PropertyHasChanged` **only when the value actually changed**. Rationale: entities are bound to WinForms
  and Blazor, which call getters many times per render, so a computed getter re-runs its calculation on
  every one. **Applies to every calculated field with no exceptions**, including trivial ones like
  `quantity * unitPrice`.
- **Never call `PropertyHasChanged` from a constructor.** Constructors assign the cache directly.
- **Prefer the property setter's `bool` return value** to trigger recalculation
  (`if (SetPropertyValue(...)) Recalculate();`) over the `postProcessing` delegate.
- **Rounding order applies to comparisons, not just storage.** Every setting and comparison uses the
  rounding order appropriate to that property.
- **`Merge` disables events**: `using (SuspendBindings()) using (SuspendValidation())`. **Child Merge must
  suspend for itself** — suspension does not cascade.
- **On merge, all calculated fields must be recalculated.** How is left to the entity: a complex entity
  legitimately has several recalculation methods driven by different property changes, and collapsing them
  into one `RecalculateAll()` creates its own synchronisation problem. *(A proposed `RecalculateAll()`
  convention was explicitly rejected.)*
- **Criteria and command parameters are not exempt from `Merge`.** Incoming instances are untrusted:
  create a new one, merge the incoming into it, use only the trusted one.
- **`AddRange`, not `Add`**, when filling a `DomainBindingList` from a DTO.
- **DTOs carry `IsNew` and `IsDirty`** (`IsSelfDirty` is the `DomainObject` counterpart; `IsNew` applies
  only to entities). These belong on the base DTO classes.
- **Project the child list's deleted items** onto a second DTO property (`DeletedLines` or similar) from
  `DomainBindingList.DeletedList`.
- **Domain entities and objects are not serializable** and carry no `[Serializable]`. **DTOs are.**
- **Regions** around fields, constructors, properties, methods and the DTO.
- **One type per file**, unless nested.
- **XML documentation** on all public and protected properties, methods, classes and interfaces.
- **The `IsNull()` extension is deprecated** — use `null == value`.

### Localization

- **The app is always localized.** A separate project `[BaseNamespace].[AppName].Localization` holds
  nothing but a standard `ResourceManager` and the resx files. **Default English, secondary Lithuanian**
  unless specified otherwise.
- **`[Display]` on every property that can reasonably be shown in a GUI**, with `Description`, `Name`,
  `ShortName` and `Prompt`. **`Prompt` is omitted for read-only and `bool` properties.** Plain ids and
  technical properties need no `[Display]` at all.
- **A missing `[Display]` is a minor UX issue, not critical** — it degrades to camel-case English. Worth a
  warning-level convention test, not a build failure.
- **Resource key convention**: type namespace minus base namespace minus app name, `.` → `_`, then
  `_{PropertyName}_{Member}`. *Stick to the convention; discrepancies in the maintainer's own samples are
  to be ignored.*
- **`ClassDescriptionAttribute`** is the correct name (the maintainer referred to it as
  `DomainClassDescription` several times; `ClassDescription` was confirmed correct).
- **Query results, criteria and command parameters need `NameForNew` only** — they are always new.
- **Lookup properties** carry `[Display]` with `Description`, `Name`, `ShortName` only, **reusing the
  corresponding entity's resource keys** rather than creating separate entries.
- **Attributes go on the class by default**; only define an entity interface and attribute that when
  multiple implementations are genuinely expected. (`IAuditable` is the case where it earns its place.)

### Validation rules

- Rules **are attributes** in 1.0.0. `CommonRules` ship as abstract `*AttributeBase` types with an abstract
  localized-message hook; the app subclasses each one it uses.
- **Rename where the name collides with `System.ComponentModel.DataAnnotations`** (`ValueRequired`, not
  `Required`), since every entity file imports it for `[Display]`.
- A rule serving one entity only is a **private nested class** inside that entity.
- **`ValueRequired` is meaningless for value types except numeric ones**, where "has a value" means
  non-zero.

### Exceptions and auditing

- **`BusinessException`** is the base for exceptions only a human can resolve — the user retypes input, or
  decides whose edit wins. Infrastructure faults are not business exceptions.
- **`ValidationException`, `ConcurrencyException`, `NotFoundException`** are defined at the **domain root**,
  app-level, because their messages need localizing. Further business exceptions only where the app's logic
  genuinely needs them — sparingly.
- **The auditable base classes** (`IAuditable`, `IAuditableEntity`, `AuditableDomainEntity`,
  `AuditableDomainSingleton`, and the DTO bases) are implemented per app and used by inheritance. CARMA's
  `DomainEntity` is used directly only when no auditing is required — typically child entities.
- **`HideAuditTrace()` is called by the fetch use case**, which has access to the identity and can check
  the permission.
- **Repositories take the user's email** for the audit log — enough to identify a person if needed.

### Application layer

- **Use case** = invoked by a user (API call, GUI action); authorized unless deliberately public; stateless;
  everything injected; **usually transient**. **Service** = never invoked by a user, only by a use case,
  event handler or cron job; stateless; **usually singleton**, or scoped when it needs current-user context
  (e.g. selected tenant).
- **All use cases and services are interface + implementation**, so any can be replaced at startup.
- **A use case interface has exactly one remote method**, marked `[RemoteMethod]`. Local helper methods are
  fine (metadata, filtered lookup lists).
- **`[UseCase]` derives from `Service`** — do not duplicate the attribute.
- **`[UseCaseDescription]`** on the interface for localization.
- **`[Authorization]`** on the interface. Permission granularity follows business logic — a read-only query
  view and CRUD are usually different permissions.
- **Authorization and exception logging live in the base use case class, following the `SaveUseCaseBase`
  pattern** exactly (log entry, portal branch, `CanInvokeAsync` before local work, log-and-rethrow, log exit).
- **Do not inherit CARMA's use case bases.** They are built around entity interfaces, which is
  overengineering for most entities. Define the app's own bases, using the framework's as reference.
- **The data portal invoke stays in the base class**, even in an app that does not use it today.
- **CRUD per entity; fetch + update only for singletons.**
- **Do not mix create and update.** Save is fine as a base class, not as an actual use case.
- **Lookups** go through `ILookupService` (`GenericLookupService` base), which checks that the invoking use
  case requires that lookup type and that the user is authorized for it, and handles caching and the portal
  hop. The use case refines the filtering. **Return `IReadOnlyCollection`** so a consumer cannot mutate the
  shared cached list.
- **Lookup repository interfaces**: `ILookupRepository`, `ICutomLookupRepository` (marker, injected as a
  collection), `ICutomLookupRepository<TLookup, TEntity>`. *(Maintainer's spelling of "Cutom" retained.)*
- **Query result**: POCO with auto-implemented properties and **protected setters** (so only the DAL's ORM
  can populate them by inheritance); **protected empty constructor** for the ORM; **private constructor
  listing all properties** for serialization; `Match(string)` for search; **virtual `CalculateSubtotals`**
  on the base so a repository can call it abstractly; other helpers such as `ApplyXCommand(parameter)` to
  avoid refetching; entity-based results always carry `Id`; `[Display]` with Description/Name/ShortName
  only; `[ClassDescription]`; `[Serializable]`. Base/abstract query results at app level are expected.
- **Query criteria are always a `DomainObject`** — validation of criteria fields plus DTO serialization.
- **Command parameters**: a plain POCO when never shown to the user (a delete taking just
  `DomainEntityIdentity<TEntity>`); a `DomainObject` when the user previews or edits it. A command may
  return a result (e.g. a query result summarising the work) or nothing.
- **Repositories never accept or produce entities — only DTOs.** No business logic. They may run on
  another server, so every parameter and return value must be serializable (POCO or private serialization
  constructor). They may also take/return query results, lookups and POCOs.
- **`ICacheProvider`** — the maintainer's standard interface, keyed by type. Reuse it.
- **`TransiantException`** is thrown **only by repositories and infrastructure services**; it means a
  server is unavailable. Application-level services (e.g. a cron job) may catch it to apply a more
  aggressive retry policy.
- **Target framework is .NET Standard 2.1** for all non-GUI projects.
- **Project structure**: `Internals/` (default; `Core/` is an accepted synonym) for base classes;
  `UseCases/`, `Services/`, `Repositories/` organised by entity; `Infrastructure/` and `Repositories/`
  contain **interfaces only** (plus parameter classes such as `EmailMessage`).
- **Background work** — a use case raises an event and completes; a separate event raising/handling service
  does the work. **There is no framework standard; implement per app.**

### Data portal — important caveat

**The current implementation is invalid; treat `IClientDataPortal` as an interface contract only.** The
intended future design moves DTOs and reinstantiates on the far side: the conventional constructor for
`DomainObject` implementations, the private serialization constructor for lookups and query results, and
plain POCO serialization otherwise. Consequence encoded in the skill: **a portal call never appears in a
concrete use case, only in an app-level base class.**

### Testing

- Reflection-based convention tests are wanted, in `{App}.Domain.Test`.
- **Permission `Id` uniqueness** should be asserted; **`Order` uniqueness should not** — display order is
  handled in UX tests, so collisions are acceptable until then.
- DTO round-trip testing is valuable but must be **reflection-driven**, or it needs updating whenever
  properties change.

### Explicitly rejected or deferred

- `RecalculateAll()` as a general convention — too complicated for entities with several recalculation
  paths.
- A `dotnet new` item template — skipped for now.
- The maintainer's own uploaded samples are **not perfect**; where they conflict with a stated convention,
  the convention wins.

---

## 3. Corrections the review process produced

Worth knowing, because they show where the original guide was wrong and what an audit should look for.

**Wrong-version content.** The original guide documented an unfinished rules redesign
(`PropertyValidationRuleBase<TMetadata>`, `IValidationRule`, `EntityRulesMetadata`,
`EntityValidationAttribute`) that does **not** exist in 1.0.0. The whole rules architecture was rewritten.

**Framework spelling traps.** `TransiantException` and `IsTransiantException()`/`ToTransiantException()`
are misspelled, but `ITransientExceptionValidator` is spelled correctly, in the same namespace. The class
inside `JObjectExtensions.cs` is `JObjectExtension`, singular. The rule override is `GetErrorDescripton`
— spelling it correctly compiles as an unrelated method that never runs.

**API facts found only by compiling.** `MaxLengthAttributeBase` exposes `MaxLength` (a `uint`), not
`MaximumLength`. `MinValueAttributeBase` splits into `ExpectedIntValue` / `MinIntValue` / `MinDblValue` /
`Digits`. `IsNullOrWhiteSpace` already exists in CARMA — redeclaring it makes every call ambiguous.
`IsNew` is on `IPersisted`, not `ITrackState`. `LookupBase<T>` declares **no constructors** (assign the
protected `_id` field) and has an **abstract `Match`**. `UseCaseAttribute`'s `Type[]` parameter is the
**required lookups**, not the entity type. `IClientDataPortal` has no `SaveAsync`/`ExecuteAsync` — saves go
through `FetchAsync<TArg, TResult>`, which returns `(TResult Result, ClaimsIdentity Identity)`. The
protected member is `ValidationProvider`, not `ValidationEngineProvider`.

**Mechanical reasons behind conventions.** `GetRemoteMethod()` takes the *first* `[RemoteMethod]` match, so
a second one makes dispatch non-deterministic rather than failing — that is why one remote method per
interface is a correctness rule. `SuspendValidation` clears a **per-instance** flag and never re-validates
on dispose, which is why children must suspend for themselves and why `CheckRules()` must be explicit.
`OnChildChanged` *is* raised for per-item edits — the `ItemChanged` filter is deduplication, not
suppression.

**The final regeneration test** (rebuilding the sample app from the skill under different names) found:
SKILL.md's quick-recipes block still contradicted the reference files in four places; the skill named
boilerplate it never provided (fixed by adding `templates/`); and three use case base classes were missing
(`CreateEntityUseCaseBase`, `CommandWithResultUseCaseBase`, `FetchSingletonUseCaseBase`) — each surfaced
only when the corresponding use case was written, which is why partial regeneration hides gaps.

---

## 4. Open items

1. **The application layer has never been compiled.** Its API surface was read from source, not verified.
   The first real `dotnet build` will likely produce a short list of signature corrections.
2. **Localization key drift on rename.** Renaming a class or property silently orphans its resource keys.
   The skill has detection (assert every referenced key exists, and every resx key is referenced) but no
   solution. The maintainer intends to address this when developing the framework further.
3. **Types expected to move into the framework eventually** — the DTO bases, query result bases,
   `Permission`, `AuthorizationAttribute`, the auditable bases. Until then they are reimplemented per app,
   which is what `templates/` is for. When they move, `templates/` shrinks and the reference files need
   rewording from "the app defines" to "the framework provides".
4. **No `DownloadFile`/`UploadFile` or unauthenticated use case example** in either test app.
5. **No DAL** — every repository is an interface. Persistence is out of scope for the skill.

---

## 5. How to continue in a new chat

1. Attach `carma-framework.skill` and this summary.
2. State what changed — a new convention, a framework version, or a correction.
3. For anything touching the framework's API, **verify against source before writing**. The single biggest
   source of error in this project was documenting a plausible API rather than a real one. If CARMA source
   is available, upload it.
4. After a substantive change, **regenerate a small app from the skill and compile the domain half**. That
   is what caught the missing base classes and the contradictory recipes; reading the skill does not.
5. Keep `SKILL.md` and the reference files reconciled. SKILL.md is always loaded, so a contradiction there
   outranks the reference file in practice — this happened once and went unnoticed for several rounds.
