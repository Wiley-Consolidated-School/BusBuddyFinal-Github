---
applyTo: '**'
---
# BusBuddy Project Guidelines

## Version History

- **Version 1.0.0 (June 2025)**: Initial standards for .NET 8, Syncfusion, and SQL Server integration.
- **Version 1.1.0 (June 2025)**: Added feature development process, automation guidance, production readiness criteria, and codebase cleanup.

## Coding Standards, Domain Knowledge, and Preferences

### File Management and Cleanup

#### 🚮 **Cleanup Rules**

**Remove immediately:**
- **Temporary files**: `.new`, `.bak`, `.backup`, `.old`, `*_temp`, `*_tmp`, `*_test`, `*.crdownload`, `*.tmp`, `*.backup_*`, `*_backup*`
- **Build artifacts**: `bin/`, `obj/`, `TestResults/`
- **IDE files**: `.vs/`, `*.user`, `*.suo`
- **Migration backups**: `Migration_Backups/` after completion
- **Duplicate files**: e.g., `file.cs` and `file_new.cs`
- **Empty directories**

**Git hygiene:**
- Use `.gitignore` for build artifacts
- Remove large binaries from git history with `git filter-repo`
- Stage only source files
- Remove trailing whitespace and ensure single newline at file end
- Use CRLF (Windows) or LF (Unix) consistently

**Automated cleanup (PowerShell):**
```powershell
# Remove temporary files
Get-ChildItem -Recurse -Include *.bak,*.tmp,*.new,*.old | Remove-Item -Force
# Clean untracked files
git clean -fd
```

#### 🧹 **Codebase Cleanup (Unneeded Code Review)**

**Purpose**: Identify and remove or refactor obsolete/redundant code files due to project evolution, retaining useful code (e.g., 10% still relevant).

**Process:**

