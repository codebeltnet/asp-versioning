# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For more details, please refer to `PackageReleaseNotes.txt` on a per assembly basis in the `.nuget` folder.

> [!NOTE]  
> Changelog entries prior to version 8.4.0 was migrated from previous versions of Cuemon.Extensions.Asp.Versioning.

## [10.2.2] - 2026-07-24

This is a patch release focused on dependency updates, build toolchain improvements, and documentation quality. The release upgrades core development dependencies to their latest stable patches, enables recommended .NET code analyzers with code style enforcement, configures consistent git tag naming, and refines XML documentation.

### Changed

- Dependencies upgraded to latest stable patches for all supported target frameworks: Codebelt.Extensions.AspNetCore.Mvc.Formatters.Newtonsoft.Json (10.1.5 → 10.1.6), Codebelt.Extensions.AspNetCore.Mvc.Formatters.Text.Yaml (10.1.5 → 10.1.6), Codebelt.Extensions.Xunit.App (11.1.1 → 11.1.2), Cuemon.AspNetCore and related packages (10.5.4 → 10.5.5), and Microsoft.NET.Test.Sdk (18.7.0 → 18.8.1),
- Build configuration now enables recommended .NET code analyzers with latest analysis level and enforces code style compliance during build,
- DocFX Dockerfile base image updated from nginx 1.31.2-alpine to nginx 1.31-alpine,
- MinVerTagPrefix configured to `v` for consistent semantic version tag naming.

### Fixed

- XML documentation improved in `RestfulApiVersionReader` with simplified and modernized parameter documentation using modern type references.

## [10.2.1] - 2026-07-09

This is a minor release focused on normalizing semantically equivalent API version formats to ensure consistent routing and version matching. Version strings like 1, 1.0, and 1.0.0 are now normalized to identical canonical forms, with automatic enabling when using `SemanticApiVersion` as the default API version.

### Changed

- `RestfulApiVersioningOptions` includes a new `ApiVersionReader` property for advanced version reader customization,
- Automatic version normalization is enabled by default when the default API version is a `SemanticApiVersion`; otherwise it retains its previous behavior.

### Fixed

- `RestfulApiVersionReader` now normalizes semantically equivalent API version formats (1, 1.0, 1.0.0) to identical values for consistent routing and version matching.

## [10.2.0] - 2026-07-07

This is a minor release adding API version aliasing support and automatic semantic version alias registration. Callers can now request versions by shortened or compatibility-oriented tokens (e.g., `1`, `1.0`, `1.0.0`), and applications using `SemanticApiVersion` as their default API version benefit from automatic alias registration without explicit configuration.

### Added

- `ApiVersionAliasParser` type for resolving friendly API version aliases before delegating to another `IApiVersionParser`, enabling callers to request versions by shortened or compatibility-oriented tokens,
- `AddApiVersionParser<T>` extension method for registering custom `IApiVersionParser` implementations as the singleton parser used by API versioning services.

### Changed

- `AddRestfulApiVersioning` now automatically registers semantic version aliases when the default API version is a `SemanticApiVersion`, eliminating boilerplate configuration for common semantic versioning scenarios,
- Documentation updated to clarify semantic version alias registration behavior and guidance for exposing additional semantic versions through custom parsers.

## [10.1.0] - 2026-07-03

This is a minor release introducing Semantic Versioning support for APIs that require major.minor.patch versioning with pre-release and build metadata beyond standard Asp.Versioning.ApiVersion. The release also includes dependency service updates and enhanced CI/CD safety.

### Added

- `SemanticApiVersion`, `SemanticApiVersionParser`, `SemanticApiVersionFormatter`, `SemanticApiVersionAttribute`, and `MapToSemanticApiVersionAttribute` types for APIs requiring Semantic Versioning compliance including full build-metadata awareness and pre-release support,
- `VersionConversionOptions` to control how .NET four-part `Version` objects are translated into `SemanticApiVersion` instances,
- Comprehensive DocFX documentation for all semantic versioning types with usage examples,
- Full test coverage for semantic version parsing, formatting, comparison, and integration within the API explorer.

### Changed

- Dependencies upgraded to the latest compatible versions: `Codebelt.Extensions.AspNetCore.Mvc.Formatters.Newtonsoft.Json` to 10.1.5, `Codebelt.Extensions.AspNetCore.Mvc.Formatters.Text.Yaml` to 10.1.5, `Codebelt.Extensions.Xunit.App` to 11.1.1, Cuemon packages to 10.5.4, and `Microsoft.NET.Test.Sdk` to 18.7.0.

## [10.0.9] - 2026-07-01

This is a patch release focused on package dependency upgrades, comprehensive public API documentation,
and CI/CD pipeline refinements to strengthen deployment safety and maintainability.

### Added

