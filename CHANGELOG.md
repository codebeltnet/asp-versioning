# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

For more details, please refer to `PackageReleaseNotes.txt` on a per assembly basis in the `.nuget` folder.

> [!NOTE]  
> Changelog entries prior to version 8.4.0 was migrated from previous versions of Cuemon.Extensions.Asp.Versioning.

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
