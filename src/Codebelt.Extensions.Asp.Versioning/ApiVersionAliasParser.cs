using Asp.Versioning;
using Cuemon;
using System;
using System.Collections.Generic;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Resolves friendly API version aliases before delegating to another <see cref="IApiVersionParser"/>.
/// </summary>
/// <remarks>
/// Use this parser when callers should be able to request a version by a shortened or compatibility-oriented token, while the application still works with the canonical <see cref="ApiVersion"/> instance.
/// Alias matching is performed before the fallback parser is invoked.
/// </remarks>
/// <seealso cref="IApiVersionParser" />
public class ApiVersionAliasParser : IApiVersionParser
{
    private readonly IReadOnlyDictionary<string, ApiVersion> _aliases;
    private readonly IApiVersionParser _fallback;

    /// <summary>
    /// Creates an API version parser that recognizes shortened aliases for a single semantic API version.
    /// </summary>
    /// <param name="version">The semantic API version to expose through major, major-minor, and major-minor-patch aliases.</param>
    /// <returns>
    /// An <see cref="IApiVersionParser"/> that maps aliases such as <c>1</c>, <c>1.2</c>, and <c>1.2.3</c> to the specified <paramref name="version"/>.
    /// </returns>
    /// <remarks>
    /// The returned parser falls back to <see cref="ApiVersionParser.Default"/> when a requested version is not one of the generated aliases.
    /// </remarks>
    public static IApiVersionParser CreateSemanticVersionAlias(SemanticApiVersion version)
    {
        return CreateSemanticVersionAlias([version]);
    }

    /// <summary>
    /// Creates an API version parser that recognizes shortened aliases for the specified semantic API versions.
    /// </summary>
    /// <param name="versions">The semantic API versions to expose through major, major-minor, and major-minor-patch aliases.</param>
    /// <returns>
    /// An <see cref="IApiVersionParser"/> that maps each generated alias to its corresponding <see cref="SemanticApiVersion"/>.
    /// </returns>
    /// <remarks>
    /// For each supplied version, aliases are generated from <see cref="ApiVersion.MajorVersion"/>, <see cref="ApiVersion.MinorVersion"/>, and <see cref="SemanticApiVersion.PatchVersion"/>.
    /// If duplicate aliases are produced, the first version that contributed the alias is retained. The returned parser falls back to <see cref="ApiVersionParser.Default"/> when a requested version is not one of the generated aliases.
    /// </remarks>
    public static IApiVersionParser CreateSemanticVersionAlias(IEnumerable<SemanticApiVersion> versions)
    {
        var aliases = new Dictionary<string, ApiVersion>();
        foreach (var version in versions)
        {
            aliases.TryAdd($"{version.MajorVersion}", version);
            aliases.TryAdd($"{version.MajorVersion}.{version.MinorVersion}", version);
            aliases.TryAdd($"{version.MajorVersion}.{version.MinorVersion}.{version.PatchVersion}", version);
        }
        return new ApiVersionAliasParser(aliases);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAliasParser"/> class that falls back to <see cref="ApiVersionParser.Default"/>.
    /// </summary>
    /// <param name="aliases">The alias map to use before invoking the default parser.</param>
    /// <remarks>
    /// The dictionary key is the external version token accepted from a request, and the dictionary value is the canonical <see cref="ApiVersion"/> returned for that token.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="aliases"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="aliases"/> contains no entries.
    /// </exception>
    public ApiVersionAliasParser(IReadOnlyDictionary<string, ApiVersion> aliases) : this(aliases, ApiVersionParser.Default)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionAliasParser"/> class with the specified alias map and fallback parser.
    /// </summary>
    /// <param name="aliases">The alias map to use before invoking <paramref name="fallback"/>.</param>
    /// <param name="fallback">The parser to use when <paramref name="aliases"/> does not contain the requested version token.</param>
    /// <remarks>
    /// The dictionary key is the external version token accepted from a request, and the dictionary value is the canonical <see cref="ApiVersion"/> returned for that token.
    /// Alias lookup uses the comparer configured by the supplied <paramref name="aliases"/> dictionary.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="aliases"/> or <paramref name="fallback"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="aliases"/> contains no entries.
    /// </exception>
    public ApiVersionAliasParser(IReadOnlyDictionary<string, ApiVersion> aliases, IApiVersionParser fallback)
    {
        Validator.ThrowIfSequenceNullOrEmpty(aliases);
        Validator.ThrowIfNull(fallback);
        _aliases = aliases;
        _fallback = fallback;
    }

    /// <summary>
    /// Parses the specified text into an API version by resolving aliases before using the fallback parser.
    /// </summary>
    /// <param name="text">The API version text to parse.</param>
    /// <returns>The API version that matched either an alias or the fallback parser.</returns>
    /// <exception cref="FormatException">
    /// <paramref name="text"/> is neither a known alias nor a version accepted by the fallback parser.
    /// </exception>
    public ApiVersion Parse(ReadOnlySpan<char> text)
    {
        return TryParse(text, out var apiVersion)
            ? apiVersion
            : throw new FormatException("The specified API version is not valid.");
    }

    /// <summary>
    /// Tries to parse the specified text into an API version by resolving aliases before using the fallback parser.
    /// </summary>
    /// <param name="text">The API version text to parse.</param>
    /// <param name="apiVersion">
    /// When this method returns, contains the API version that matched either an alias or the fallback parser; otherwise, contains <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if <paramref name="text"/> matched an alias or was accepted by the fallback parser; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Override this method to customize alias resolution while preserving the same parse contract.
    /// </remarks>
    public virtual bool TryParse(ReadOnlySpan<char> text, out ApiVersion apiVersion)
    {
        if (_aliases.TryGetValue(text.ToString(), out apiVersion))
        {
            return true;
        }

        return _fallback.TryParse(text, out apiVersion);
    }
}