- DocFX overwrite files for all four public non-abstraction types (`ApplicationBuilderExtensions`, `RestfulApiVersionReader`, `RestfulApiVersioningOptions`, `ServiceCollectionExtensions`) in the Codebelt.Extensions.Asp.Versioning namespace, including usage examples and availability notes,
- `dotnet-docfx-digest` maintenance block in `AGENTS.md` to persist documentation rules for the public API surface.

### Changed

- Dependencies upgraded to the latest compatible versions: `Codebelt.Extensions.AspNetCore.Mvc.Formatters.Newtonsoft.Json` to 10.1.5, `Codebelt.Extensions.AspNetCore.Mvc.Formatters.Text.Yaml` to 10.1.5, `Codebelt.Extensions.Xunit.App` to 11.1.1, Cuemon packages to 10.5.4, and `Microsoft.NET.Test.Sdk` to 18.7.0,
- DocFX Dockerfile base image advanced to nginx 1.31.2-alpine for latest security and stability fixes,
- `docfx.json` configuration split so namespace and type overwrite files are scoped under `build.overwrite` while remaining excluded from `build.content`,
- CI/CD pipeline deployment condition tightened to require the build, pack, test-quality-gate, SonarCloud, Codecov, and CodeQL jobs to succeed before the NuGet deploy step runs, ensuring a skipped optional job can no longer suppress deployment,
- Namespace overview documentation rewritten to lead with the developer problem and provide start-here guidance.

## [10.0.8] - 2026-06-06

This is a service update that focuses on package dependencies.

## [10.0.7] - 2026-05-23

This is a service update that focuses on package dependencies and explicit dual-framework support through conditionally-targeted Asp.Versioning package versions, ensuring compatibility with .NET 9 (Asp.Versioning 8.1.1) and .NET 10 (Asp.Versioning 10.0.0).

### Changed

- `Directory.Packages.props` to conditionally target Asp.Versioning package versions: version 8.1.1 for .NET 9 target framework and version 10.0.0 for .NET 10 target framework, providing version-appropriate behavior for each framework,
- Dependencies upgraded to the latest compatible versions for all supported target frameworks (.NET 10 and .NET 9).

## [10.0.6] - 2026-04-18

This is a service update that focuses on package dependencies.

## [10.0.5] - 2026-03-28

This is a patch release that upgrades package dependencies and delivers several ALM refinements: a cleaner `PackageReleaseNotes.txt` generation pipeline, improved `Directory.Build.targets` file reading, an expanded test-environment matrix, and a documentation toolchain bump.

### Changed

- Dependencies upgraded to the latest compatible versions for all supported target frameworks (.NET 10 and .NET 9),
- `Directory.Build.targets` to read `PackageReleaseNotes.txt` via `File.ReadAllText` instead of the line-by-line `ReadLinesFromFile` / `@(…, '%0A')` approach, ensuring newlines are preserved faithfully in NuGet metadata,
- `service-update.yml` workflow to emit clean newlines (no trailing space) in generated `PackageReleaseNotes.txt` entries,
- `testenvironments.json` Docker test entry split into separate `Docker-Ubuntu (net9)` and `Docker-Ubuntu (net10)` environments using versioned image tags (`9` / `10`) for cleaner per-TFM test isolation,
- DocFX image bumped from `2.78.4` to `2.78.5` in `Dockerfile.docfx`,
- `bump-nuget.py` extended with a `carter` → `Codebelt.Extensions.Carter` source-package mapping.

## [10.0.4] - 2026-02-28

This is a service update that focuses on package dependencies.

## [10.0.3] - 2026-02-20

This is a service update that focuses on package dependencies.

## [10.0.2] - 2026-02-15

This is a service update that focuses on package dependencies.

## [10.0.1] - 2026-01-22

This is a service update that focuses on package dependencies.

## [10.0.0] - 2025-11-13

This is a major release that focuses on adapting the latest `.NET 10` release (LTS) in exchange for current `.NET 8` (LTS).

> To ensure access to current features, improvements, and security updates, and to keep the codebase clean and easy to maintain, we target only the latest long-term (LTS), short-term (STS) and (where applicable) cross-platform .NET versions.

## [9.0.8] - 2025-10-20

This is a service update that focuses on package dependencies.

## [9.0.7] - 2025-09-15

This is a service update that focuses on package dependencies.

## [9.0.6] - 2025-08-20

This is a service update that focuses on package dependencies.

## [9.0.5] - 2025-07-11

This is a service update that focuses on package dependencies.

## [9.0.4] - 2025-06-16

This is a service update that focuses on package dependencies.

## [9.0.3] - 2025-05-25

This is a service update that focuses on package dependencies.

## [9.0.2] - 2025-04-16

This is a service update that focuses on package dependencies.

## [9.0.1] - 2025-01-30

This is a service update that primarily focuses on package dependencies and minor improvements.

## [9.0.0] - 2024-11-13

This major release is first and foremost focused on ironing out any wrinkles that have been introduced with .NET 9 preview releases so the final release is production ready together with the official launch from Microsoft.

