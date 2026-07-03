---
uid: Codebelt.Extensions.Asp.Versioning.SemanticApiVersionParser
example:
- *content
---
Use `SemanticApiVersionParser` when a raw API-version value must be parsed before it is passed into `Asp.Versioning` services. The parsed result is still an `ApiVersion`, but callers can use the semantic subtype to inspect patch, pre-release, and build metadata values.

```csharp
using Asp.Versioning;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticVersionRequestReader
{
    public string Describe(string requestedVersion)
    {
        var parser = new SemanticApiVersionParser();
        ApiVersion apiVersion = parser.Parse(requestedVersion);
        var semanticVersion = (SemanticApiVersion)apiVersion;

        return semanticVersion.Prerelease == null
            ? $"Stable API {semanticVersion.MajorVersion}.{semanticVersion.MinorVersion}.{semanticVersion.PatchVersion}"
            : $"Preview API {semanticVersion.Prerelease}";
    }
}
```
