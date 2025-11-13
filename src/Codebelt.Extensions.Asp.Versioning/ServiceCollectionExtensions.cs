using System;
using Asp.Versioning;
using Cuemon;
using Microsoft.Extensions.DependencyInjection;
using Cuemon.AspNetCore.Http;

namespace Codebelt.Extensions.Asp.Versioning
{
    /// <summary>
    /// Extension methods for the <see cref="IServiceCollection"/> interface.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a compound service API versioning to the specified <paramref name="services"/> collection that is optimized for RESTful APIs.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
        /// <param name="setup">The <see cref="RestfulApiVersioningOptions"/> that may be configured.</param>
        /// <returns>A reference to <paramref name="services" /> so that additional calls can be chained.</returns>
        /// <summary>
        /// Adds and configures RESTful API versioning, related MVC conventions, API Explorer grouping, and Problem Details handling to the service collection.
        /// </summary>
        /// <param name="setup">Optional configurator for RestfulApiVersioningOptions that controls default API version, valid Accept headers, parameter name, conventions, API version selector type, and Problem Details behavior.</param>
        /// <returns>The original IServiceCollection instance for chaining.</returns>
        public static IServiceCollection AddRestfulApiVersioning(this IServiceCollection services, Action<RestfulApiVersioningOptions> setup = null)
        {
            Validator.ThrowIfNull(services);
            Validator.ThrowIfInvalidConfigurator(setup, out var options);

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = options.DefaultApiVersion;
                o.ReportApiVersions = options.ReportApiVersions;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = new RestfulApiVersionReader(options.ValidAcceptHeaders, options.ParameterName);
                o.ApiVersionSelector = (Activator.CreateInstance(options.ApiVersionSelectorType, o) as IApiVersionSelector)!;
            }).AddMvc(o =>
            {
                o.Conventions = options.Conventions;
            }).AddApiExplorer(o =>
            {
                o.GroupNameFormat = $"'{options.ParameterName}'VVV";
                o.DefaultApiVersion = options.DefaultApiVersion;
                o.SubstituteApiVersionInUrl = true;
            });

            if (options.UseBuiltInRfc7807)
            {
                services.AddProblemDetails();
            }
            else
            {
                services.AddProblemDetails(o => o.CustomizeProblemDetails = context =>
                {
                    var pd = context.ProblemDetails;
                    if (HttpStatusCodeException.TryParse(pd.Status ?? 500, pd.Detail ?? pd.Title, null, out var statusCodeEquivalentException))
                    {
                        throw statusCodeEquivalentException;
                    }

                    throw new InternalServerErrorException(pd.Detail);
                });
            }

            return services;
        }
    }
}