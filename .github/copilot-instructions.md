# Copilot Coding Agent Instructions

## Repository Summary

`Wolfgang.Etl.Xml` is a shipping .NET library that provides XML extractors and loaders built on the `Wolfgang.Etl.Abstractions` ETL pattern. The public surface is four classes — `XmlSingleStreamExtractor<T>`, `XmlMultiStreamExtractor<T>`, `XmlSingleStreamLoader<T>`, `XmlMultiStreamLoader<T>` — plus the `XmlReport` progress type.

**Repository Type**: Shipping NuGet library (not a template)
**Target Frameworks (src)**: `net462`, `net481`, `netstandard2.0`, `net8.0`, `net10.0`
**Test Frameworks**: full matrix `net462`–`net481`, `netcoreapp3.1`, `net5.0`–`net10.0`
**Primary Language**: C# (LangVersion `latest`)
**Size**: One src project, one unit-test project, one benchmarks project, one examples project

## Build and Validation Instructions

### Prerequisites
- .NET 10.0 SDK (covers the full TFM matrix when paired with the older targeting packs)
- ReportGenerator tool (installed via `dotnet tool install -g dotnet-reportgenerator-globaltool`)
- DevSkim CLI (installed via `dotnet tool install --global Microsoft.CST.DevSkim.CLI`)

### Build Process

1. **Restore Dependencies** (always run first):
   ```powershell
   dotnet restore
   ```

2. **Build Solution**:
   ```powershell
   dotnet build --no-restore --configuration Release
   ```

3. **Run Tests with Coverage**:
   ```powershell
   # Find and test all test projects
   Get-ChildItem -Path ./tests -Filter '*Test*.csproj' -Recurse | ForEach-Object {
     dotnet test $_.FullName --no-build --configuration Release --collect:"XPlat Code Coverage" --results-directory "./TestResults"
   }
   ```

4. **Generate Coverage Reports**:
   ```powershell
   reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:"Html;TextSummary;MarkdownSummaryGithub;CsvSummary"
   ```

5. **Security Scanning**:
   ```powershell
   devskim analyze --source-code . -f text --output-file devskim-results.txt -E
   ```

### Critical Build Requirements
- **Code Coverage**: Minimum 90% line coverage required for all projects
- **Security Scanning**: DevSkim must pass with no errors
- **Build Configuration**: Always use Release configuration for CI
- **Test Pattern**: Test projects must match `*Test*.csproj` pattern in `/tests` folder

### Common Issues and Workarounds
- **Timeout Issues**: Coverage and security scans can take 5-10 minutes for larger projects
- **Coverage Threshold Failures**: If below 90%, the build will fail - this is by design
- **Missing Test Projects**: The workflow expects at least one test project in `/tests` folder
- **DevSkim False Positives**: Review `devskim-results.txt` for any security findings

## Project Layout and Architecture

### Standard Directory Structure
```
root/
├── MySolution.sln              # Solution file (create in root)
├── src/                        # Application projects
│   ├── MyApp/
│   │   └── MyApp.csproj
│   └── MyLib/
│       └── MyLib.csproj
├── tests/                      # Test projects (required)
│   ├── MyApp.Tests/
│   │   └── MyApp.Tests.csproj
│   └── MyLib.Tests/
│       └── MyLib.Tests.csproj
├── benchmarks/                 # Performance benchmarks (optional)
│   └── MyApp.Benchmarks/
│       └── MyApp.Benchmarks.csproj
├── examples/                   # Example projects (optional)
├── docs/                       # Documentation
└── .github/                    # GitHub configuration
```

