using System;
using System.Linq;
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

        [Theory]
        [InlineData("1.2.3-alpha", "1.2.3")]
        [InlineData("1.2.3-alpha.1", "1.2.3-alpha.beta")]
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
        [InlineData("1.2.3.4")]
        [InlineData("01.2.3")]
        [InlineData("1.02.3")]
        [InlineData("1.2.03")]
        [InlineData("1.2.3-")]
        [InlineData("1.2.3-alpha.01")]
        [InlineData("1.2.3+")]
        [InlineData("1.2.3+build_1")]
        public void TryParse_ShouldReturnFalse_WhenVersionIsInvalid(string version)
        {
            var result = SemanticApiVersionParser.Default.TryParse(version.AsSpan(), out var apiVersion);

            Assert.False(result);
            Assert.Null(apiVersion);
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
        [InlineData("V", "1")]
        [InlineData("v", "2")]
        [InlineData("VV", "1.2")]
        [InlineData("VVV", "1")]
        [InlineData("VVVV", "1.2-alpha")]
        [InlineData("P", "3")]
        [InlineData("S", "alpha")]
        [InlineData("B", "build.5")]
        [InlineData("VV'-'S'+'B", "1.2-alpha+build.5")]
        public void ToString_ShouldReturnFormattedVersion_WhenFormatIsSupported(string format, string expected)
        {
            var sut = new SemanticApiVersion(1, 2, 3, "alpha", "build.5");

            Assert.Equal(expected, sut.ToString(format));
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
        public void MapToSemanticApiVersionAttribute_ShouldDefineSemanticApiVersionMetadata()
        {
            var sut = new MapToSemanticApiVersionAttribute(1, 2, 3, "alpha", "build.5");
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
    }
}
