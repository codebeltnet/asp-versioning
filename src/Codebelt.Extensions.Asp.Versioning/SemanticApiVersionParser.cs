using System;
using Asp.Versioning;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Parses semantic API version strings into <see cref="SemanticApiVersion"/> instances.
/// </summary>
/// <seealso cref="IApiVersionParser" />
public sealed class SemanticApiVersionParser : IApiVersionParser
{
    /// <summary>
    /// Gets the default parser instance.
    /// </summary>
    /// <value>The default parser instance.</value>
    public static SemanticApiVersionParser Default { get; } = new();

    /// <inheritdoc />
    public ApiVersion Parse(ReadOnlySpan<char> text)
    {
        return TryParse(text, out var apiVersion)
            ? apiVersion
            : throw new FormatException("The specified API version is not a valid semantic version.");
    }

    /// <inheritdoc />
    public bool TryParse(ReadOnlySpan<char> text, out ApiVersion apiVersion)
    {
        if (!TryParse(text, out var major, out var minor, out var patch, out var prerelease, out var buildMetadata))
        {
            apiVersion = null;
            return false;
        }

        apiVersion = new SemanticApiVersion(major, minor, patch, prerelease, buildMetadata);
        return true;
    }

    internal static bool IsValidPrerelease(ReadOnlySpan<char> value)
    {
        return IsValidDotSeparatedIdentifiers(value, validateNumericLeadingZeroes: true);
    }

    internal static bool IsValidBuildMetadata(ReadOnlySpan<char> value)
    {
        return IsValidDotSeparatedIdentifiers(value, validateNumericLeadingZeroes: false);
    }

    private static bool TryParse(ReadOnlySpan<char> text, out int major, out int minor, out int patch, out string prerelease, out string buildMetadata)
    {
        major = default;
        minor = default;
        patch = default;
        prerelease = null;
        buildMetadata = null;

        if (text.IsEmpty)
        {
            return false;
        }

        var buildIndex = text.IndexOf('+');
        if (buildIndex >= 0)
        {
            buildMetadata = text[(buildIndex + 1)..].ToString();
            if (string.IsNullOrEmpty(buildMetadata) || !IsValidBuildMetadata(buildMetadata.AsSpan()))
            {
                return false;
            }

            text = text[..buildIndex];
        }

        var prereleaseIndex = text.IndexOf('-');
        if (prereleaseIndex >= 0)
        {
            prerelease = text[(prereleaseIndex + 1)..].ToString();
            if (string.IsNullOrEmpty(prerelease) || !IsValidPrerelease(prerelease.AsSpan()))
            {
                return false;
            }

            text = text[..prereleaseIndex];
        }

        return TryReadCore(text, out major, out minor, out patch);
    }

    private static bool TryReadCore(ReadOnlySpan<char> text, out int major, out int minor, out int patch)
    {
        major = default;
        minor = default;
        patch = default;

        var firstDot = text.IndexOf('.');
        if (firstDot <= 0)
        {
            return false;
        }

        var secondDot = text[(firstDot + 1)..].IndexOf('.');
        if (secondDot <= 0)
        {
            return false;
        }

        secondDot += firstDot + 1;

        return text[(secondDot + 1)..].IndexOf('.') < 0
            && TryParseNumericIdentifier(text[..firstDot], out major)
            && TryParseNumericIdentifier(text[(firstDot + 1)..secondDot], out minor)
            && TryParseNumericIdentifier(text[(secondDot + 1)..], out patch);
    }

    private static bool TryParseNumericIdentifier(ReadOnlySpan<char> text, out int value)
    {
        value = default;

        if (text.IsEmpty || (text.Length > 1 && text[0] == '0'))
        {
            return false;
        }

        for (var i = 0; i < text.Length; i++)
        {
            if (!char.IsDigit(text[i]))
            {
                return false;
            }
        }

        return int.TryParse(text, out value);
    }

    private static bool IsValidDotSeparatedIdentifiers(ReadOnlySpan<char> value, bool validateNumericLeadingZeroes)
    {
        while (!value.IsEmpty)
        {
            var dotIndex = value.IndexOf('.');
            var identifier = dotIndex < 0 ? value : value[..dotIndex];

            if (!IsValidIdentifier(identifier, validateNumericLeadingZeroes))
            {
                return false;
            }

            if (dotIndex < 0)
            {
                return true;
            }

            value = value[(dotIndex + 1)..];
        }

        return false;
    }

    private static bool IsValidIdentifier(ReadOnlySpan<char> value, bool validateNumericLeadingZeroes)
    {
        if (value.IsEmpty)
        {
            return false;
        }

        var isNumeric = true;
        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];
            if (char.IsDigit(c))
            {
                continue;
            }

            isNumeric = false;
            if (!IsAsciiLetter(c) && c != '-')
            {
                return false;
            }
        }

        return !validateNumericLeadingZeroes || !isNumeric || value.Length == 1 || value[0] != '0';
    }

    private static bool IsAsciiLetter(char c)
    {
        return c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
    }
}
