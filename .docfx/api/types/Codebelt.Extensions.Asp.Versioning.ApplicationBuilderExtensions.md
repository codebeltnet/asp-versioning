---
uid: Codebelt.Extensions.Asp.Versioning.ApplicationBuilderExtensions
example:
- *content
---
Add `UseRestfulApiVersioning` to the request pipeline when you want status codes written directly to the response — such as the `406 Not Acceptable` or `415 Unsupported Media Type` produced by `Asp.Versioning` when a client requests an unsupported version — translated into typed `HttpStatusCodeException` values that your exception-handling middleware can render. By default the middleware throws a mapped `HttpStatusCodeException` for status codes that `Cuemon.AspNetCore.Http` understands, falling back to `InternalServerErrorException` for everything else. Supply a custom factory when you need to surface a domain-specific exception or attach extra context to the failure.

```csharp
using Cuemon.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Codebelt.Extensions.Asp.Versioning;

public class RestfulApiVersioningPipelineSetup
{
    public static IApplicationBuilder ConfigurePipeline(IApplicationBuilder builder)
    {
        return builder.UseRestfulApiVersioning(context =>
        {
            if (HttpStatusCodeException.TryParse(context.Response.StatusCode, out var mappedException))
            {
                return mappedException;
            }
            return new InternalServerErrorException("Unmapped status code emitted by upstream middleware");
        });
    }
}
```
