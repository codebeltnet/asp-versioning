using System;
using Codebelt.Extensions.Xunit;
using Cuemon;
using Xunit;

namespace Codebelt.Extensions.Asp.Versioning
{
    public class VersionConversionOptionsTest : Test
    {
        public VersionConversionOptionsTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void VersionConversionOptions_ShouldHaveDefaultValues()
        {
            var sut = new VersionConversionOptions();

            Assert.False(sut.IncludeRevision);
            Assert.Equal("revision", sut.RevisionIdentifier);
        }

        [Fact]
        public void VersionConversionOptions_IncludeRevisionIsFalse_ShouldAllowInvalidRevisionIdentifier()
        {
            var sut = new VersionConversionOptions
            {
                IncludeRevision = false,
                RevisionIdentifier = null
            };

            sut.ValidateOptions();
            Validator.ThrowIfInvalidOptions(sut);
        }

        [Theory]
        [InlineData("build")]
        [InlineData("build.01")]
        [InlineData("build-1")]
        public void VersionConversionOptions_IncludeRevisionIsTrueAndRevisionIdentifierIsValid_ShouldPassValidation(string revisionIdentifier)
        {
            var sut = new VersionConversionOptions
            {
                IncludeRevision = true,
                RevisionIdentifier = revisionIdentifier
            };

            sut.ValidateOptions();
            Validator.ThrowIfInvalidOptions(sut);
        }

        [Fact]
        public void VersionConversionOptions_IncludeRevisionIsTrueAndRevisionIdentifierIsNull_ShouldThrowInvalidOperationException()
        {
            var sut1 = new VersionConversionOptions
            {
                IncludeRevision = true,
                RevisionIdentifier = null
            };
            var sut2 = Assert.Throws<InvalidOperationException>(() => sut1.ValidateOptions());
            var sut3 = Assert.Throws<ArgumentException>(() => Validator.ThrowIfInvalidOptions(sut1));

            Assert.Equal("Operation is not valid due to the current state of the object. (Expression 'IncludeRevision && string.IsNullOrWhiteSpace(RevisionIdentifier)')", sut2.Message);
            Assert.Equal("VersionConversionOptions are not in a valid state. (Parameter 'sut1')", sut3.Message);
            Assert.IsType<InvalidOperationException>(sut3.InnerException);
        }

        [Fact]
        public void VersionConversionOptions_IncludeRevisionIsTrueAndRevisionIdentifierIsEmpty_ShouldThrowInvalidOperationException()
        {
            var sut1 = new VersionConversionOptions
            {
                IncludeRevision = true,
                RevisionIdentifier = string.Empty
            };
            var sut2 = Assert.Throws<InvalidOperationException>(() => sut1.ValidateOptions());
            var sut3 = Assert.Throws<ArgumentException>(() => Validator.ThrowIfInvalidOptions(sut1));

            Assert.Equal("Operation is not valid due to the current state of the object. (Expression 'IncludeRevision && string.IsNullOrWhiteSpace(RevisionIdentifier)')", sut2.Message);
            Assert.Equal("VersionConversionOptions are not in a valid state. (Parameter 'sut1')", sut3.Message);
            Assert.IsType<InvalidOperationException>(sut3.InnerException);
        }

        [Fact]
        public void VersionConversionOptions_IncludeRevisionIsTrueAndRevisionIdentifierIsWhiteSpace_ShouldThrowInvalidOperationException()
        {
            var sut1 = new VersionConversionOptions
            {
                IncludeRevision = true,
                RevisionIdentifier = " "
            };
            var sut2 = Assert.Throws<InvalidOperationException>(() => sut1.ValidateOptions());
            var sut3 = Assert.Throws<ArgumentException>(() => Validator.ThrowIfInvalidOptions(sut1));

            Assert.Equal("Operation is not valid due to the current state of the object. (Expression 'IncludeRevision && string.IsNullOrWhiteSpace(RevisionIdentifier)')", sut2.Message);
            Assert.Equal("VersionConversionOptions are not in a valid state. (Parameter 'sut1')", sut3.Message);
            Assert.IsType<InvalidOperationException>(sut3.InnerException);
        }

        [Fact]
        public void VersionConversionOptions_IncludeRevisionIsTrueAndRevisionIdentifierIsInvalid_ShouldThrowInvalidOperationException()
        {
            var sut1 = new VersionConversionOptions
            {
                IncludeRevision = true,
                RevisionIdentifier = "build_1"
            };
            var sut2 = Assert.Throws<InvalidOperationException>(() => sut1.ValidateOptions());
            var sut3 = Assert.Throws<ArgumentException>(() => Validator.ThrowIfInvalidOptions(sut1));

            Assert.Equal("Operation is not valid due to the current state of the object. (Expression 'IncludeRevision && !SemanticApiVersionParser.IsValidBuildMetadata(RevisionIdentifier.AsSpan())')", sut2.Message);
            Assert.Equal("VersionConversionOptions are not in a valid state. (Parameter 'sut1')", sut3.Message);
            Assert.IsType<InvalidOperationException>(sut3.InnerException);
        }
    }
}
