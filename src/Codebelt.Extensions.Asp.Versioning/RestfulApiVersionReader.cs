using System;
using System.Collections.Generic;
using System.Linq;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Codebelt.Extensions.Asp.Versioning
{
    /// <summary>
    /// Represents a RESTful API version reader that reads the value from a filtered list of <see cref="HeaderNames.Accept"/> headers in the request.
    /// </summary>
    /// <remarks>This class was introduced to have an inclusive filter on what MIME types to consider valid when parsing HTTP Accept headers; for more information have a read at https://github.com/dotnet/aspnet-api-versioning/issues/887</remarks>
    public class RestfulApiVersionReader : MediaTypeApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestfulApiVersionReader"/> class.
        /// </summary>
        /// <param name="validAcceptHeaders">The valid accept headers to filter the raw collection of <see cref="HeaderNames.Accept"/> headers.</param>
        /// <param name="parameterName">The name of the version parameter.</param>
        public RestfulApiVersionReader(IEnumerable<string> validAcceptHeaders, string parameterName) : base(parameterName)
        {
            ValidAcceptHeaders = new List<string>(validAcceptHeaders ?? Enumerable.Empty<string>());
        }

        /// <summary>
        /// Gets the valid accept headers that <see cref="ReadAcceptHeader"/> will filter by.
        /// </summary>
        /// <value>The valid accept headers that <see cref="ReadAcceptHeader"/> will filter by.</value>
        public IList<string> ValidAcceptHeaders { get; }

        internal bool PreviousBehavior { get; set; }

        /// <summary>
        /// Reads the requested API version from the HTTP request.
        /// </summary>
        /// <param name="request">The HTTP request to read from.</param>
        /// <returns>The requested API versions.</returns>
        public override IReadOnlyList<string> Read(HttpRequest request)
        {
            var versions = base.Read(request);
            if (PreviousBehavior || versions.Count <= 1)
            {
                return versions;
            }

            var parser = request.HttpContext.RequestServices.GetService<IApiVersionParser>();
            if (parser == null)
            {
                return versions;
            }

            var normalizedVersions = new List<string>(versions.Count);
            var seenVersions = new HashSet<string>(StringComparer.Ordinal);
            foreach (var version in versions)
            {
                if (!parser.TryParse(version, out var apiVersion) || apiVersion == null)
                {
                    return versions;
                }

                var normalizedVersion = apiVersion.ToString();
                if (seenVersions.Add(normalizedVersion))
                {
                    normalizedVersions.Add(normalizedVersion);
                }
            }

            return normalizedVersions.Count == versions.Count ? versions : normalizedVersions;
        }

        /// <summary>
        /// Reads the requested API version from the HTTP Accept header.
        /// </summary>
        /// <param name="accept">The <see cref="ICollection{MediaTypeHeaderValue}"/> of Accept headers to read from.</param>
        /// <returns>The API version read or <c>null</c>.</returns>
        /// <remarks>This implementation will, when <see cref="ValidAcceptHeaders"/> has values, filter <paramref name="accept"/> to only include valid <see cref="HeaderNames.Accept"/> headers.</remarks>
        protected override string ReadAcceptHeader(ICollection<MediaTypeHeaderValue> accept)
        {
            var filteredAcceptHeaders = new List<MediaTypeHeaderValue>(ValidAcceptHeaders != null && ValidAcceptHeaders.Count > 0
                ? accept.Where(ah => ValidAcceptHeaders.Any(vah => ah.MediaType.Value != null && ah.MediaType.Value.StartsWith(vah, StringComparison.OrdinalIgnoreCase)))
                : accept);
            return base.ReadAcceptHeader(filteredAcceptHeaders);
        }
    }
}
