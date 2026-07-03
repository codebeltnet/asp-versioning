using System;
using Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Defines semantic API version metadata.
/// </summary>
/// <seealso cref="ApiVersionAttribute" />
public sealed class SemanticApiVersionAttribute : ApiVersionAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="version">The semantic API version string to parse.</param>
    /// <exception cref="FormatException">
    /// <paramref name="version"/> is not a valid semantic API version.
    /// </exception>
    public SemanticApiVersionAttribute(string version) : base(SemanticApiVersionParser.Default, version)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticApiVersionAttribute"/> class.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <param name="prerelease">The optional pre-release identifier.</param>
    /// <param name="buildMetadata">The optional build metadata.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="major"/>, <paramref name="minor"/>, or <paramref name="patch"/> is less than zero.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="prerelease"/> is not a valid semantic version pre-release identifier - or -
    /// <paramref name="buildMetadata"/> is not valid semantic version build metadata.
    /// </exception>
    public SemanticApiVersionAttribute(int major, int minor, int patch, string prerelease = null, string buildMetadata = null) : base(new SemanticApiVersion(major, minor, patch, prerelease, buildMetadata))
    {
    }
}
