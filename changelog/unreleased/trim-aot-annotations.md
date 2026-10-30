type: feature

The package now declares `IsAotCompatible` on net8.0+: every public constructor and `EtlPipeline` XML factory carries `[RequiresDynamicCode]` as well as `[RequiresUnreferencedCode]`, and the record type parameter carries `[DynamicallyAccessedMembers]`, so a trimmed or Native AOT application keeps the record's public properties and fields instead of silently writing and reading empty elements.
