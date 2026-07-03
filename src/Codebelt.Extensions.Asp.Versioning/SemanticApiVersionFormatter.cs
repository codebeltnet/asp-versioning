using System;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Formats instances of <see cref="SemanticApiVersion"/>.
/// </summary>
/// <seealso cref="ICustomFormatter" />
/// <seealso cref="IFormatProvider" />
public sealed class SemanticApiVersionFormatter : IFormatProvider, ICustomFormatter
{
    private static readonly SemanticApiVersionFormatter Default = new();

    /// <summary>
    /// Gets a semantic API version formatter from the specified <paramref name="formatProvider"/>.
    /// </summary>
    /// <param name="formatProvider">The format provider to inspect.</param>
    /// <returns>A semantic API version formatter.</returns>
    public static SemanticApiVersionFormatter GetInstance(IFormatProvider formatProvider)
    {
        return formatProvider is SemanticApiVersionFormatter formatter
            ? formatter
            : formatProvider?.GetFormat(typeof(SemanticApiVersionFormatter)) is SemanticApiVersionFormatter customFormatter
                ? customFormatter
                : Default;
    }

    /// <inheritdoc />
    public object GetFormat(Type formatType)
    {
        return typeof(ICustomFormatter).Equals(formatType)
            || (formatType != null && GetType().GetTypeInfo().IsAssignableFrom(formatType.GetTypeInfo()))
                ? this
                : null;
    }

    /// <inheritdoc />
    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
        return arg is SemanticApiVersion version
            ? FormatVersion(format, version)
            : GetDefaultFormat(format, arg, formatProvider);
    }

    private static string FormatVersion(string format, SemanticApiVersion version)
    {
        if (string.IsNullOrEmpty(format) || string.Equals(format, "G", StringComparison.OrdinalIgnoreCase))
        {
            return FormatFull(version);
        }

        return format switch
        {
            "F" or "FF" => FormatFull(version),
            "V" or "M" => version.MajorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
            "v" or "m" => version.MinorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
            "VV" => FormatMajorMinor(version),
            "VVV" => version.MajorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
            "VVVV" => FormatMajorMinorWithPrerelease(version),
            "P" => version.PatchVersion.ToString(CultureInfo.InvariantCulture),
            "S" or "R" => version.Prerelease ?? string.Empty,
            "B" => version.BuildMetadata ?? string.Empty,
            _ => FormatComposite(format, version)
        };
    }

    private static string FormatMajorMinor(SemanticApiVersion version)
    {
        return string.Create(CultureInfo.InvariantCulture, $"{version.MajorVersion.GetValueOrDefault()}.{version.MinorVersion.GetValueOrDefault()}");
    }

    private static string FormatMajorMinorWithPrerelease(SemanticApiVersion version)
    {
        return AppendPrerelease(FormatMajorMinor(version), version);
    }

    private static string AppendPrerelease(string text, SemanticApiVersion version)
    {
        return string.IsNullOrEmpty(version.Prerelease)
            ? text
            : $"{text}-{version.Prerelease}";
    }

    private static string FormatComposite(string format, SemanticApiVersion version)
    {
        var text = new StringBuilder();

        for (var i = 0; i < format.Length; i++)
        {
            var c = format[i];
            if (c is '\'' or '"')
            {
                i = AppendQuotedLiteral(format, i, text);
                continue;
            }

            if (c == 'V')
            {
                var length = CountRepeated(format, i, 'V');
                text.Append(FormatVersionToken(length, version));
                i += length - 1;
                continue;
            }

            text.Append(c switch
            {
                'v' => version.MinorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
                'S' => version.Prerelease ?? string.Empty,
                'B' => version.BuildMetadata ?? string.Empty,
                _ => throw new FormatException($"The '{format}' format is not supported.")
            });
        }

        return text.ToString();
    }

    private static string FormatVersionToken(int length, SemanticApiVersion version)
    {
        return length switch
        {
            1 => version.MajorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
            2 => FormatMajorMinor(version),
            3 => version.MajorVersion.GetValueOrDefault().ToString(CultureInfo.InvariantCulture),
            4 => FormatMajorMinorWithPrerelease(version),
            _ => throw new FormatException($"The 'V' format token length '{length}' is not supported.")
        };
    }

    private static int CountRepeated(string format, int startIndex, char value)
    {
        var length = 1;
        for (var i = startIndex + 1; i < format.Length && format[i] == value; i++)
        {
            length++;
        }

        return length;
    }

    private static int AppendQuotedLiteral(string format, int startIndex, StringBuilder text)
    {
        var quote = format[startIndex];
        for (var i = startIndex + 1; i < format.Length; i++)
        {
            if (format[i] == quote)
            {
                return i;
            }

            text.Append(format[i]);
        }

        throw new FormatException($"The '{format}' format contains an unterminated quoted literal.");
    }

    private static string FormatFull(SemanticApiVersion version)
    {
        var text = new StringBuilder()
            .Append(version.MajorVersion.GetValueOrDefault())
            .Append('.')
            .Append(version.MinorVersion.GetValueOrDefault())
            .Append('.')
            .Append(version.PatchVersion);

        if (!string.IsNullOrEmpty(version.Prerelease))
        {
            text.Append('-').Append(version.Prerelease);
        }

        if (!string.IsNullOrEmpty(version.BuildMetadata))
        {
            text.Append('+').Append(version.BuildMetadata);
        }

        return text.ToString();
    }

    private static string GetDefaultFormat(string format, object arg, IFormatProvider formatProvider)
    {
        return arg == null
            ? format ?? string.Empty
            : !string.IsNullOrEmpty(format) && arg is IFormattable formattable
                ? formattable.ToString(format, formatProvider)
                : arg.ToString() ?? string.Empty;
    }
}
