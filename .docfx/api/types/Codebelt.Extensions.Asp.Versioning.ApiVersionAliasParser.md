---
uid: Codebelt.Extensions.Asp.Versioning.ApiVersionAliasParser
example:
- *content
---
Use `ApiVersionAliasParser` when callers send short version tokens such as `1`, `1.2`, or `1.2.3` over the wire but the rest of the application works with canonical `SemanticApiVersion` instances. The static `CreateSemanticVersionAlias` factory generates the alias map for a set of semantic versions, and `Parse` resolves a request token to the canonical version while the parser falls back to `ApiVersionParser.Default` for anything outside the alias table. The example below parses `1.2` and `2.0` from request strings and returns the resolved semantic versions.

```csharp
using System;
using System.Collections.Generic;
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class AliasAwareVersionResolver
{
    public IReadOnlyList<string> ResolveShortVersionTokens()
    {
        var stable = new SemanticApiVersion(1, 2, 0);
        var next = new SemanticApiVersion(2, 0, 0);

        IApiVersionParser parser = ApiVersionAliasParser.CreateSemanticVersionAlias([stable, next]);

        return new[]
        {
            parser.Parse("1.2").ToString(),
            parser.Parse("2.0").ToString()
        };
    }
}
```
