using System;
using Asp.Versioning;
using Cuemon;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Represents an <see cref="ApiVersion"/> that follows Semantic Versioning precedence rules.
/// </summary>
/// <remarks>
/// Equality and hash-code calculation use the complete version identity, including build metadata. Precedence comparisons performed by <see cref="CompareTo(ApiVersion)"/> ignore build metadata as required by Semantic Versioning.
/// </remarks>
/// <seealso cref="ApiVersion" />
public sealed class SemanticApiVersion : ApiVersion
{
    private int _hashCode;
    private bool _hashCodeComputed;

    /// <summary>
    /// Creates a new instance of the <see cref="SemanticApiVersion"/> class from a <see cref="Version"/> object.
    /// </summary>
    /// <param name="version">The <see cref="Version"/> to convert to a semantic API version.</param>
    /// <param name="setup">The <see cref="VersionConversionOptions"/> that may be configured.</param>
    /// <returns>A new <see cref="SemanticApiVersion"/> instance derived from the specified <paramref name="version"/>.</returns>
    /// <remarks>
    /// The conversion follows these rules:
    /// <list type="bullet">
    /// <item>
    /// <description>The <see cref="Version.Major"/> component maps to the major version.</description>
    /// </item>
    /// <item>
    /// <description>The <see cref="Version.Minor"/> component maps to the minor version.</description>
    /// </item>
    /// <item>
    /// <description>The <see cref="Version.Build"/> component maps to the patch version; if negative, it defaults to zero.</description>
    /// </item>
    /// <item>
    /// <description>The <see cref="Version.Revision"/> component may be included in build metadata if configured via <see cref="VersionConversionOptions.IncludeRevision"/>.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="version"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="setup"/> failed to configure an instance of <see cref="VersionConversionOptions"/> in a valid state.
    /// </exception>
    public static SemanticApiVersion FromVersion(Version version, Action<VersionConversionOptions> setup = null)
    {
        Validator.ThrowIfNull(version);
        Validator.ThrowIfInvalidConfigurator(setup, out var options);

        var patch = version.Build < 0 ? 0 : version.Build;
        var buildMetadata = options.IncludeRevision && version.Revision >= 0
            ? $"{options.RevisionIdentifier}.{version.Revision}"
            : null;

        return new SemanticApiVersion(version.Major, version.Minor, patch, buildMetadata: buildMetadata);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SemanticApiVersion"/> class.
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
    public SemanticApiVersion(int major, int minor, int patch, string prerelease = null, string buildMetadata = null) : base(ValidateNonNegative(major, nameof(major)), ValidateNonNegative(minor, nameof(minor)), null)
    {
        ValidateNonNegative(patch, nameof(patch));

        if (!string.IsNullOrEmpty(prerelease) && !SemanticApiVersionParser.IsValidPrerelease(prerelease.AsSpan()))
        {
            throw new ArgumentException("The specified pre-release identifier is not a valid semantic version identifier.", nameof(prerelease));
        }

        if (!string.IsNullOrEmpty(buildMetadata) && !SemanticApiVersionParser.IsValidBuildMetadata(buildMetadata.AsSpan()))
        {
            throw new ArgumentException("The specified build metadata is not valid semantic version build metadata.", nameof(buildMetadata));
        }

        PatchVersion = patch;
        Prerelease = prerelease;
        BuildMetadata = buildMetadata;
    }

    /// <summary>
    /// Gets the patch version number.
    /// </summary>
    /// <value>The patch version number.</value>
    public int PatchVersion { get; }

    /// <summary>
    /// Gets the optional pre-release identifier.
    /// </summary>
    /// <value>The optional pre-release identifier.</value>
    public string Prerelease { get; }

    /// <summary>
    /// Gets the optional build metadata.
    /// </summary>
    /// <value>The optional build metadata.</value>
    /// <remarks>Build metadata is part of exact version identity but is ignored by <see cref="CompareTo(ApiVersion)"/>.</remarks>
    public string BuildMetadata { get; }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        if (_hashCodeComputed)
        {
            return _hashCode;
        }

        if (IsCompatibleWithStandardApiVersion())
        {
            _hashCode = base.GetHashCode();
            _hashCodeComputed = true;
            return _hashCode;
        }

        HashCode hash = default;
        hash.Add(MajorVersion.GetValueOrDefault());
        hash.Add(MinorVersion.GetValueOrDefault());
        hash.Add(PatchVersion);
        hash.Add(Prerelease, StringComparer.Ordinal);
        hash.Add(BuildMetadata, StringComparer.Ordinal);

        _hashCode = hash.ToHashCode();
        _hashCodeComputed = true;
        return _hashCode;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is ApiVersion version && Equals(version);
    }

