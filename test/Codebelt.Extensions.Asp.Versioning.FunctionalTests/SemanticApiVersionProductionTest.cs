using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning;

public class SemanticApiVersionProductionTest : MinimalWebHostTest<ManagedWebMinimalHostFixture>
{
    private const string SemanticVersionHeaderName = "X-Semantic-Version";
    private const string HttpMethodHeaderName = "X-Http-Method";
    private static readonly SemanticApiVersion DefaultVersion = new(1, 2, 3);
    private static readonly SemanticApiVersion AlternateVersion = new(2, 0, 0);
    private static readonly string[] RoutedMethods =
    [
        HttpMethods.Get,
        HttpMethods.Head,
        HttpMethods.Options,
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    ];

    public SemanticApiVersionProductionTest(ManagedWebMinimalHostFixture hostFixture, ITestOutputHelper output) : base(hostFixture, output, typeof(SemanticApiVersionProductionTest))
    {
    }

    public static TheoryData<string, HttpStatusCode, bool> VersionedHttpMethods => new()
    {
        { HttpMethods.Get, HttpStatusCode.NotAcceptable, false },
        { HttpMethods.Head, HttpStatusCode.NotAcceptable, false },
        { HttpMethods.Options, HttpStatusCode.NotAcceptable, false },
        { HttpMethods.Post, HttpStatusCode.UnsupportedMediaType, true },
        { HttpMethods.Put, HttpStatusCode.UnsupportedMediaType, true },
        { HttpMethods.Patch, HttpStatusCode.UnsupportedMediaType, true },
        { HttpMethods.Delete, HttpStatusCode.NotAcceptable, false }
    };

    protected override void ConfigureHost(IHostApplicationBuilder hb)
    {
        var services = hb.Services;
        services.AddRouting();
        services.AddRestfulApiVersioning(options =>
        {
            options.DefaultApiVersion = DefaultVersion;
            options.UseBuiltInRfc7807 = true;
        });
        services.AddSingleton<IApiVersionParser>(SemanticApiVersionParser.Default);
    }

    public override void ConfigureApplication(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            var versionSet = endpoints.NewApiVersionSet()
                .HasApiVersion(DefaultVersion)
                .HasApiVersion(AlternateVersion)
                .Build();

            foreach (var method in RoutedMethods)
            {
                var route = GetRoute(method);
                endpoints.MapMethods(route, [method], (HttpContext context) => WriteResponse(context, method, DefaultVersion))
                    .WithApiVersionSet(versionSet)
                    .MapToApiVersion(DefaultVersion);

                endpoints.MapMethods(route, [method], (HttpContext context) => WriteResponse(context, method, AlternateVersion))
                    .WithApiVersionSet(versionSet)
                    .MapToApiVersion(AlternateVersion);
            }
        });
    }

    [Theory]
    [MemberData(nameof(VersionedHttpMethods))]
    public async Task ExplicitSemanticVersion_ShouldRouteToDefaultSemanticEndpoint(string method, HttpStatusCode unsupportedStatusCode, bool includeRequestBody)
    {
        _ = unsupportedStatusCode;

        using var client = Host.GetTestClient();
        using var response = await client.SendAsync(CreateRequest(method, DefaultVersion.ToString(), includeRequestBody));

        TestOutput.WriteLine($"{method} {GetRoute(method)} => {(int)response.StatusCode}");
        TestOutput.WriteLine(string.Join(", ", response.Headers.Select(header => $"{header.Key}={string.Join("|", header.Value)}")));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(DefaultVersion.ToString(), GetHeader(response.Headers, SemanticVersionHeaderName));
        Assert.Equal(method, GetHeader(response.Headers, HttpMethodHeaderName));
    }

    [Theory]
    [MemberData(nameof(VersionedHttpMethods))]
    public async Task ExplicitSemanticVersion_ShouldRouteToReleaseSemanticEndpoint(string method, HttpStatusCode unsupportedStatusCode, bool includeRequestBody)
    {
        _ = unsupportedStatusCode;

        using var client = Host.GetTestClient();
        using var response = await client.SendAsync(CreateRequest(method, AlternateVersion.ToString(), includeRequestBody));

        TestOutput.WriteLine($"{method} {GetRoute(method)} => {(int)response.StatusCode}");
        TestOutput.WriteLine(string.Join(", ", response.Headers.Select(header => $"{header.Key}={string.Join("|", header.Value)}")));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AlternateVersion.ToString(), GetHeader(response.Headers, SemanticVersionHeaderName));
        Assert.Equal(method, GetHeader(response.Headers, HttpMethodHeaderName));
    }

    [Theory]
    [MemberData(nameof(VersionedHttpMethods))]
    public async Task UnspecifiedSemanticVersion_ShouldUseCurrentImplementationSelectorAcrossHttpMethods(string method, HttpStatusCode unsupportedStatusCode, bool includeRequestBody)
    {
        _ = unsupportedStatusCode;

        using var client = Host.GetTestClient();
        using var response = await client.SendAsync(CreateRequest(method, version: null, includeRequestBody));

        TestOutput.WriteLine($"{method} {GetRoute(method)} => {(int)response.StatusCode}");
        TestOutput.WriteLine(string.Join(", ", response.Headers.Select(header => $"{header.Key}={string.Join("|", header.Value)}")));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(AlternateVersion.ToString(), GetHeader(response.Headers, SemanticVersionHeaderName));
        Assert.Equal(method, GetHeader(response.Headers, HttpMethodHeaderName));
    }

    [Theory]
    [MemberData(nameof(VersionedHttpMethods))]
    public async Task UnknownSemanticVersion_ShouldReturnExpectedNegotiationStatusAcrossHttpMethods(string method, HttpStatusCode expectedStatusCode, bool includeRequestBody)
    {
        using var client = Host.GetTestClient();
        using var response = await client.SendAsync(CreateRequest(method, "1.2.4", includeRequestBody));

        var payload = await response.Content.ReadAsStringAsync();

        TestOutput.WriteLine($"{method} {GetRoute(method)} => {(int)response.StatusCode}");
        TestOutput.WriteLine(payload);

        Assert.Equal(expectedStatusCode, response.StatusCode);
        Assert.False(response.Headers.Contains(SemanticVersionHeaderName));
    }

    private static IResult WriteResponse(HttpContext context, string method, SemanticApiVersion version)
    {
        context.Response.Headers[SemanticVersionHeaderName] = version.ToString();
        context.Response.Headers[HttpMethodHeaderName] = method;
        return Results.NoContent();
    }

    private static HttpRequestMessage CreateRequest(string method, string? version, bool includeRequestBody)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), GetRoute(method));
        request.Headers.Add("Accept", version is null ? "application/json" : $"application/json;v={version}");
        if (includeRequestBody)
        {
            request.Content = new StringContent("""{"payload":"semantic"}""", Encoding.UTF8, "application/json");
        }

        return request;
    }

    private static string GetHeader(HttpResponseHeaders headers, string headerName)
    {
        return headers.TryGetValues(headerName, out IEnumerable<string>? values)
            ? Assert.Single(values)
            : throw new InvalidOperationException($"Expected response header '{headerName}' was not found.");
    }

    private static string GetRoute(string method)
    {
        return $"/semantic/{method.ToLowerInvariant()}";
    }
}
