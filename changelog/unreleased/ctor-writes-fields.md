type: internal

The options constructor assigns the stage's backing fields directly instead of going through the deprecated setters, so the `CS0618` suppressions that covered those writes are gone. Neither loader's deprecated setter validates, so no record-side guard was needed. (#347)
