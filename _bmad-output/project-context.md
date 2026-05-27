---
project_name: 'Hexalith.Commons'
user_name: 'JeromePiquot'
date: '2026-03-14'
sections_completed: ['technology_stack', 'language_rules', 'framework_rules', 'testing_rules', 'code_quality', 'workflow_rules', 'critical_rules']
status: 'complete'
rule_count: 56
optimized_for_llm: true
---

# Project Context for AI Agents

_This file contains critical rules and patterns that AI agents must follow when implementing code in this project. Focus on unobvious details that agents might otherwise miss._

---

## Technology Stack & Versions

- **.NET 10.0** / **C# latest** — all projects target net10.0, LangVersion=latest
- **Centralized package management** via `Directory.Packages.props` (versions in `Hexalith.Builds` submodule)
- **CompareNETObjects 4.84.0** — deep object comparison
- **FluentValidation 12.1.1** — settings/input validation
- **Humanizer 3.0.10** — string/enum formatting
- **Hexalith.PolymorphicSerializations** — polymorphic JSON serialization (internal)
- **Microsoft.Extensions.* 10.0.5** — logging, options, configuration, DI
- **XUnit 2.9.3 + Shouldly 4.3.0** — testing and assertions
- **coverlet 8.0.0 + Microsoft.NET.Test.Sdk 18.3.0** — test runner and coverage
- **Analyzers (all projects):** SonarAnalyzer.CSharp 10.21.0, StyleCop 1.2.0-beta.556, Roslynator 4.15.0, Microsoft.VisualStudio.Threading.Analyzers 17.14.15
- **Build settings:** Nullable=enable, ImplicitUsings=enable, GenerateDocumentationFile=true, EnforceCodeStyleInBuild=true
- **Semantic-release CI/CD** — automated versioning from conventional commits

## Critical Implementation Rules

### C# Language-Specific Rules

- **Nullable reference types enabled** — all code must handle nullability; use `ArgumentNullException.ThrowIfNull()` and `ArgumentException.ThrowIfNullOrWhiteSpace()` for validation
- **Primary constructors** — use on records and classes when possible
- **Records for data models** — use `record` for immutable data (ApplicationError, Metadata, ConditionalValue); use `class` for stateful helpers/utilities
- **DataContract/DataMember attributes** — required on serializable records alongside `JsonPropertyOrder` and `JsonConstructor`
- **ValueOrError<T> result pattern** — use for expected failures instead of throwing exceptions; exceptions reserved for truly exceptional cases
- **ErrorCategory enum** — separate `Business` (user-facing) from `Technical` (internal) errors
- **Static helper classes** — primary extension mechanism (StringHelper, ReflectionHelper, EquatableHelper); avoid class hierarchies
- **MIT license copyright header** — every .cs file must start with `// <copyright ... ITANEO ... MIT License`
- **XML documentation mandatory** — all public/protected/internal members; records use `<param>` tags for primary constructor parameters
- **SuppressMessage with justification** — when suppressing analyzer warnings, always include a reason string
- **Namespace = folder path** — `Hexalith.Commons.{Feature}` matching directory structure; no deep nesting
- **One class/interface per file** — file name matches type name exactly

### Framework-Specific Rules (Hexalith Ecosystem)

- **Hexalith.Builds submodule** — never add package versions to project-level `.csproj` files; all versions go in centralized `Directory.Packages.props`
- **InternalsVisibleTo for tests** — defined in `src/libraries/Directory.Build.props` via `AssemblyAttribute`, not per-project
- **Hexalith.PolymorphicSerializations** — use for types requiring polymorphic JSON serialization (e.g., Metadata records)
- **ISettings interface** — all configuration classes implement `ISettings` with `ConfigurationName` static property for Options pattern binding
- **FluentValidateOptions<T>** — use for validating configuration at startup via `IValidateOptions<T>`
- **IEquatableObject interface** — implement for domain objects needing custom equality beyond default record equality
- **IIdempotent interface** — implement for operations requiring idempotency keys

### Testing Rules

