// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>Determines how strictness of considering two functions equal.</summary>
enum Strictness
{
    /// <summary>The function must be invoked.</summary>
    Exact,

    /// <summary>The function must be invoked implicitly with a constraint.</summary>
    ImplicitlyAndConstrained,

    /// <summary>The function must be invoked implicitly without a constraint.</summary>
    ImplicitlyAndUnconstrained,

    /// <summary>The function must be invoked implicitly.</summary>
    Implicitly,
}
