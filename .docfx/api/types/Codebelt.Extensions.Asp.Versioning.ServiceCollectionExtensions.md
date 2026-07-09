---
uid: Codebelt.Extensions.Asp.Versioning.ServiceCollectionExtensions
example:
- *content
---
Call `AddRestfulApiVersioning` on `IServiceCollection` to wire up `Asp.Versioning` for a RESTful API in a single registration. The extension combines `AddApiVersioning`, `AddMvc`, and `AddApiExplorer`, installs `RestfulApiVersionReader` to parse the version from a filtered set of `Accept` media types, and registers problem-details integration that translates version-resolution errors into `HttpStatusCodeException` values compatible with the rest of the pipeline. When the application also needs short version tokens such as `1` or `1.2` to map to canonical `SemanticApiVersion` values, follow up with `AddApiVersionParser` and an `ApiVersionAliasParser` built from the same set of versions. Configure the default version, parameter name, accepted media types, version selector strategy, and problem-details style through the optional `setup` delegate.

```csharp
using System.Collections.Generic;
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Codebelt.Extensions.Asp.Versioning;

public class RestfulApiVersioningServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var stable = new SemanticApiVersion(1, 0, 0);
        var next = new SemanticApiVersion(2, 0, 0);

        services.AddRestfulApiVersioning(options =>
        {
            options.DefaultApiVersion = stable;
            options.ParameterName = "version";
            options.ReportApiVersions = true;
            options.UseApiVersionSelector<LowestImplementedApiVersionSelector>();
        });

        services.AddApiVersionParser(ApiVersionAliasParser.CreateSemanticVersionAlias([stable, next]));
    }
}
```
