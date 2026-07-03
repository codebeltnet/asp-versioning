---
uid: Codebelt.Extensions.Asp.Versioning
summary: *content
---
Use this namespace when building versioned ASP.NET Core RESTful APIs that accept the API version from the HTTP `Accept` header — a common pattern for APIs consumed by real browsers and heterogeneous clients — without writing bespoke middleware to filter browser MIME types or translate Asp.Versioning error codes into structured exception types. It also provides semantic API version metadata for APIs whose public contract needs `major.minor.patch`, pre-release, and build metadata while still participating in the `Asp.Versioning.ApiVersion` model.

RESTful API versioning built on `Asp.Versioning` normally requires coordinating three separate registration calls — `AddApiVersioning`, `AddMvc`, and `AddApiExplorer` — alongside a version reader that copes with the broad range of `Accept` header values real browsers and HTTP clients send. This namespace provides a single-call registration path and a filtered media-type version reader that handles that coordination automatically.

Start with `AddRestfulApiVersioning` on `IServiceCollection`: it wires all three calls together, installs `RestfulApiVersionReader` to parse the API version from the `Accept` header while ignoring irrelevant browser MIME types, and registers problem-details integration compatible with RFC 7807. To translate Asp.Versioning status-code responses into typed `HttpStatusCodeException` values that the rest of your pipeline can handle, call `UseRestfulApiVersioning` on `IApplicationBuilder` in the middleware pipeline. Configure behaviour — default API version, parameter name, accepted media types, version selector strategy, and problem-details style — through `RestfulApiVersioningOptions`.

Use `SemanticApiVersion`, `SemanticApiVersionParser`, and the semantic version attributes when you need `Asp.Versioning` endpoints to advertise or map versions such as `1.2.3-alpha+build.5`. `SemanticApiVersion.CompareTo` follows Semantic Versioning precedence and ignores build metadata for ordering, while equality keeps build metadata as part of exact version identity. The existing RESTful API Explorer group-name format is unchanged; `SemanticApiVersionFormatter` collapses semantic endpoint versions such as `1.0.1`, `1.2.0`, and `1.2.3-alpha+build.5` to the `v1` group.

[!INCLUDE [availability-modern](../../includes/availability-modern.md)]

Complements: [Asp.Versioning](https://github.com/dotnet/aspnet-api-versioning) together with Open API/Swagger for RESTful APIs. 🔗

### Extension Members

|Type|Ext|Methods|
|--:|:-:|---|
|`IApplicationBuilder`|⬇️|`UseRestfulApiVersioning`|
|`IServiceCollection`|⬇️|`AddRestfulApiVersioning`|
