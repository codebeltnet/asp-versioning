---
uid: Codebelt.Extensions.Asp.Versioning.VersionConversionOptions
example:
- *content
---
Use `VersionConversionOptions` to control how a .NET `Version` is translated into a `SemanticApiVersion` by `SemanticApiVersion.FromVersion`. The options let callers decide whether the `Revision` component is folded into the build metadata and which identifier labels that segment. Configure a `VersionConversionOptions` instance, validate it with `ValidateOptions`, then pass it through the `Action<VersionConversionOptions>` overload of `SemanticApiVersion.FromVersion`. The example below opts into revision mapping and renames the build-metadata key to `build`, so a four-part `Version(1, 2, 3, 4)` renders as `1.2.3+build.4`.

```csharp
using System;
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticVersionFromVersionSetup
{
    public string ConvertAssemblyVersionToSemantic()
    {
        var options = new VersionConversionOptions
        {
            IncludeRevision = true,
            RevisionIdentifier = "build"
        };

        var assemblyVersion = new Version(1, 2, 3, 4);
        return SemanticApiVersion.FromVersion(assemblyVersion, o =>
        {
            o.IncludeRevision = options.IncludeRevision;
            o.RevisionIdentifier = options.RevisionIdentifier;
        }).ToString();
    }
}
```
