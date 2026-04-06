# Code Formatting

This repository uses `dotnet format` to enforce consistent C# code style.

## Prerequisites

The `dotnet format` command is **built into the .NET SDK** starting with .NET 6 and later. For full multi-target formatting (including the `net10.0` target), install the .NET 10.0 SDK or later so all target frameworks can be evaluated. If you only have the .NET 8.0 SDK installed, you can still run formatting by restricting to the `net8.0` target, for example: `dotnet format --framework net8.0`.

> **Note:** The standalone `dotnet-format` global tool was deprecated when `dotnet format` was integrated into the .NET 6 SDK in August 2021.

## For Developers

### Before Committing

Run the formatting script with PowerShell Core (`pwsh`) on any supported platform:

```powershell
pwsh ./scripts/format.ps1
```

Or check without making changes:

```powershell
pwsh ./scripts/format.ps1 -Check
```

### Manual Formatting

```bash
dotnet format
```

### Check Formatting (like CI does)

```bash
dotnet format --verify-no-changes
```

## Configuration

Code style rules are defined in `.editorconfig` at the repository root.

## CI/CD

Formatting is enforced locally before committing. Run the formatting script to ensure your code matches the expected style.

### Before Pushing

1. Run `pwsh ./scripts/format.ps1` locally
2. Review the changes
3. Commit and push the formatted code

## IDE Integration

Most IDEs automatically read `.editorconfig`:

- **Visual Studio**: Built-in support, formats on save (Tools > Options > Text Editor > C# > Code Style)
- **VS Code**: Install "EditorConfig for VS Code" extension
- **JetBrains Rider**: Built-in support

## Formatting Rules

Key style rules:
- **Indentation**: 4 spaces for C# (with `switch` case contents not additionally indented when inside a block, per `.editorconfig`), 2 for XML/JSON
- **Braces**: Opening brace on new line
- **Line endings**: LF (Unix style) for most files; PowerShell scripts (`*.ps1`) use CRLF as configured in `.editorconfig`
- **Trailing whitespace**: Removed
- **Using directives**: System namespaces first, sorted alphabetically
