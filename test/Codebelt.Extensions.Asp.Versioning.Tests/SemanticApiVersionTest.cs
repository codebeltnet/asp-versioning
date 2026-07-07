using System;
using System.Linq;
using System.Reflection;
using Asp.Versioning;
using Codebelt.Extensions.Xunit;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning
{
    public class SemanticApiVersionTest : Test
    {
        public SemanticApiVersionTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void SemanticApiVersion_ShouldExposeSemanticVersionParts()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha.1", "build.5");

            Assert.Equal(1, sut.MajorVersion);
            Assert.Equal(2, sut.MinorVersion);
            Assert.Equal(3, sut.PatchVersion);
            Assert.Equal("alpha.1", sut.Prerelease);
            Assert.Equal("build.5", sut.BuildMetadata);
            Assert.Equal("1.2.3-alpha.1+build.5", sut.ToString());
        }

        [Theory]
        [InlineData("1.2.3+build.1", "1.2.3+build.2")]
        [InlineData("1.2.3-alpha+build.1", "1.2.3-alpha+build.2")]
        public void CompareTo_ShouldReturnZero_WhenOnlyBuildMetadataDiffers(string left, string right)
        {
            var sut1 = SemanticApiVersionParser.Default.Parse(left);
            var sut2 = SemanticApiVersionParser.Default.Parse(right);

            Assert.Equal(0, sut1.CompareTo(sut2));
            Assert.Equal(0, sut2.CompareTo(sut1));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenOnlyBuildMetadataDiffers()
        {
            var sut1 = SemanticApiVersionParser.Default.Parse("1.2.3+build.1");
            var sut2 = SemanticApiVersionParser.Default.Parse("1.2.3+build.2");

            Assert.NotEqual(sut1, sut2);
            Assert.NotEqual(sut1.GetHashCode(), sut2.GetHashCode());
        }

        [Fact]
        public void Equals_ShouldReturnExpectedResult_WhenComparingDifferentObjectTypes()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            Assert.True(sut.Equals(new SemanticApiVersion(1, 2, 3, "alpha", "build.5")));
            Assert.False(sut.Equals(new ApiVersion(1, 2)));
            Assert.False(sut.Equals("1.2.3-alpha+build.5"));
            Assert.False(sut.Equals(null));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenComparingPatchZeroReleaseSemanticVersionToApiVersion()
        {
            var semantic = new SemanticApiVersion(1, 0, 0);
            var standard = new ApiVersion(1, 0);

            Assert.False(semantic.Equals(standard));
            Assert.False(standard.Equals(semantic));
        }

        [Fact]
        public void Equals_ShouldReturnFalse_WhenComparingNonZeroPatchSemanticVersionToApiVersion()
        {
            var semantic = new SemanticApiVersion(1, 2, 3);
            var standard = new ApiVersion(1, 2);

            Assert.False(semantic.Equals(standard));
            Assert.False(standard.Equals(semantic));
        }

        [Fact]
        public void GetHashCode_ShouldReturnCachedHashCode_WhenCalledMoreThanOnce()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            var hashCode = sut.GetHashCode();

            Assert.Equal(hashCode, sut.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ShouldReturnCachedZeroHashCode_WhenZeroValueHasBeenComputed()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");
            var hashCodeField = typeof(SemanticApiVersion).GetField("_hashCode", BindingFlags.Instance | BindingFlags.NonPublic);
            var hashCodeComputedField = typeof(SemanticApiVersion).GetField("_hashCodeComputed", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(hashCodeField);
            Assert.NotNull(hashCodeComputedField);

            hashCodeField!.SetValue(sut, 0);
            hashCodeComputedField!.SetValue(sut, true);

            Assert.Equal(0, sut.GetHashCode());
        }

        [Fact]
        public void CompareTo_ShouldReturnOne_WhenOtherVersionIsNull()
        {
            var sut = new SemanticApiVersion(1, 2, 3);

            Assert.Equal(1, sut.CompareTo(null));
        }

        [Fact]
        public void CompareTo_ShouldUseBaseComparison_WhenOtherVersionIsNotSemanticApiVersion()
        {
            var sut = new SemanticApiVersion(2, 0, 0);

            Assert.True(sut.CompareTo(new ApiVersion(1, 0)) > 0);
        }

        [Theory]
        [InlineData("1.2.3", "2.0.0")]
        [InlineData("1.2.3", "1.3.0")]
        [InlineData("1.2.3", "1.2.4")]
        [InlineData("1.2.3-alpha", "1.2.3-alpha.1")]
        [InlineData("1.2.3-alpha.beta", "1.2.3-beta")]
        [InlineData("1.2.3-1", "1.2.3-alpha")]
        public void CompareTo_ShouldApplySemanticVersionPrecedence(string lower, string higher)
        {
            var sut1 = SemanticApiVersionParser.Default.Parse(lower);
            var sut2 = SemanticApiVersionParser.Default.Parse(higher);

            Assert.True(sut1.CompareTo(sut2) < 0);
            Assert.True(sut2.CompareTo(sut1) > 0);
        }

        [Theory]
        [InlineData("1.2.3-alpha", "1.2.3")]
        [InlineData("1.2.3-alpha.1", "1.2.3-alpha.beta")]
        [InlineData("1.2.3-alpha.1", "1.2.3-alpha.2")]
        [InlineData("1.2.3-alpha.2", "1.2.3-alpha.10")]
        [InlineData("1.2.3-alpha.99999999999999999999", "1.2.3-alpha.100000000000000000000")]
        public void CompareTo_ShouldApplySemanticVersionPrereleasePrecedence(string lower, string higher)
        {
            var sut1 = SemanticApiVersionParser.Default.Parse(lower);
            var sut2 = SemanticApiVersionParser.Default.Parse(higher);

            Assert.True(sut1.CompareTo(sut2) < 0);
            Assert.True(sut2.CompareTo(sut1) > 0);
        }

        [Theory]
        [InlineData("1.2.3", 1, 2, 3, null, null)]
        [InlineData("1.2.3-alpha.1", 1, 2, 3, "alpha.1", null)]
        [InlineData("1.2.3+build.01", 1, 2, 3, null, "build.01")]
        [InlineData("1.2.3-alpha.1+build.5", 1, 2, 3, "alpha.1", "build.5")]
        [InlineData("1.2.3-ALPHA+BUILD", 1, 2, 3, "ALPHA", "BUILD")]
        [InlineData("1.2.3-alpha-1+build-5", 1, 2, 3, "alpha-1", "build-5")]
        public void Parse_ShouldReturnSemanticApiVersion_WhenVersionIsValid(string version, int major, int minor, int patch, string prerelease, string buildMetadata)
        {
            var sut = Assert.IsType<SemanticApiVersion>(SemanticApiVersionParser.Default.Parse(version));

            Assert.Equal(major, sut.MajorVersion);
            Assert.Equal(minor, sut.MinorVersion);
            Assert.Equal(patch, sut.PatchVersion);
            Assert.Equal(prerelease, sut.Prerelease);
            Assert.Equal(buildMetadata, sut.BuildMetadata);
        }

        [Theory]
        [InlineData("")]
        [InlineData("1")]
        [InlineData("1.2")]
        [InlineData("1..3")]
        [InlineData("1.2.")]
        [InlineData("1.2.a")]
        [InlineData("1.2.3.4")]
        [InlineData("01.2.3")]
        [InlineData("1.02.3")]
        [InlineData("1.2.03")]
        [InlineData("1.2.3-")]
        [InlineData("1.2.3-alpha.")]
        [InlineData("1.2.3-alpha..1")]
        [InlineData("1.2.3-alpha.01")]
        [InlineData("1.2.3-alpha_1")]
        [InlineData("1.2.3+")]
        [InlineData("1.2.3+build.")]
        [InlineData("1.2.3+build..1")]
        [InlineData("1.2.3+build_1")]
        [InlineData("1.2.3+build+1")]
        [InlineData("2147483648.2.3")]
        public void TryParse_ShouldReturnFalse_WhenVersionIsInvalid(string version)
        {
            var result = SemanticApiVersionParser.Default.TryParse(version.AsSpan(), out var apiVersion);

            Assert.False(result);
            Assert.Null(apiVersion);
        }

        [Fact]
        public void Parse_ShouldThrowFormatException_WhenVersionIsInvalid()
        {
            var sut = Assert.Throws<FormatException>(() => SemanticApiVersionParser.Default.Parse("1..2"));

            Assert.Equal("The specified API version is not a valid semantic version.", sut.Message);
        }

        [Theory]
        [InlineData(-1, 0, 0, "major")]
        [InlineData(1, -1, 0, "minor")]
        [InlineData(1, 0, -1, "patch")]
        public void SemanticApiVersion_ShouldThrowArgumentOutOfRangeException_WhenVersionPartIsNegative(int major, int minor, int patch, string parameterName)
        {
            var sut = Assert.Throws<ArgumentOutOfRangeException>(() => new SemanticApiVersion(major, minor, patch));

            Assert.Equal(parameterName, sut.ParamName);
        }

        [Theory]
        [InlineData("alpha.01", null, "prerelease")]
        [InlineData(null, "build_1", "buildMetadata")]
        public void SemanticApiVersion_ShouldThrowArgumentException_WhenIdentifierIsInvalid(string prerelease, string buildMetadata, string parameterName)
        {
            var sut = Assert.Throws<ArgumentException>(() => new SemanticApiVersion(1, 2, 3, prerelease, buildMetadata));

            Assert.Equal(parameterName, sut.ParamName);
        }

        [Theory]
        [InlineData(null, "1.2.3-alpha+build.5")]
        [InlineData("G", "1.2.3-alpha+build.5")]
        [InlineData("F", "1.2.3-alpha+build.5")]
        [InlineData("FF", "1.2.3-alpha+build.5")]
        [InlineData("g", "1.2.3-alpha+build.5")]
        [InlineData("V", "1")]
        [InlineData("M", "1")]
        [InlineData("v", "2")]
        [InlineData("m", "2")]
        [InlineData("VV", "1.2")]
        [InlineData("VVV", "1")]
        [InlineData("VVVV", "1.2-alpha")]
        [InlineData("P", "3")]
        [InlineData("S", "alpha")]
        [InlineData("R", "alpha")]
        [InlineData("B", "build.5")]
        [InlineData("VV'-'S'+'B", "1.2-alpha+build.5")]
        [InlineData("VV\"-\"S\"+\"B", "1.2-alpha+build.5")]
        [InlineData("V'.'v", "1.2")]
        [InlineData("V'.'v'.'P", "1.2.3")]
        [InlineData("M'.'m'.'P'-'R", "1.2.3-alpha")]
        [InlineData("VVVV'+'B", "1.2-alpha+build.5")]
        public void ToString_ShouldReturnFormattedVersion_WhenFormatIsSupported(string format, string expected)
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            Assert.Equal(expected, sut.ToString(format));
        }

        [Theory]
        [InlineData("S", "")]
        [InlineData("B", "")]
        [InlineData("G", "1.2.3")]
        [InlineData("VVVV", "1.2")]
        public void ToString_ShouldOmitOptionalParts_WhenSemanticVersionHasNoOptionalIdentifiers(string format, string expected)
        {
            var sut = new SemanticApiVersion(1, 2, 3);

            Assert.Equal(expected, sut.ToString(format));
        }

        [Theory]
        [InlineData("Q")]
        [InlineData("VVVVV")]
        [InlineData("VV'")]
        public void ToString_ShouldThrowFormatException_WhenFormatIsUnsupported(string format)
        {
            var sut = new SemanticApiVersion(1, 2, 3);

            Assert.Throws<FormatException>(() => sut.ToString(format));
        }

        [Fact]
        public void SemanticApiVersionFormatter_ShouldReturnExpectedFormatProvider()
        {
            var sut = new SemanticApiVersionFormatter();
            var provider = new SemanticApiVersionFormatterProvider(sut);

            Assert.Same(sut, SemanticApiVersionFormatter.GetInstance(sut));
            Assert.Same(sut, SemanticApiVersionFormatter.GetInstance(provider));
            Assert.Same(sut, sut.GetFormat(typeof(ICustomFormatter)));
            Assert.Same(sut, sut.GetFormat(typeof(SemanticApiVersionFormatter)));
            Assert.Null(sut.GetFormat(null));
            Assert.Null(sut.GetFormat(typeof(string)));
        }

        [Fact]
        public void SemanticApiVersionFormatter_ShouldUseDefaultFormatting_WhenArgumentIsNotSemanticApiVersion()
        {
            var sut = new SemanticApiVersionFormatter();

            Assert.Equal(string.Empty, sut.Format(null, null, null));
            Assert.Equal("2A", sut.Format("X", 42, null));
            Assert.Equal("text", sut.Format("ignored", "text", null));
        }

        [Fact]
        public void TryFormat_ShouldWriteFormattedVersion_WhenDestinationIsLargeEnough()
        {
            Span<char> destination = stackalloc char[32];
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            var result = sut.TryFormat(destination, out var charsWritten, "G", null);

            Assert.True(result);
            Assert.Equal(19, charsWritten);
            Assert.Equal("1.2.3-alpha+build.5", destination[..charsWritten].ToString());
        }

        [Fact]
        public void TryFormat_ShouldReturnFalse_WhenDestinationIsTooSmall()
        {
            Span<char> destination = stackalloc char[4];
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            var result = sut.TryFormat(destination, out var charsWritten, "G", null);

            Assert.False(result);
            Assert.Equal(0, charsWritten);
        }

        [Theory]
        [InlineData(1, 0, 1, "v1")]
        [InlineData(1, 2, 0, "v1")]
        [InlineData(1, 2, 3, "v1")]
        public void ToString_ShouldReturnMajorVersionGroupName_WhenUsingRestfulApiExplorerFormat(int major, int minor, int patch, string expected)
        {
            var sut = new SemanticApiVersion(major, minor, patch, "alpha", "build.5");

            Assert.Equal(expected, sut.ToString("'v'VVV"));
        }

        [Fact]
        public void SemanticApiVersionAttribute_ShouldDefineSemanticApiVersionMetadata()
        {
            var sut = new SemanticApiVersionAttribute("1.2.3-alpha+build.5");
            var version = Assert.IsType<SemanticApiVersion>(sut.Versions.Single());

            Assert.Equal("1.2.3-alpha+build.5", version.ToString());
        }

        [Fact]
        public void SemanticApiVersionAttribute_ShouldDefineSemanticApiVersionMetadata_WhenUsingVersionParts()
        {
            var sut = new SemanticApiVersionAttribute(1, 2, 3, "alpha", "build.5");
            var version = Assert.IsType<SemanticApiVersion>(sut.Versions.Single());

            Assert.Equal("1.2.3-alpha+build.5", version.ToString());
        }

        [Fact]
        public void MapToSemanticApiVersionAttribute_ShouldDefineSemanticApiVersionMetadata()
        {
            var sut = new MapToSemanticApiVersionAttribute(1, 2, 3, "alpha", "build.5");
            var version = Assert.IsType<SemanticApiVersion>(sut.Versions.Single());

            Assert.Equal("1.2.3-alpha+build.5", version.ToString());
        }

        [Fact]
        public void MapToSemanticApiVersionAttribute_ShouldDefineSemanticApiVersionMetadata_WhenUsingString()
        {
            var sut = new MapToSemanticApiVersionAttribute("1.2.3-alpha+build.5");
            var version = Assert.IsType<SemanticApiVersion>(sut.Versions.Single());

            Assert.Equal("1.2.3-alpha+build.5", version.ToString());
        }

        [Fact]
        public void FromVersion_ShouldThrowArgumentNullException_WhenVersionIsNull()
        {
            var sut = Assert.Throws<ArgumentNullException>(() => SemanticApiVersion.FromVersion(null));

            Assert.Equal("version", sut.ParamName);
        }

        [Fact]
        public void FromVersion_ShouldDefaultPatchToZero_WhenBuildComponentIsNegative()
        {
            var sut = SemanticApiVersion.FromVersion(new Version(1, 2));

            Assert.Equal(1, sut.MajorVersion);
            Assert.Equal(2, sut.MinorVersion);
            Assert.Equal(0, sut.PatchVersion);
            Assert.Null(sut.BuildMetadata);
            Assert.Equal("1.2.0", sut.ToString());
        }

        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(2, 0, 5)]
        [InlineData(10, 1, 0)]
        public void FromVersion_ShouldMapVersionComponents_WhenBuildComponentIsNonNegative(int major, int minor, int build)
        {
            var sut = SemanticApiVersion.FromVersion(new Version(major, minor, build));

            Assert.Equal(major, sut.MajorVersion);
            Assert.Equal(minor, sut.MinorVersion);
            Assert.Equal(build, sut.PatchVersion);
            Assert.Null(sut.BuildMetadata);
        }

        [Fact]
        public void FromVersion_ShouldNotIncludeBuildMetadata_WhenIncludeRevisionIsFalse()
        {
            var sut = SemanticApiVersion.FromVersion(new Version(1, 2, 3, 4));

            Assert.Equal("1.2.3", sut.ToString());
            Assert.Null(sut.BuildMetadata);
        }

        [Fact]
        public void FromVersion_ShouldIncludeRevisionInBuildMetadata_WhenIncludeRevisionIsTrue()
        {
            var sut = SemanticApiVersion.FromVersion(new Version(1, 2, 3, 4), o => o.IncludeRevision = true);

            Assert.Equal(1, sut.MajorVersion);
            Assert.Equal(2, sut.MinorVersion);
            Assert.Equal(3, sut.PatchVersion);
            Assert.Equal("revision.4", sut.BuildMetadata);
            Assert.Equal("1.2.3+revision.4", sut.ToString());
        }

        [Fact]
        public void FromVersion_ShouldUseCustomRevisionIdentifier_WhenConfigured()
        {
            var sut = SemanticApiVersion.FromVersion(new Version(1, 2, 3, 4), o =>
            {
                o.IncludeRevision = true;
                o.RevisionIdentifier = "build";
            });

            Assert.Equal("build.4", sut.BuildMetadata);
            Assert.Equal("1.2.3+build.4", sut.ToString());
        }

        [Fact]
        public void FromVersion_ShouldNotIncludeBuildMetadata_WhenRevisionIsNegativeAndIncludeRevisionIsTrue()
        {
            var sut = SemanticApiVersion.FromVersion(new Version(1, 2, 3), o => o.IncludeRevision = true);

            Assert.Equal("1.2.3", sut.ToString());
            Assert.Null(sut.BuildMetadata);
        }

        [Fact]
        public void FromVersion_ShouldThrowArgumentException_WhenSetupIsInvalid()
        {
            var sut = Assert.Throws<ArgumentException>(() => SemanticApiVersion.FromVersion(new Version(1, 2, 3, 4), o =>
            {
                o.IncludeRevision = true;
                o.RevisionIdentifier = null;
            }));

            Assert.Equal("setup", sut.ParamName);
        }

        [Fact]
        public void ToString_ShouldReturnDefaultFormat_WhenCalledWithoutParameters()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build");

            var result = sut.ToString();

            Assert.Equal("1.2.3-alpha+build", result);
        }

        [Fact]
        public void ToString_ShouldReturnFormattedVersion_WhenCalledWithFormatOnly()
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build");

            var result = sut.ToString("G");

            Assert.Equal("1.2.3-alpha+build", result);
        }

        [Fact]
        public void TryFormat_ShouldHandleEmptyFormatSpan()
        {
            Span<char> destination = stackalloc char[32];
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build");
            ReadOnlySpan<char> format = default;

            var result = sut.TryFormat(destination, out var charsWritten, format, null);

            Assert.True(result);
            Assert.Equal("1.2.3-alpha+build", destination[..charsWritten].ToString());
        }

        [Fact]
        public void TryFormat_ShouldReturnZeroCharsWritten_WhenCopyFails()
        {
            Span<char> destination = stackalloc char[5];
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build");
            ReadOnlySpan<char> format = default;

            var result = sut.TryFormat(destination, out var charsWritten, format, null);

            Assert.False(result);
            Assert.Equal(0, charsWritten);
        }

        private sealed class SemanticApiVersionFormatterProvider : IFormatProvider
        {
            private readonly SemanticApiVersionFormatter _formatter;

            public SemanticApiVersionFormatterProvider(SemanticApiVersionFormatter formatter)
            {
                _formatter = formatter;
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(SemanticApiVersionFormatter)
                    ? _formatter
                    : null;
            }
        }
    }
}
