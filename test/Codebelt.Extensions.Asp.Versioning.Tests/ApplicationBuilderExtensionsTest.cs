using System;
using System.Threading.Tasks;
using Codebelt.Extensions.Asp.Versioning.Assets;
using Codebelt.Extensions.Xunit;
using Codebelt.Extensions.Xunit.Hosting.AspNetCore;
using Cuemon.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning
{
    public class ApplicationBuilderExtensionsTest : Test
    {
        public ApplicationBuilderExtensionsTest(ITestOutputHelper output) : base(output)
        {
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Use minimal controller registration.
            // [ApiController]'s ClientErrorResultFilter automatically converts StatusCode(4xx) results
            // to problem details (sets application/problem+json content-type), which prevents
            // UseStatusCodePages from firing (it requires content-type to be null).
            // The minimal-API lambda endpoints added in ConfigureApp bypass [ApiController] filters.
            services.AddControllers().AddApplicationPart(typeof(FakeController).Assembly);
        }

        private static void ConfigureApp(IApplicationBuilder app, Func<HttpContext, HttpStatusCodeException> factory = null)
        {
            if (factory != null)
            {
                app.UseRestfulApiVersioning(factory);
            }
            else
            {
                app.UseRestfulApiVersioning();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // Minimal-API lambda endpoints: just set the status code with no content-type.
                // These bypass [ApiController]'s ClientErrorResultFilter so UseStatusCodePages can fire.
                endpoints.MapGet("/test/not-acceptable", ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status406NotAcceptable;
                    return Task.CompletedTask;
                });
                endpoints.MapGet("/test/teapot", ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status418ImATeapot;
                    return Task.CompletedTask;
                });
            });
        }

        [Fact]
        public async Task UseRestfulApiVersioning_WithCustomFactory_ShouldInvokeProvidedFactory()
        {
            var customFactoryInvoked = false;

            using (var app = WebHostTestFactory.Create(ConfigureServices, appBuilder =>
                   {
                       ConfigureApp(appBuilder, context =>
                       {
                           customFactoryInvoked = true;
                           return new NotAcceptableException("Custom factory: version not acceptable");
                       });
                   }))
            {
                var client = app.Host.GetTestClient();

                // /test/not-acceptable is a minimal-API endpoint that sets 406 with no content-type.
                // UseStatusCodePages fires; the custom factory (non-null ??= branch) is invoked.
                var ex = await Assert.ThrowsAsync<NotAcceptableException>(() => client.GetAsync("/test/not-acceptable"));

                Assert.True(customFactoryInvoked);
                Assert.StartsWith("Custom factory:", ex.Message);
            }
        }

        [Fact]
        public async Task UseRestfulApiVersioning_DefaultFactory_WithMappedStatusCode_ShouldThrowCorrespondingException()
        {
            using (var app = WebHostTestFactory.Create(ConfigureServices, app => ConfigureApp(app)))
            {
                var client = app.Host.GetTestClient();

                // /test/not-acceptable returns 406 with no content-type.
                // UseStatusCodePages fires; default factory invoked with Response.StatusCode=406.
                // TryParse(406) succeeds -> NotAcceptableException is returned and thrown.
                await Assert.ThrowsAsync<NotAcceptableException>(() => client.GetAsync("/test/not-acceptable"));
            }
        }

        [Fact]
        public async Task UseRestfulApiVersioning_DefaultFactory_WithUnmappedStatusCode_ShouldThrowInternalServerErrorException()
        {
            using (var app = WebHostTestFactory.Create(ConfigureServices, app => ConfigureApp(app)))
            {
                var client = app.Host.GetTestClient();

                // /test/teapot returns 418 with no content-type.
                // UseStatusCodePages fires; default factory invoked with Response.StatusCode=418.
                // TryParse(418) fails (418 is not in Cuemon's mapped codes) -> InternalServerErrorException.
                await Assert.ThrowsAsync<InternalServerErrorException>(() => client.GetAsync("/test/teapot"));
            }
        }
    }
}