## [8.4.0] - 2024-09-21


## [8.1.0] - 2024-02-11

### Changed

- RestfulApiVersioningOptions class in the Codebelt.Extensions.Asp.Versioning namespace to include non-official MIME-types in the ValidAcceptHeaders property

## [7.1.0] 2022-12-11

### Added

- UseBuiltInRfc7807 property on the RestfulApiVersioningOptions class in the Codebelt.Extensions.Asp.Versioning namespace (https://github.com/dotnet/aspnet-api-versioning/releases/tag/v7.0.0)

### Removed

- ProblemDetailsFactoryType and UseProblemDetailsFactory{T} from the RestfulApiVersioningOptions class in the Codebelt.Extensions.Asp.Versioning namespace when targeting .NET 7 (https://github.com/dotnet/aspnet-api-versioning/releases/tag/v7.0.0)

### Fixed

- RestfulProblemDetailsFactory class in the Codebelt.Extensions.Asp.Versioning namespace due to changes for 6.3 release of Asp.Versioning (https://github.com/dotnet/aspnet-api-versioning/commit/0a999316aebc81fb1bf3842a2980901f9539978b)

### Changed

- ServiceCollectionExtensions class in the Codebelt.Extensions.Asp.Versioning namespace so that AddRestfulApiVersioning now is backward compatible with recent changes mentioned here https://github.com/dotnet/aspnet-api-versioning/releases/tag/v7.0.0


## [7.0.0] 2022-11-09

### Added

- RestfulApiVersioningOptions class in the Codebelt.Extensions.Asp.Versioning namespace that provides programmatic configuration for the ServiceCollectionExtensions.AddRestfulApiVersioning method
- ServiceCollectionExtensions class in the Codebelt.Extensions.Asp.Versioning namespace that consist of extension methods for the IServiceCollection interface: AddRestfulApiVersioning
- ApplicationBuilderExtensions class in the Codebelt.Extensions.Asp.Versioning namespace that consist of extension methods for the IApplicationBuilder interface: UseRestfulApiVersioning
- RestfulApiVersionReader class in the Codebelt.Extensions.Asp.Versioning namespace that represents a RESTful API version reader that reads the value from a filtered list of HTTP Accept headers in the request
- RestfulProblemDetailsFactory class in the Codebelt.Extensions.Asp.Versioning namespace that represents a RESTful implementation of the IProblemDetailsFactory which throws variants of HttpStatusCodeException that needs to be translated accordingly

[10.2.2]: https://github.com/codebeltnet/asp-versioning/compare/v10.2.1...v10.2.2
[10.2.1]: https://github.com/codebeltnet/asp-versioning/compare/v10.2.0...v10.2.1
[10.2.0]: https://github.com/codebeltnet/asp-versioning/compare/v10.1.0...v10.2.0
[10.1.0]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.9...v10.1.0
[10.0.9]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.8...v10.0.9
[10.0.8]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.7...v10.0.8
[10.0.7]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.6...v10.0.7
[10.0.6]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.5...v10.0.6
[10.0.5]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.4...v10.0.5
[10.0.4]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.3...v10.0.4
[10.0.3]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.2...v10.0.3
[10.0.2]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.1...v10.0.2
[10.0.1]: https://github.com/codebeltnet/asp-versioning/compare/v10.0.0...v10.0.1
[10.0.0]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.8...v10.0.0
[9.0.8]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.7...v9.0.8
[9.0.7]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.6...v9.0.7
[9.0.6]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.5...v9.0.6
[9.0.5]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.4...v9.0.5
[9.0.4]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.3...v9.0.4
[9.0.3]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.2...v9.0.3
[9.0.2]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.1...v9.0.2
[9.0.1]: https://github.com/codebeltnet/asp-versioning/compare/v9.0.0...v9.0.1
[9.0.0]: https://github.com/codebeltnet/asp-versioning/compare/v8.4.0...v9.0.0
[8.4.0]: https://github.com/codebeltnet/asp-versioning/compare/v8.3.2...v8.4.0
[8.3.2]: https://github.com/codebeltnet/asp-versioning/compare/v8.3.0...v8.3.2
[8.3.0]: https://github.com/codebeltnet/asp-versioning/compare/v8.2.0...v8.3.0
[8.2.0]: https://github.com/codebeltnet/asp-versioning/compare/v8.1.0...v8.2.0
[8.1.0]: https://github.com/codebeltnet/asp-versioning/compare/v8.0.1...v8.1.0
[8.0.1]: https://github.com/codebeltnet/asp-versioning/compare/v8.0.0...v8.0.1
[8.0.0]: https://github.com/codebeltnet/asp-versioning/compare/v7.1.0...v8.0.0
[7.1.0]: https://github.com/codebeltnet/asp-versioning/compare/v7.0.0...v7.1.0
[7.0.0]: https://github.com/codebeltnet/asp-versioning/releases/tag/v7.0.0
