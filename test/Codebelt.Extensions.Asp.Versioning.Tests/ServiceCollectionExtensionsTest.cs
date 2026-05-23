using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Codebelt.Extensions.Asp.Versioning.Assets;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Cuemon.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning
{
    public class ServiceCollectionExtensionsTest : Test
    {
        public ServiceCollectionExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void AddRestfulApiVersioning_ShouldConfigureApiExplorerOptions_WithExpectedGroupNameFormat()
        {
            using (var app = WebHostTestFactory.Create(services =>
                   {
                       services.AddControllers().AddApplicationPart(typeof(FakeController).Assembly);
                       services.AddRestfulApiVersioning(o =>
                       {
                           o.ParameterName = "version";
                           o.DefaultApiVersion = new ApiVersion(2, 0);
                       });
                   }, app =>
                   {
                       app.UseRouting();
                       app.UseEndpoints(routes => routes.MapControllers());
                   }))
            {
                var options = app.Host.Services.GetRequiredService<IOptions<ApiExplorerOptions>>().Value;

                TestOutput.WriteLine($"GroupNameFormat: {options.GroupNameFormat}");
                TestOutput.WriteLine($"SubstituteApiVersionInUrl: {options.SubstituteApiVersionInUrl}");
                TestOutput.WriteLine($"DefaultApiVersion: {options.DefaultApiVersion}");

                Assert.Equal("'version'VVV", options.GroupNameFormat);
                Assert.True(options.SubstituteApiVersionInUrl);
                Assert.Equal(new ApiVersion(2, 0), options.DefaultApiVersion);
            }
        }

        [Fact]
        public async Task AddRestfulApiVersioning_CustomizeProblemDetails_ShouldThrowInternalServerErrorException_WhenStatusCodeIsUnmapped()
        {
            using (var app = WebHostTestFactory.Create(services =>
                   {
                       services.AddControllers().AddApplicationPart(typeof(FakeController).Assembly);
                       services.AddRestfulApiVersioning();
                   }, app =>
                   {
                       app.UseRouting();
                       app.UseEndpoints(routes => routes.MapControllers());
                   }))
            {
                var client = app.Host.GetTestClient();

                // /fake/problem418 invokes IProblemDetailsService.WriteAsync with Status=418
                // CustomizeProblemDetails fires; TryParse(418) fails (418 is not in Cuemon's mapped codes)
                // -> throws InternalServerErrorException (line 57 in ServiceCollectionExtensions.cs)
                await Assert.ThrowsAsync<InternalServerErrorException>(() => client.GetAsync("/fake/problem418"));
            }
        }
    }
}
