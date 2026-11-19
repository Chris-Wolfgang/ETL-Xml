type: internal

Updated `Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies`, `Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit` to 0.28.0.

The multi-stream loader idempotency test now derives from the three-type-argument `IdempotentLoaderContractTests<TSut, TItem, TProgress>` (the two-argument base is obsolete) and no longer overrides `GetCurrentItemCount`.
