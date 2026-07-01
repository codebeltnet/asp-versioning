---
uid: Codebelt.Extensions.Asp.Versioning.RestfulApiVersioningOptions
example:
- *content
---
Inspect or adjust the defaults applied by `AddRestfulApiVersioning` by constructing a `RestfulApiVersioningOptions` instance directly. Defaults include API version 1.0, the parameter name `"v"`, a broad set of accepted media types, `CurrentImplementationApiVersionSelector`, and custom RFC 7807 problem-details handling. The example below trims the accepted media-type list to JSON only and switches the version selector to always route to the lowest implemented version.

```csharp
using System.Collections.Generic;
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class VersioningOptionsSetup
{
    public static RestfulApiVersioningOptions CreateOptions()
    {
        var options = new RestfulApiVersioningOptions
        {
            DefaultApiVersion = new ApiVersion(1, 0),
            ParameterName = "v",
            ReportApiVersions = true,
            ValidAcceptHeaders = new List<string>
            {
                "application/json",
                "text/json"
            }
        };
        options.UseApiVersionSelector<LowestImplementedApiVersionSelector>();
        return options;
    }
}
```

