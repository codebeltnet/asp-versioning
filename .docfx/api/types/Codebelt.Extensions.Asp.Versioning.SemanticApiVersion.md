---
uid: Codebelt.Extensions.Asp.Versioning.SemanticApiVersion
example:
- *content
---
Use `SemanticApiVersion` when API-version ordering must follow Semantic Versioning while equality still distinguishes exact build identities. In this example, two builds have equal precedence, but they remain different exact API-version values.

```csharp
using Codebelt.Extensions.Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticVersionPrecedence
{
    public bool HasSamePrecedenceButDifferentIdentity()
    {
        var buildOne = new SemanticApiVersion(1, 2, 3, buildMetadata: "build.1");
        var buildTwo = new SemanticApiVersion(1, 2, 3, buildMetadata: "build.2");

        return buildOne.CompareTo(buildTwo) == 0 && !buildOne.Equals(buildTwo);
    }
}
```