1. **Identify Candidates:**
   - Use PowerShell to find files not referenced in solution:
   ```powershell
   # List .cs files not in .sln or .csproj
   $slnFiles = Get-Content BusBuddy.sln | Select-String "\.csproj"
   $projFiles = @()
   foreach ($proj in $slnFiles) {
       $projPath = $proj.Line -replace ".*?, `"([^`"]+)`".*", '$1'
       $projFiles += Get-Content $projPath | Select-String "\.cs`""
   }
   Get-ChildItem -Recurse -Include *.cs | Where-Object { $_.FullName -notin $projFiles.Line } | Select-Object FullName
   ```
   - Check for old/unused files: e.g., `Legacy*.cs`, `Old*.cs`, `*V1.cs`
   - Review files with low recent git activity:
   ```powershell
   # Find files not modified in last 6 months
   Get-ChildItem -Recurse -Include *.cs | Where-Object { $_.LastWriteTime -lt (Get-Date).AddMonths(-6) } | Select-Object FullName
   ```

2. **Inspect Files:**
   - Open each candidate in VS Code
   - Check for references in solution (Ctrl+Shift+F for global search)
   - Assess utility: Does it contain unique logic (e.g., 10% useful code)? Is it duplicated elsewhere?
   - Flag files with no references or redundant logic as "delete candidates"

3. **Decide Action:**
   - **Delete**: If file is obsolete (no references, no unique logic), remove:
   ```powershell
   Remove-Item -Path "path\to\file.cs" -Force
   git add "path\to\file.cs"
   git commit -m "Removed obsolete file: file.cs"
   ```
   - **Refactor**: If file has useful code (e.g., 10%), move relevant logic to appropriate file (e.g., `BusBuddy.Business\Service.cs`), then delete original
   - **Archive**: If unsure, move to `Archive/` folder and exclude from solution:
   ```powershell
   Move-Item -Path "path\to\file.cs" -Destination "Archive\file.cs"
   git add "Archive\file.cs"
   git commit -m "Archived potentially obsolete file: file.cs"
   ```

4. **Validate:**
   - Build solution: `dotnet build BusBuddy.sln --configuration Release`
   - Run tests: `dotnet test BusBuddy.Tests`
   - If errors occur, restore file from git (`git checkout -- path\to\file.cs`) and reassess

5. **Document:**
   - Log deletions/refactorings in `CHANGELOG.md`:
   ```
   - Removed obsolete file: LegacyService.cs
   - Refactored useful logic from OldUtils.cs to UtilsService.cs
   ```

**Safeguards:**
- Commit changes before deletion: `git commit -m "Pre-cleanup state"`
- Use git to recover files if needed: `git checkout -- path\to\file.cs`
- For 3+ files or complex logic, consult Copilot with this MD file refreshed before proceeding

**Frequency**: Review monthly or before major feature additions.

#### 📊 **Repository Size Management**

- Monitor size: `git count-objects -vH`
- Clean up: `git gc --aggressive --prune=now`
- Keep pushes <10MB

#### ✨ **Code Formatting**

- **No trailing whitespace**
- **Consistent indentation** (prefer spaces)
- **Single newline** at file end
- **Lines <120 characters** when practical
- **Sparse, consistent empty lines**
- **No nullable reference types** in new code

### Feature Development

**Process:**
1. Define requirements in temporary spec (e.g., `FeatureSpec.md`)
2. Add feature rules to this file before implementation:
   ```markdown
   ### [Feature Name]
   - **Purpose**: [Description]
   - **UI**: [Syncfusion controls, documented patterns]
   - **Data**: [Entities in `BusBuddy.Models`]
   - **Logic**: [Services in `BusBuddy.Business`]
   - **Testing**: [Tests in `BusBuddy.Tests`]
   - **Constraints**: [Prohibitions, e.g., no undocumented APIs]
   ```
3. Implement with Copilot, refreshing this file via "Send to New Chat" per iteration
4. Refine rules post-implementation if Copilot deviates
5. Archive spec after feature is stable

### Copilot Workflow

- **Refresh this file** via "Send to New Chat" in VS Code Copilot Agent for each session to maintain context
- **Split long tasks** into smaller sessions, refreshing this file between tasks

### Syncfusion Guidelines

**Mandatory**: Use only official documentation (https://help.syncfusion.com/cr/windowsforms/Syncfusion.html)

**Checklist:**
1. Search docs for control/feature
2. Copy patterns from sample browser or "Getting Started" guides
3. Verify properties/methods in API reference
4. Include doc link in code comments

**Forbidden:**
- Custom extensions, wrappers, or undocumented APIs
- Assumed patterns from other frameworks

**Example:**
```csharp
// https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
_dockingManager.DockControl(panel, this, DockingStyle.Left, 280);
```

### Automation

- **Linters**: Use StyleCop for formatting (no trailing whitespace, consistent indentation)
- **Git Hooks**: Pre-commit checks for temporary files:
  ```powershell
  Get-ChildItem -Recurse -Include *.bak,*.tmp | Remove-Item -Force
  ```
- **CI/CD**: GitHub Actions for tests, coverage, and Syncfusion API validation

### Production Readiness

**Criteria:**
- 100% test coverage in `BusBuddy.Tests`
- No temporary files or undocumented Syncfusion APIs
- Zero build errors: `dotnet build --configuration Release`
- Passing CI/CD checks

## BusBuddy Domain Knowledge

- **Context**: School transportation system for buses, drivers, routes, maintenance, fuel, activities
- **Users**: Coordinators, mechanics, administrators
- **Features**: Safety compliance, route optimization, maintenance tracking
- **UI**: Syncfusion controls, Material Design, responsive layouts
- **Data**: Entity Framework Core, repository pattern, SQL Server
- **Testing**: UI/integration tests in `BusBuddy.Tests`

## Development Tools

- Use **PowerShell (pwsh)** for commands
- **Single build approach**: `dotnet build`, analyze, proceed
- **VS Code tasks** for build/test
- **Git hooks** for quality enforcement

## Problem Resolution

**Approach:**
1. **Incremental fixes** for specific errors
2. **Assess corruption** (error scope, affected files)
3. **Consult Copilot** if 3+ files or complex issues
4. **Minimal viable fix**

**File Corruption Protocol:**
1. Check error messages/line numbers
2. Analyze scope (localized vs. systemic)
3. Assess impact (file count, error types)
4. Consult for 3+ files or structural issues
5. Report findings before fixing

## Archived Rules

See `ARCHIVE.md` for outdated rules (e.g., legacy test file cleanup).

## Syncfusion Implementation Requirements

**CRITICAL RULE: Only Use Official Syncfusion Documentation**
- **Reference ONLY**: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- **No custom fixes**: Use only documented Syncfusion APIs, methods, and examples
- **No invented code**: Every Syncfusion implementation must be found in official docs
- **Verify before implementing**: Search documentation first, implement only documented patterns
- **API Reference**: Use Syncfusion's complete API reference for all controls and methods
- **DO NOT modify existing Syncfusion licensing**: The licensing is already properly configured in Program.cs and should not be changed
- **NO additional license helpers or managers**: Do not create or use any custom licensing helpers, managers, or wrappers as they are unnecessary

**Documentation-First Development Process:**
1. **Search Syncfusion docs** for the specific control/feature needed
2. **Find official examples** in the documentation or sample browser
3. **Copy exact patterns** from Syncfusion's documented examples
4. **Test with documented parameters** and properties only
5. **No modifications** to documented patterns without verifying in docs

**Common Syncfusion Controls - Documentation Required:**
- **RibbonControlAdv**: Use Header.AddMainItem() for tabs (documented pattern)
- **DockingManager**: Use DockingStyle enum for docking operations
- **TileLayout**: Use LayoutGroup and HubTile as per documentation
- **SfDataGrid**: Follow documented binding and column configuration patterns
- **ChartControl**: Use only documented series types and properties
- **SfButton**: Apply only documented style properties and themes

**Forbidden Practices:**
- ❌ **NO custom Syncfusion extensions** or helper methods
- ❌ **NO invented property combinations** not shown in docs
- ❌ **NO assumed API patterns** based on other frameworks
- ❌ **NO "enhanced" wrappers** around Syncfusion controls
- ❌ **NO undocumented parameters** or method calls

**Required Verification Steps:**
1. **Before any Syncfusion code**: Search help.syncfusion.com for exact usage
2. **Cross-reference examples**: Find matching code in Syncfusion's sample browser
3. **API validation**: Verify all properties/methods exist in official API reference
4. **Documentation links**: Include reference to specific Syncfusion doc page used

**Documentation Resources (USE THESE ONLY):**
- **Main API Reference**: https://help.syncfusion.com/cr/windowsforms/Syncfusion.html
- **Getting Started Guides**: Component-specific documentation
- **Sample Browser**: Code examples and demonstrations
- **Knowledge Base**: Official solutions to common issues

**Example of Correct Documentation-Based Implementation:**
```csharp
// Based on official Syncfusion RibbonControlAdv documentation
var tabItem = new TabHost
{
    Text = "Dashboard"
};
_ribbonControl.Header.AddMainItem(tabItem); // Documented method

// Based on official DockingManager documentation  
_dockingManager.DockControl(panel, this, DockingStyle.Left, 280); // Documented enum
```