### Key Configuration Files
- **`.editorconfig`**: Code style rules (C# file-scoped namespaces, var preferences, analyzer severity)
- **`.gitignore`**: Comprehensive .NET gitignore (Visual Studio, build artifacts, packages)
- **`CONTRIBUTING.md`**: Contribution guidelines
- **`CODE_OF_CONDUCT.md`**: Standard Contributor Covenant v2.0

### GitHub Integration
- **Workflows**: `.github/workflows/pr.yaml` - Comprehensive CI/CD pipeline
- **Issue Templates**: Bug reports (YAML) and feature requests (Markdown)
- **PR Template**: Structured pull request template with checklists
- **CODEOWNERS**: Default owner `@Chris-Wolfgang`, update usernames as needed
- **Dependabot**: Configured for NuGet packages in all project directories

### Continuous Integration Pipeline (`.github/workflows/pr.yaml`)
The workflow runs on pull requests to `main` branch and includes:

1. **Environment**: Ubuntu / Windows / macOS matrix with the .NET 10.0 SDK (older TFMs use targeting packs)
2. **Build Steps**: Checkout → Setup .NET → Restore → Build → Test → Coverage → Security
3. **Artifacts**: Coverage reports and DevSkim results uploaded
4. **Branch Protection**: Configured to require this workflow to pass before merging

### Branch Protection Configuration
Branch protection rules are configured by running the local PowerShell script `scripts/Setup-BranchRuleset.ps1`. The script prompts you to choose repository settings during setup.

**Single-Developer Configuration (Default):**
- No PR approvals required (you can merge your own PRs)
- Allows solo developers to merge their own PRs while still enforcing CI/CD checks

**Multi-Developer Configuration:**
- Requires 1+ approval before merging
- Requires code owner review

**All Configurations Include:**
- Require status checks to pass before merging
- Require branches to be up to date
- Require conversation resolution before merging
- Restrict deletions and block force pushes
- Require code scanning (CodeQL High+ severity)

**Branch Protection Setup Instructions:**
1. Install GitHub CLI (gh) from https://cli.github.com/
2. Authenticate: `gh auth login`
3. From PowerShell 7+ (for example, using `pwsh`), run the branch protection setup script:
   ```powershell
   pwsh -File ./scripts/Setup-BranchRuleset.ps1
   ```
4. When prompted by the script, choose single-developer or multi-developer settings

## Key Files and Locations

### Root Directory Files
- `README.md` - Basic template description (update for your project)
- `LICENSE` - MIT License
- `REPO-INSTRUCTIONS.md` - Template setup instructions (delete after setup)
- `.editorconfig` - Code style configuration
- `.gitignore` - .NET-specific gitignore

### GitHub Directory (`.github/`)
- `workflows/pr.yaml` - Main CI/CD pipeline
- `ISSUE_TEMPLATE/` - Bug report (YAML) and feature request templates
- `pull_request_template.md` - PR template with checklists
- `CODEOWNERS` - Code ownership rules
- `dependabot.yml` - Dependency update configuration

### Project Directories (Currently Empty in Template)
- `src/` - Application source code
- `tests/` - Unit and integration tests
- `benchmarks/` - Performance benchmarks
- `examples/` - Example usage projects
- `docs/` - Documentation (contains placeholder `index.html`)

## Agent Guidelines

### Trust These Instructions
This information has been validated against the template structure and GitHub workflows. **Only search for additional information if these instructions are incomplete or found to be incorrect.**

### When Working with This Template
1. **Creating New Projects**: Follow the structure outlined in `REPO-INSTRUCTIONS.md`
2. **Adding Dependencies**: Use `dotnet add package` commands
3. **Code Style**: Follow `.editorconfig` rules (file-scoped namespaces, explicit typing)
4. **Testing**: Ensure test projects follow `*Test*.csproj` naming convention
5. **Coverage**: Aim for >90% code coverage to pass CI
6. **Security**: Review DevSkim findings and address security concerns

### Validation Steps
Before submitting changes:
1. Run `dotnet restore && dotnet build --configuration Release`
2. Run tests with coverage collection
3. Verify coverage meets 90% threshold
4. Run DevSkim security scan
5. Ensure all GitHub Actions checks pass

This template provides a solid foundation for .NET projects with enterprise-grade CI/CD, security scanning, and development best practices built-in.
