---
uid: Codebelt.Extensions.Asp.Versioning.MapToSemanticApiVersionAttribute
example:
- *content
---
Use `MapToSemanticApiVersionAttribute` when one implementation advertises multiple semantic API versions and a specific action should handle only one of them. The example constructs the same metadata an action attribute declaration would create and reads back the mapped semantic API version.

```csharp
using System.Linq;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticActionMapping
{
    public string GetMappedVersion()
    {
        var attribute = new MapToSemanticApiVersionAttribute("1.2.3-alpha+build.5");
        var version = attribute.Versions.OfType<SemanticApiVersion>().Single();

        return version.ToString();
    }
}
```
