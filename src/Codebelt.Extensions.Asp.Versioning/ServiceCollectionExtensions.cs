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
        /// <remarks>
        /// This is a convenient method to add API versioning to your ASP.NET Core WebApi. Call <c>AddApiVersioning</c>, <c>AddMvc</c> and <c>AddApiExplorer</c>. Configuration, which is optimized for RESTful APIs, are done through <paramref name="setup"/>.
        /// When <see cref="RestfulApiVersioningOptions.DefaultApiVersion"/> is a <see cref="SemanticApiVersion"/>, semantic aliases are automatically registered only for that default version. Applications that expose additional semantic versions and need aliases such as <c>2</c> or <c>2.0</c> should call <see cref="AddApiVersionParser{T}(IServiceCollection, T)"/> with <see cref="ApiVersionAliasParser.CreateSemanticVersionAlias(System.Collections.Generic.IEnumerable{SemanticApiVersion})"/>.
        /// </remarks>
        public static IServiceCollection AddRestfulApiVersioning(this IServiceCollection services, Action<RestfulApiVersioningOptions> setup = null)
        {
            Validator.ThrowIfNull(services);
            Validator.ThrowIfInvalidConfigurator(setup, out var options);

            var defaultSemanticApiVersion = options.DefaultApiVersion as SemanticApiVersion;
            var preferSemVerBehavior = defaultSemanticApiVersion != null;

            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = options.DefaultApiVersion;
                o.ReportApiVersions = options.ReportApiVersions;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ApiVersionReader = options.ApiVersionReader ?? new RestfulApiVersionReader(options.ValidAcceptHeaders, options.ParameterName)
                {
                    PreviousBehavior = !preferSemVerBehavior
                };
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

            if (defaultSemanticApiVersion != null)
            {
                services.AddApiVersionParser(ApiVersionAliasParser.CreateSemanticVersionAlias(defaultSemanticApiVersion));
            }

            return services;
        }

        /// <summary>
        /// Adds the specified API version parser as the singleton parser used by API versioning services.
        /// </summary>
        /// <typeparam name="T">The concrete <see cref="IApiVersionParser"/> type to register.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to extend.</param>
        /// <param name="parser">The parser instance that should resolve API version values for the application.</param>
        /// <returns>A reference to <paramref name="services" /> so that additional calls can be chained.</returns>
        /// <remarks>
        /// Register a custom parser when the application accepts version formats that differ from the default parser, such as semantic version aliases or compatibility tokens.
        /// The supplied <paramref name="parser"/> is registered as the <see cref="IApiVersionParser"/> singleton.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="services"/> or <paramref name="parser"/> is <c>null</c>.
        /// </exception>
        public static IServiceCollection AddApiVersionParser<T>(this IServiceCollection services, T parser) where T : IApiVersionParser
        {
            Validator.ThrowIfNull(services);
            Validator.ThrowIfNull(parser);
            return services.AddSingleton<IApiVersionParser>(parser);
        }
    }
}
