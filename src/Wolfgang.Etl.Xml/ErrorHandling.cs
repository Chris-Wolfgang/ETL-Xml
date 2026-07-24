namespace Wolfgang.Etl.Xml;

/// <summary>Controls how deserialization errors are handled during extraction.</summary>
public enum ErrorHandling
{
    /// <summary>Throw on the first deserialization error (default).</summary>
    Throw,

    /// <summary>
    /// Capture the error in <c>Errors</c> and continue processing remaining records where possible.
    /// In the single-stream case a record is read as a self-contained element before deserialization,
    /// so a type/mapping failure is isolated and the next sibling element is still reachable; XML that
    /// is not well-formed cannot be skipped past and still aborts.
    /// </summary>
    CaptureAndContinue,

    /// <summary>Skip the error silently, logging a warning if a logger is configured.</summary>
    SkipAndLog,
}