    /// <inheritdoc />
    public override bool Equals(ApiVersion other)
    {
        if (other is SemanticApiVersion version)
        {
            return MajorVersion == version.MajorVersion
                && MinorVersion == version.MinorVersion
                && PatchVersion == version.PatchVersion
                && string.Equals(Prerelease, version.Prerelease, StringComparison.Ordinal)
                && string.Equals(BuildMetadata, version.BuildMetadata, StringComparison.Ordinal);
        }

        return other is not null
            && IsCompatibleWithStandardApiVersion()
            && MajorVersion == other.MajorVersion
            && MinorVersion == other.MinorVersion
            && string.IsNullOrEmpty(other.Status);
    }

    /// <inheritdoc />
    public override int CompareTo(ApiVersion other)
    {
        if (ReferenceEquals(other, null))
        {
            return 1;
        }

        if (other is not SemanticApiVersion version)
        {
            return base.CompareTo(other);
        }

        var result = MajorVersion.GetValueOrDefault().CompareTo(version.MajorVersion.GetValueOrDefault());
        if (result != 0)
        {
            return result;
        }

        result = MinorVersion.GetValueOrDefault().CompareTo(version.MinorVersion.GetValueOrDefault());
        if (result != 0)
        {
            return result;
        }

        result = PatchVersion.CompareTo(version.PatchVersion);
        return result != 0 ? result : ComparePrerelease(Prerelease, version.Prerelease);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <inheritdoc />
    public override string ToString(string format)
    {
        return ToString(format, null);
    }

    /// <inheritdoc />
    public override string ToString(string format, IFormatProvider formatProvider)
    {
        var formatter = SemanticApiVersionFormatter.GetInstance(formatProvider);
        return formatter.Format(format, this, formatProvider);
    }

    /// <inheritdoc />
    public override bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider provider)
    {
        var text = ToString(format.IsEmpty ? null : format.ToString(), provider);
        if (text.AsSpan().TryCopyTo(destination))
        {
            charsWritten = text.Length;
            return true;
        }

        charsWritten = 0;
        return false;
    }

    private static int ComparePrerelease(string left, string right)
    {
        var leftIsEmpty = string.IsNullOrEmpty(left);
        var rightIsEmpty = string.IsNullOrEmpty(right);

        if (leftIsEmpty && rightIsEmpty)
        {
            return 0;
        }

        if (leftIsEmpty)
        {
            return 1;
        }

        if (rightIsEmpty)
        {
            return -1;
        }

        var leftParts = left.Split('.');
        var rightParts = right.Split('.');
        var length = Math.Min(leftParts.Length, rightParts.Length);

        for (var i = 0; i < length; i++)
        {
            var result = ComparePrereleaseIdentifier(leftParts[i], rightParts[i]);
            if (result != 0)
            {
                return result;
            }
        }

        return leftParts.Length.CompareTo(rightParts.Length);
    }

    private static int ComparePrereleaseIdentifier(string left, string right)
    {
        var leftNumeric = IsNumericIdentifier(left);
        var rightNumeric = IsNumericIdentifier(right);

        if (leftNumeric && rightNumeric)
        {
            return CompareNumericIdentifier(left, right);
        }

        if (leftNumeric)
        {
            return -1;
        }

        if (rightNumeric)
        {
            return 1;
        }

        return string.CompareOrdinal(left, right);
    }

    private static int CompareNumericIdentifier(string left, string right)
    {
        var result = left.Length.CompareTo(right.Length);
        return result != 0 ? result : string.CompareOrdinal(left, right);
    }

    private static bool IsNumericIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }

    private bool IsCompatibleWithStandardApiVersion()
    {
        return PatchVersion == 0
            && string.IsNullOrEmpty(Prerelease)
            && string.IsNullOrEmpty(BuildMetadata);
    }

    private static int ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Semantic version parts must be non-negative.");
        }

        return value;
    }
}
