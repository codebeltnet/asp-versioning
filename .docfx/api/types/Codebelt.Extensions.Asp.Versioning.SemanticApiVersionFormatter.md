---
uid: Codebelt.Extensions.Asp.Versioning.SemanticApiVersionFormatter
example:
- *content
---
Use `SemanticApiVersionFormatter` when a semantic API version needs the same formatting tokens used by `Asp.Versioning` plus patch, pre-release, and build metadata output. The formatter below creates a complete display value and a compact group name from the same version.

```csharp
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticVersionDisplay
{
    public (string DisplayName, string GroupName) Format()
    {
        var version = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");
        var formatter = SemanticApiVersionFormatter.GetInstance(null);

        var displayName = formatter.Format("G", version, null);
        var groupName = formatter.Format("VV'-'S", version, null);

        return (displayName, groupName);
    }
}
```
