using System;
using Cuemon;
using Cuemon.Configuration;

namespace Codebelt.Extensions.Asp.Versioning;

/// <summary>
/// Provides programmatic configuration for version conversion operations.
/// </summary>
/// <remarks>
/// This class is used to configure how .NET <see cref="Version"/> objects are converted to semantic API versions.
/// </remarks>
/// <seealso cref="IValidatableParameterObject"/>
public class VersionConversionOptions : IValidatableParameterObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VersionConversionOptions"/> class.
    /// </summary>
    /// <remarks>
    /// The following table shows the initial property values for an instance of <see cref="VersionConversionOptions"/>.
    /// <list type="table">
    ///     <listheader>
    ///         <term>Property</term>
    ///         <description>Initial Value</description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="IncludeRevision"/></term>
    ///         <description><c>false</c></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="RevisionIdentifier"/></term>
    ///         <description><c>revision</c></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public VersionConversionOptions()
    {
        RevisionIdentifier = nameof(Version.Revision).ToLowerInvariant();
    }

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="Version.Revision"/> component should be included in the converted semantic version.
    /// </summary>
    /// <value><c>true</c> if the revision component should be included; otherwise, <c>false</c>.</value>
    public bool IncludeRevision { get; set; }

    /// <summary>
    /// Gets or sets the identifier to use for the revision component when included in the semantic version build metadata.
    /// </summary>
    /// <value>The identifier used for the revision build metadata. The default value is <c>revision</c>.</value>
    public string RevisionIdentifier { get; set; }

    /// <summary>
    /// Validates the current state of the options.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="IncludeRevision"/> is <c>true</c> and <see cref="RevisionIdentifier"/> is <c>null</c>, empty, or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="RevisionIdentifier"/> is not a valid semantic version build metadata identifier.</exception>
    public void ValidateOptions()
    {
        Validator.ThrowIfInvalidState(IncludeRevision && string.IsNullOrWhiteSpace(RevisionIdentifier));
        Validator.ThrowIfInvalidState(IncludeRevision && !SemanticApiVersionParser.IsValidBuildMetadata(RevisionIdentifier.AsSpan()));
    }
}
