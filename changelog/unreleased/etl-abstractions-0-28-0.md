type: fix

Updated `Wolfgang.Etl.Abstractions`, `Wolfgang.Etl.ErrorPolicies`, `Wolfgang.Etl.TestKit` and `Wolfgang.Etl.TestKit.Xunit` to 0.28.0. The update brings two behaviour fixes to pipelines built with this library: `EtlPipelineProgress.LoadedItemCount` is now the loader's own count of loaded records (it no longer over-reports when a loader skips records, discards one through an error policy or stops at `MaximumItemCount`), and when a run fails or is cancelled and the final progress report also throws, the caller now receives the run's own exception.

The multi-stream loader idempotency test now derives from the three-type-argument `IdempotentLoaderContractTests<TSut, TItem, TProgress>` (the two-argument base is obsolete) and no longer overrides `GetCurrentItemCount`.
