// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage(
    "AsyncUsage",
    "AsyncFixer01:Unnecessary async/await usage",
    Justification = "All tests have been written with a certain pattern where this diagnostic fires, and makes it less valuable (right now).")]