- **XUnit + Shouldly** — use Shouldly assertions (`ShouldBe`, `ShouldNotBeNull`, `ShouldThrow`), never raw `Assert.*`
- **PascalCase test method names** — descriptive: `LargeFibonacciNumberShouldBeCalculatedCorrectly`, not snake_case
- **[Theory] + [InlineData]** — use for parameterized tests with multiple inputs
- **Arrange-Act-Assert** — consistent pattern across all tests
- **Test organization mirrors source** — `test/Hexalith.Commons.Tests/{Feature}/` matches `src/libraries/Hexalith.Commons/{Feature}/`
- **Test file naming** — `{ClassName}Test.cs` (singular, no "s"), e.g., `FibonacciTest.cs`, `MetadataTest.cs`
- **Global using** — `<Using Include="Xunit" />` in test `.csproj`, no per-file `using Xunit;`
- **Test both success and failure paths** — verify exceptions with `Should.Throw<T>()`, verify valid outputs with Shouldly fluent assertions
- **Dummy/fixture classes** — place test data helpers (e.g., `DummyEquatable.cs`) alongside test files in the same feature folder

### Code Quality & Style Rules

- **5 analyzers enforced on every build** — SonarAnalyzer, StyleCop, Roslynator, Roslynator.Formatting, Microsoft.VisualStudio.Threading
- **EnforceCodeStyleInBuild=true** — style violations fail the build
- **AnalysisLevel=preview-all** — strictest analysis level enabled
- **Suppressed StyleCop rules** — SA1609 (property docs), SA1309 (field prefix), SA1101 (this. prefix), SA1009 (closing paren spacing), SA1111 (closing paren positioning), SA1502 (element on single line), SA1518 (trailing blank lines)
- **Naming conventions** — PascalCase for classes/methods/properties, `I` prefix for interfaces, no `_` prefix for fields (SA1309 suppressed)
- **Interfaces prefixed with I** — `ISettings`, `IEquatableObject`, `IMappableType`
- **No trailing blank lines** — SA1518 suppressed but enforced via Roslynator.Formatting
- **sealed when possible** — use `sealed record` and `sealed class` for types not designed for inheritance

### Development Workflow Rules

- **Angular Conventional Commits** — all commits must follow `<type>(<scope>): <description>` format for semantic-release
- **Commit types with version consequences:**
  - `feat` → minor version bump (new public API only, not refactors)
  - `fix` → patch version bump (bug fixes only)
  - `refactor`/`test`/`docs`/`style`/`chore`/`build`/`ci` → no version bump
  - `BREAKING CHANGE:` in footer → major version bump (use when removing/renaming public API)
- **Imperative mood, lowercase, no period** — e.g., `feat(errors): add ValueOrError result type`
- **Short description under 50 chars** (including `type(scope): ` prefix); body wrapped at 72 chars
- **Always verify before committing** — run `dotnet build && dotnet test`; no green, no commit
- **CI/CD pipeline** — GitHub Actions on main/next/alpha/beta branches; uses `Hexalith/Hexalith.Builds/Github/package-release@main`; NuGet publishing is fully automated on merge
- **Hexalith.Builds submodule** — NEVER modify submodule files without explicit user approval; changes propagate to ALL Hexalith repos; if modified, commit inside submodule first then update parent ref
- **No direct commits to main** — always use feature branches and PRs

### Critical Don't-Miss Rules

- **Never add package versions in `.csproj`** — all versions must be in centralized `Directory.Packages.props`; `.csproj` files use `<PackageReference Include="..." />` without `Version`
- **Never modify `Hexalith.Builds` submodule without asking** — it's shared across 8+ repos; a bad change breaks the entire ecosystem
- **Never use `feat` for refactors** — it triggers a minor version bump and NuGet publish; use `refactor` type instead
- **Never skip XML documentation** — `GenerateDocumentationFile=true` is enforced; missing docs will cause warnings treated as build noise
- **Never use `Assert.*` in tests** — always use Shouldly (`ShouldBe`, `ShouldNotBeNull`, etc.)
- **Never create nested namespaces** — keep flat: `Hexalith.Commons.{Feature}`, not `Hexalith.Commons.{Feature}.{Sub}`
- **Always add copyright header** — MIT license with ITANEO attribution on every `.cs` file
- **Always use `sealed`** — unless the type is explicitly designed for inheritance
- **Always validate at boundaries** — use `ArgumentNullException.ThrowIfNull()` and `ArgumentException.ThrowIfNullOrWhiteSpace()` for public method parameters
- **Error handling** — use `ValueOrError<T>` for expected business failures; throw exceptions only for truly exceptional/programmer errors

---

## Usage Guidelines

**For AI Agents:**

- Read this file before implementing any code
- Follow ALL rules exactly as documented
- When in doubt, prefer the more restrictive option
- Update this file if new patterns emerge

**For Humans:**

- Keep this file lean and focused on agent needs
- Update when technology stack changes
- Review quarterly for outdated rules
- Remove rules that become obvious over time

Last Updated: 2026-03-14
