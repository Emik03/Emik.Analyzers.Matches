// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>Determines the result of trying to capture a string.</summary>
public enum RegexStatus
{
    /// <summary>Indicates that the capture succeeded.</summary>
    Passed,

    /// <summary>Indicates that the capture failed.</summary>
    Failed,

    /// <summary>Indicates that the capture cannot be performed.</summary>
    Invalid,

    /// <summary>Indicates that the capture timed out.</summary>
    Timeout,
}
