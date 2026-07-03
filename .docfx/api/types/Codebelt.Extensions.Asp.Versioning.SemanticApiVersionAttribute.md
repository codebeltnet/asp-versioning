---
uid: Codebelt.Extensions.Asp.Versioning.SemanticApiVersionAttribute
example:
- *content
---
Use `SemanticApiVersionAttribute` when an endpoint contract uses Semantic Versioning instead of the usual major/minor API-version shape. The attribute parses the supplied text through `SemanticApiVersionParser`, so the declared metadata keeps its patch, pre-release, and build metadata.

```csharp
using System.Linq;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticControllerVersionMetadata
{
    public string GetDeclaredVersion()
    {
        var attribute = new SemanticApiVersionAttribute("2.1.3-rc.1+build.7");
        var version = attribute.Versions.OfType<SemanticApiVersion>().Single();

        return version.ToString();
    }
}
```
