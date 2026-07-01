---
uid: Codebelt.Extensions.Asp.Versioning.RestfulApiVersionReader
example:
- *content
---
Use `RestfulApiVersionReader` when you need fine-grained control over which Accept media types are eligible for version parsing — for example when configuring `AddApiVersioning` directly rather than using the `AddRestfulApiVersioning` convenience method. The reader filters the incoming `Accept` header collection to only the entries whose media types start with one of the configured `validAcceptHeaders` values before delegating to `MediaTypeApiVersionReader`, which prevents browser-injected entries such as `text/html` or `image/webp` from being misread as version indicators.

```csharp
using System.Collections.Generic;
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Codebelt.Extensions.Asp.Versioning;

public class CustomVersioningSetup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        var acceptHeaders = new List<string>
        {
            "application/json",
            "application/xml",
            "text/json",
            "text/xml",
            "text/plain",
            "*/*"
        };

        services.AddApiVersioning(o =>
        {
            o.DefaultApiVersion = new ApiVersion(1, 0);
            o.AssumeDefaultVersionWhenUnspecified = true;
            o.ApiVersionReader = new RestfulApiVersionReader(acceptHeaders, "v");
        });
    }
}
```
