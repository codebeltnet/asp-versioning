---
uid: Codebelt.Extensions.Asp.Versioning.ServiceCollectionExtensions
example:
- *content
---
Call `AddRestfulApiVersioning` on `IServiceCollection` to wire up `Asp.Versioning` for a RESTful API in a single registration. The extension combines `AddApiVersioning`, `AddMvc`, and `AddApiExplorer`, installs `RestfulApiVersionReader` to parse the version from a filtered set of `Accept` media types, and registers problem-details integration that translates version-resolution errors into `HttpStatusCodeException` values compatible with the rest of the pipeline. Configure the default version, parameter name, accepted media types, version selector strategy, and problem-details style through the optional `setup` delegate.

```csharp
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Codebelt.Extensions.Asp.Versioning;

public class RestfulApiVersioningServiceRegistration
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddRestfulApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.ParameterName = "version";
            options.ReportApiVersions = true;
            options.UseApiVersionSelector<LowestImplementedApiVersionSelector>();
        });
    }
}
```
