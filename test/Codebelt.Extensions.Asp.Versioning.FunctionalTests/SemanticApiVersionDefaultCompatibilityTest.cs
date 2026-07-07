using System;
using System.Collections.Generic;
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

public class SemanticApiVersionDefaultCompatibilityTest : MinimalWebHostTest<ManagedWebMinimalHostFixture>
{
    private const string RequestedVersionHeaderName = "X-Requested-Version";
    private static readonly SemanticApiVersion DefaultVersion = new(1, 0, 0);
    private static readonly SemanticApiVersion AlternateVersion = new(2, 0, 0);

    public SemanticApiVersionDefaultCompatibilityTest(ManagedWebMinimalHostFixture hostFixture, ITestOutputHelper output) : base(hostFixture, output, typeof(SemanticApiVersionCompatibilityTest))
    {
    }

    public static TheoryData<string?, ApiVersion> SupportedVersionFormats => new()
    {
        { null, AlternateVersion },
        { "1", DefaultVersion },
        { "1.0", DefaultVersion },
        { "1.0.0", DefaultVersion },
        { "2", AlternateVersion },
        { "2.0", AlternateVersion },
        { "2.0.0", AlternateVersion }
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

        services.AddApiVersionParser(ApiVersionAliasParser.CreateSemanticVersionAlias([DefaultVersion, AlternateVersion]));
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

            endpoints.MapPost("/requests", (HttpContext context) => WriteResponse(context, DefaultVersion))
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(DefaultVersion);

            endpoints.MapPost("/requests", (HttpContext context) => WriteResponse(context, AlternateVersion))
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(AlternateVersion);
        });
    }

    [Theory]
    [MemberData(nameof(SupportedVersionFormats))]
    public async Task PostRequest_ShouldRouteWhenVersionIsMissingOrUsesShortSemanticFormat(string? version, ApiVersion expectedVersion)
    {
        using var client = Host.GetTestClient();
        using var response = await client.SendAsync(CreateRequest(version));

        var payload = await response.Content.ReadAsStringAsync();

        TestOutput.WriteLine(version is null ? "v=<unspecified>" : $"v={version}");
        TestOutput.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
        if (!string.IsNullOrWhiteSpace(payload))
        {
            TestOutput.WriteLine(payload);
        }

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(expectedVersion.ToString(), GetHeader(response.Headers, RequestedVersionHeaderName));
    }

    private static IResult WriteResponse(HttpContext context, ApiVersion version)
    {
        context.Response.Headers[RequestedVersionHeaderName] = version.ToString();
        return Results.NoContent();
    }

    private static HttpRequestMessage CreateRequest(string? version)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/requests")
        {
            Content = new StringContent("""{"payload":"request"}""", Encoding.UTF8, "application/json")
        };

        request.Headers.Add("Accept", version is null ? "application/json" : $"application/json;v={version}");
        if (version is not null)
        {
            request.Content.Headers.ContentType?.Parameters.Add(new NameValueHeaderValue("v", version));
        }

        return request;
    }

    private static string GetHeader(HttpResponseHeaders headers, string headerName)
    {
        return headers.TryGetValues(headerName, out IEnumerable<string>? values)
            ? Assert.Single(values)
            : throw new InvalidOperationException($"Expected response header '{headerName}' was not found.");
    }
}
